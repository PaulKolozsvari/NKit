namespace NKit.Web.Service.RestApi
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.Logging;
    using NKit.Web.Client;
    using NKit.Web.Service.RestApi.Events;
    using Microsoft.AspNetCore.Http.Extensions;
    using System.Security.Claims;
    using NKit.Data;
    using Microsoft.Extensions.Options;
    using NKit.Utilities.SettingsFile.Default;
    using System.IO;
    using System.Reflection;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Utilities.Serialization;

    #endregion //Using Directives

    /// <summary>
    /// Must register HttpContextAccessor as a service in Startup.ConfigureServices of your app: services.AddHttpContextAccessor();
    /// </summary>
    /// <typeparam name="D"></typeparam>
    [ApiController]
    public class NKitWebApiController<D> : ControllerBase where D : NKitDbContextRepository
    {
        #region Constructors

        public NKitWebApiController(
            D context,
            IHttpContextAccessor httpContextAccessor, 
            IOptions<NKitWebApiControllerSettings> webApiOptions,
            IOptions<NKitDbContextRepositorySettings> databaseOptions,
            IOptions<NKitEmailCllientSettings> emailOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            ILogger logger)
        {
            DataValidator.ValidateObjectNotNull(context, nameof(context), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(httpContextAccessor, nameof(httpContextAccessor), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(webApiOptions, nameof(webApiOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(databaseOptions, nameof(databaseOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(emailOptions, nameof(emailOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitWebApiController<D>));
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webApiSettings = webApiOptions.Value;
            _databaseSettings = databaseOptions.Value;
            _emailSettings = emailOptions.Value;
            _loggingSettings = loggingOptions.Value;
            _logger = logger;
        }

        #endregion //Constructors

        #region Fields

        protected event OnBeforeGetEntitiesHandlerCore OnBeforeGetEntities;
        protected event OnAfterGetEntitiesHandlerCore OnAfterGetEntities;

        protected event OnBeforeGetEntityByIdHandlerCore OnBeforeGetEntityById;
        protected event OnAfterGetEntityByIdHandlerCore OnAfterGetEntityById;

        protected event OnBeforeGetEntitiesHandlerCore OnBeforeGetEntitiesByField;
        protected event OnAfterGetEntitiesHandlerCore OnAfterGetEntitiesByField;

        protected event OnBeforePutEntityHandlerCore OnBeforePut;
        protected event OnAfterPutEntityHandlerCore OnAfterPut;

        protected event OnBeforePostEntityHandlerCore OnBeforePost;
        protected event OnAfterPostEntityHandlerCore OnAfterPost;

        protected event OnBeforeDeleteEntityHandlerCore OnBeforeDelete;
        protected event OnAfterDeleteEntityHandlerCore OnAfterDelete;

        protected Nullable<Guid> _serviceInstanceId;

        protected readonly ILogger _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected IServiceProvider _serviceProvider;
        protected IHttpContextAccessor _httpContextAccessor;
        protected D _context;

        protected NKitWebApiControllerSettings _webApiSettings;
        protected NKitDbContextRepositorySettings _databaseSettings;
        protected NKitEmailCllientSettings _emailSettings;
        protected NKitLoggingSettings _loggingSettings;

        #endregion //Fields

        #region Methods

        protected void LogRequest(string actionName, string requestMessage)
        {
            if (!_webApiSettings.LogRequests && !_webApiSettings.LogRequestsInDatabaseNKitLogEntry)
            {
                return;
            }
            StringBuilder logMessageBuilder = new StringBuilder();
            logMessageBuilder.AppendLine($"Request Name: {actionName}");
            if (!string.IsNullOrEmpty(requestMessage))
            {
                logMessageBuilder.AppendLine($"Request Message:");
                logMessageBuilder.AppendLine(requestMessage);
            }
            AppendStatsToLogMessage(logMessageBuilder);
            string logMessage = logMessageBuilder.ToString();
            if (_logger != null && _webApiSettings.LogRequests)
            {
                _logger.LogInformation(logMessage);
            }
            if (_context != null && _webApiSettings.LogRequestsInDatabaseNKitLogEntry)
            {
                _context.LogWebActionActivityToNKitLogEntry(nameof(NKitWebApiController<D>), actionName, logMessage, new EventId(27, "Request"));
            }
        }

        protected void LogResponse(string actionName, string responseMessage)
        {
            if (!_webApiSettings.LogResponses && !_webApiSettings.LogResponsesInDatabaseNKitLogEntry)
            {
                return;
            }
            StringBuilder logMessageBuilder = new StringBuilder();
            logMessageBuilder.AppendLine($"Response Message: {actionName}");
            if (!string.IsNullOrEmpty(responseMessage))
            {
                logMessageBuilder.AppendLine($"Request Message:");
                logMessageBuilder.AppendLine(responseMessage);
            }
            AppendStatsToLogMessage(logMessageBuilder);
            string logMessage = logMessageBuilder.ToString();
            if (_logger != null && _webApiSettings.LogResponses)
            {
                _logger.LogInformation(logMessage);
            }
            if (_context != null && _webApiSettings.LogResponsesInDatabaseNKitLogEntry)
            {
                _context.LogWebActionActivityToNKitLogEntry(nameof(NKitWebApiController<D>), actionName, logMessage, new EventId(28, "Response"));
            }
        }

        private void AppendStatsToLogMessage(StringBuilder logMessage)
        {
            ThreadHelper.GetCurrentThreadCount(out int workerThreadsRunning, out int completionPortThreadsRunning);
            int totalThreadsRunning = ThreadHelper.GetTotalThreadsRunningCountInCurrentProcess();
            logMessage.AppendLine($"Request URI: {GetCurrentRequestUri()}");
            logMessage.AppendLine($"Request Verb: {_httpContextAccessor.HttpContext.Request.Method}");
            logMessage.AppendLine($"Service Instance ID: {_serviceInstanceId}");
            logMessage.AppendLine($"Worker Threads Running: {workerThreadsRunning}");
            logMessage.AppendLine($"Completion Port Threads Running: {completionPortThreadsRunning}");
            logMessage.AppendLine($"Total Threads Running: {totalThreadsRunning}");
            logMessage.AppendLine($"Current Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        }

        protected string GetServerHostName()
        {
            return _httpContextAccessor.HttpContext.Request.Host.Value;
        }

        protected string GetCurrentRequestUri()
        {
            //var request = _httpContextAccessor.HttpContext.Request;
            //UriBuilder uriBuilder = new UriBuilder();
            //uriBuilder.Scheme = request.Scheme;
            //uriBuilder.Host = request.Host.Host;
            //uriBuilder.Path = request.Path.ToString();
            //uriBuilder.Query = request.QueryString.ToString();
            //return uriBuilder.Uri.ToString();
            return _httpContextAccessor.HttpContext.Request.GetDisplayUrl(); //You need to have a using statement to include the GetDisplayUrl extendion method in your file: using Microsoft.AspNetCore.Http.Extensions
        }

        protected string GetCurrentRequestMethod()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        protected string GetAllHeadersFullString()
        {
            return _httpContextAccessor.HttpContext.Request.Headers.ToString();
        }

        protected string GetAllHeadersFormatted()
        {
            StringBuilder result = new StringBuilder();
            foreach (var key in _httpContextAccessor.HttpContext.Request.Headers.Keys)
            {
                result.AppendLine(string.Format("{0}={1}", key, _httpContextAccessor.HttpContext.Request.Headers[key]));
            }
            return result.ToString();
        }

        protected string GetHeader(string key, bool throwExceptionOnNotFound)
        {
            string result = string.Empty;
            if (_httpContextAccessor.HttpContext.Request != null && _httpContextAccessor.HttpContext.Request.Headers.ContainsKey(key))
            {
                result = _httpContextAccessor.HttpContext.Request.Headers[key];
            }
            if (result == null && throwExceptionOnNotFound)
            {
                throw new NullReferenceException(string.Format("Could not find HTTP Header with key {0}.", key));
            }
            return result;
        }

        protected virtual void ValidateRequestMethod(HttpVerb verb)
        {
            ValidateRequestMethod(verb.ToString());
        }

        protected virtual void ValidateRequestMethod(string verb)
        {
            if (GetCurrentRequestMethod() != verb)
            {
                throw new UserThrownException(
                    string.Format(
                    "Unexpected Method of {0} on incoming POST Request {1}.",
                    GetCurrentRequestMethod(),
                    GetCurrentRequestUri()),
                    LoggingLevel.Normal);
            }
        }

        protected virtual string GetCurrentUserName()
        {
            if (_httpContextAccessor.HttpContext.User != null)
            {
                Claim claim = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                {
                    return claim.Value;
                }
            }
            return null;
        }

        protected virtual Type GetEntityType(string entityName)
        {
            Type result = AssemblyReader.FindType(_databaseSettings.EntityFrameworkModelsAssembly, _databaseSettings.EntityFrameworkModelsNamespace, entityName, false);
            if (result == null) //If the entity type was not found in the specified models assembly and namespace, look for entity type in the current executing assembly in the default core rest models namespace.
            {
                string currentAssemblyName = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);
                string RestApiModelsNamespace = typeof(NKitBaseModel).Namespace;
                result = AssemblyReader.FindType(currentAssemblyName, RestApiModelsNamespace, entityName, false);
            }
            if (result == null)
            {
                throw new NullReferenceException(string.Format("Could not find entity with name {0}.", entityName));
            }
            return result;
        }

        protected void DisposeEntityContext()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }

        /// <summary>
        /// Returns either a JSON or XML serializer based on the SerializerType set in the NKitWebApiSettings of the appsettings.json file.
        /// </summary>
        /// <returns></returns>
        protected ISerializer GetSerializer()
        {
            return GOC.Instance.GetSerializer(_webApiSettings.SerializerType);
        }

        protected Type[] GetNKitSerializerModelTypes()
        {
            return new Type[] { typeof(NKitBaseModel), typeof(NKitHttpExceptionResponse), typeof(NKitLogEntry) };
        }

        #endregion //Methods

        #region Actions

        [HttpGet, Route("{entityName}/{entityId}")]
        public virtual IActionResult GetEntityById(string entityName, string entityId)
        {
            try
            {
                string requestName = $"{nameof(GetEntityById)} : Entity Name = {entityName} : Entity ID = {entityId}";
                LogRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntityById != null)
                {
                    OnBeforeGetEntityById(this, new NKitRestApiGetEntityByIdEventArgsCore(entityName, userName, _context, entityType, entityId, null));
                }
                object outputEntity = _context.GetEntityBySurrogateKey(entityType, entityId, false, userName).Contents;
                if (OnAfterGetEntityById != null)
                {
                    OnAfterGetEntityById(this, new NKitRestApiGetEntityByIdEventArgsCore(entityName, userName, _context, entityType, entityId, outputEntity));
                }
                string serializedText = GetSerializer().SerializeToText(outputEntity, GetNKitSerializerModelTypes());
                LogResponse(requestName, serializedText);
                Response.ContentType = _webApiSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        //[HttpGet, Route("{entityName}?searchBy={fieldName}&searchValueOf={fieldValue}")]
        //Called as such: "{entityName}?={fieldName}&searchValueOf={fieldValue}")]
        //Using Query Parameters: https://stackoverflow.com/questions/59621208/how-do-i-use-query-parameters-in-attributes
        [HttpGet, Route("{entityName}")]
        public virtual IActionResult GetEntities([FromRoute] string entityName, [FromQuery] string searchBy, [FromQuery] string searchValueOf)
        {
            try
            {
                string requestName = $"{nameof(GetEntities)} : Entity Name = {entityName} : Search by = {searchBy} : Search by value = {searchValueOf}";
                LogRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntitiesByField != null)
                {
                    OnBeforeGetEntitiesByField(this, new NKitRestApiGetEntitiesEventArgsCore(entityName, userName, _context, entityType, searchBy, searchValueOf, null));
                }
                List<object> outputEntities = string.IsNullOrEmpty(searchBy) ?
                    _context.GetAllEntities(entityType, false, userName).Contents :
                    _context.GetEntitiesByField(entityType, searchBy, searchValueOf, false, userName).Contents;
                if (OnAfterGetEntitiesByField != null)
                {
                    OnAfterGetEntitiesByField(this, new NKitRestApiGetEntitiesEventArgsCore(entityName, userName, _context, entityType, searchBy, searchValueOf, outputEntities));
                }
                string serializedText = GetSerializer().SerializeToText(outputEntities, GetNKitSerializerModelTypes());
                LogResponse(requestName, serializedText);
                Response.ContentType = _webApiSettings.ResponseContentType;
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpPut, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult PutEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PutEntity)} : Entity Name : {entityName}";
                ValidateRequestMethod(HttpVerb.PUT);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GetSerializer().DeserializeFromText(entityType, GetNKitSerializerModelTypes(), serializedText);
                LogRequest(requestName, serializedText);
                if (OnBeforePut != null)
                {
                    OnBeforePut(this, new NKitRestApiPutEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                _context.Save(entityType, new List<object>() { inputEntity }, userName, false);
                if (OnAfterPut != null)
                {
                    OnAfterPut(this, new NKitRestApiPutEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                string responseMessage = string.Format("{0} saved successfully.", entityName);
                LogResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpPost, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON, MimeContentType.APPLICATION_XML)]
        public virtual IActionResult PostEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PostEntity)} : Entity Name = {entityName}";
                ValidateRequestMethod(HttpVerb.POST);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GetSerializer().DeserializeFromText(entityType, GetNKitSerializerModelTypes(), serializedText);
                LogRequest(requestName, serializedText);
                if (OnBeforePost != null)
                {
                    OnBeforePost(this, new NKitRestApiPostEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                _context.Insert(entityType, new List<object>() { inputEntity }, userName, false);
                if (OnAfterPost != null)
                {
                    OnAfterPost(this, new NKitRestApiPostEntityEventArgsCore(
                        entityName, userName, _context, entityType, inputEntity));
                }
                string responseMessage = string.Format("{0} saved successfully.", entityName);
                LogResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpDelete, Route("{entityName}/{entityId}")]
        public virtual IActionResult DeleteEntity(string entityName, string entityId)
        {
            try
            {
                string requestName = $"{nameof(DeleteEntity)} : Entity Name = {entityName} : Entity ID = {entityId}";
                LogRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.DELETE);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeDelete != null)
                {
                    OnBeforeDelete(this, new NKitRestApiDeleteEntityEventArgsCore(
                        entityName, userName, _context, entityType, entityId));
                }
                _context.DeleteBySurrogateKey(entityType, new List<object>() { entityId }, userName);
                if (OnAfterDelete != null)
                {
                    OnAfterDelete(this, new NKitRestApiDeleteEntityEventArgsCore(
                        entityName, userName, _context, entityType, entityId));
                }
                string responseMessage = string.Format("{0} deleted successfully.", entityName);
                LogResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        #endregion //Actions
    }
}
