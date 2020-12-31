namespace NKit.Web.Service.CoreRest
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
    using NKit.Core.Utilities.Logging;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.Logging;
    using NKit.Web.Client;
    using NKit.Web.Service.CoreRest.Events;
    using Microsoft.AspNetCore.Http.Extensions;
    using System.Security.Claims;
    using NKit.Data;
    using Microsoft.Extensions.Options;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    /// <summary>
    /// Must register HttpContextAccessor as a service in Startup.ConfigureServices of your app: services.AddHttpContextAccessor();
    /// </summary>
    /// <typeparam name="D"></typeparam>
    [ApiController]
    public class WebApiRestServiceControllerCore<D> : ControllerBase where D : DbContextCrudTransactionsRepositoryCore
    {
        #region Constructors

        public WebApiRestServiceControllerCore(
            D context,
            IHttpContextAccessor httpContextAccessor, 
            IOptions<DatabaseSettings> databaseOptions,
            IOptions<EmailSettings> emailOptions,
            IOptions<LoggingSettings> loggingOptions)
        {
            _logger = AppLogger.CreateLogger(nameof(WebApiRestServiceControllerCore<D>));
            DataValidator.ValidateObjectNotNull(context, nameof(context), nameof(WebApiRestServiceControllerCore<D>));
            DataValidator.ValidateObjectNotNull(httpContextAccessor, nameof(httpContextAccessor), nameof(WebApiRestServiceControllerCore<D>));
            DataValidator.ValidateObjectNotNull(databaseOptions, nameof(databaseOptions), nameof(WebApiRestServiceControllerCore<D>));
            DataValidator.ValidateObjectNotNull(emailOptions, nameof(emailOptions), nameof(WebApiRestServiceControllerCore<D>));
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(WebApiRestServiceControllerCore<D>));
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _databaseSettings = databaseOptions.Value;
            _emailSettings = emailOptions.Value;
            _loggingSettings = loggingOptions.Value;
        }

        #endregion //Constructors

        #region Fields

        public event OnBeforeGetEntitiesHandlerCore OnBeforeGetEntities;
        public event OnAfterGetEntitiesHandlerCore OnAfterGetEntities;

        public event OnBeforeGetEntityByIdHandlerCore OnBeforeGetEntityById;
        public event OnAfterGetEntityByIdHandlerCore OnAfterGetEntityById;

        public event OnBeforeGetEntitiesByFieldHandlerCore OnBeforeGetEntitiesByField;
        public event OnAfterGetEntitiesByFieldHandlerCore OnAfterGetEntitiesByField;

        public event OnBeforePutEntityHandlerCore OnBeforePut;
        public event OnAfterPutEntityHandlerCore OnAfterPut;

        public event OnBeforePostEntityHandlerCore OnBeforePost;
        public event OnAfterPostEntityHandlerCore OnAfterPost;

        public event OnBeforeDeleteEntityHandlerCore OnBeforeDelete;
        public event OnAfterDeleteEntityHandlerCore OnAfterDelete;

        protected bool _auditServiceCalls;
        protected Nullable<Guid> _serviceInstanceId;

        protected readonly ILogger _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected IServiceProvider _serviceProvider;
        protected IHttpContextAccessor _httpContextAccessor;
        protected D _context;

        protected DatabaseSettings _databaseSettings;
        protected EmailSettings _emailSettings;
        protected LoggingSettings _loggingSettings;

        #endregion //Fields

        #region Methods

        protected void AuditRequest(string requestName, string requestMessage)
        {
            if (!_auditServiceCalls)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine($"Request Name: {requestName}");
            if (!string.IsNullOrEmpty(requestMessage))
            {
                logMessage.AppendLine($"Request Message:");
                logMessage.AppendLine(requestMessage);
            }
            AppendStatsToLogMessage(logMessage);
            _logger.LogInformation(logMessage.ToString());
        }

        protected void AuditResponse(string requestName, string responseMessage)
        {
            if (!_auditServiceCalls)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine($"Response Message: {requestName}");
            if (!string.IsNullOrEmpty(responseMessage))
            {
                logMessage.AppendLine($"Request Message:");
                logMessage.AppendLine(responseMessage);
            }
            AppendStatsToLogMessage(logMessage);
            _logger.LogInformation(logMessage.ToString());
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

        public string GetServerHostName()
        {
            return _httpContextAccessor.HttpContext.Request.Host.Value;
        }

        public string GetCurrentRequestUri()
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

        public string GetCurrentRequestMethod()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        public string GetAllHeadersFullString()
        {
            return _httpContextAccessor.HttpContext.Request.Headers.ToString();
        }

        public string GetAllHeadersFormatted()
        {
            StringBuilder result = new StringBuilder();
            foreach (var key in _httpContextAccessor.HttpContext.Request.Headers.Keys)
            {
                result.AppendLine(string.Format("{0}={1}", key, _httpContextAccessor.HttpContext.Request.Headers[key]));
            }
            return result.ToString();
        }

        public string GetHeader(string key, bool throwExceptionOnNotFound)
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
            Type result = AssemblyReader.FindType(
                _databaseSettings.EntityFrameworkModelsAssembly,
                _databaseSettings.EntityFrameworkModelsNamespace,
                entityName,
                false);
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

        #endregion //Methods

        #region Actions

        [HttpGet, Route("{entityName}")]
        public virtual IActionResult GetEntities(string entityName)
        {
            try
            {
                string requestName = $"{nameof(GetEntities)} : Entity Name = {entityName}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntities != null)
                {
                    OnBeforeGetEntities(this, new RestServiceGetEntitiesEventArgsCore(entityName, userName, _context, entityType, null));
                }
                List<object> outputEntities = _context.GetAllEntities(entityType, false, userName).Contents;
                if (OnAfterGetEntities != null)
                {
                    OnAfterGetEntities(this, new RestServiceGetEntitiesEventArgsCore(entityName, userName, _context, entityType, outputEntities));
                }
                string serializedText = GOC.Instance.JsonSerializer.SerializeToText(outputEntities);
                AuditResponse(requestName, serializedText);
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpGet, Route("{entityName}/{entityId}")]
        public virtual IActionResult GetEntityById(string entityName, string entityId)
        {
            try
            {
                string requestName = $"{nameof(GetEntityById)} : Entity Name = {entityName} : Entity ID = {entityId}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntityById != null)
                {
                    OnBeforeGetEntityById(this, new RestServiceGetEntityByIdEventArgsCore(entityName, userName, _context, entityType, entityId, null));
                }
                object outputEntity = _context.GetEntityBySurrogateKey(entityType, entityId, false, userName).Contents;
                if (OnAfterGetEntityById != null)
                {
                    OnAfterGetEntityById(this, new RestServiceGetEntityByIdEventArgsCore(entityName, userName, _context, entityType, entityId, outputEntity));
                }
                string serializedText = GOC.Instance.JsonSerializer.SerializeToText(outputEntity);
                AuditResponse(requestName, serializedText);
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
        [HttpGet, Route("{entityName}/searchBy")]
        public virtual IActionResult GetEntitiesByField([FromRoute] string entityName, [FromQuery] string fieldName, [FromQuery] string fieldValue)
        {
            try
            {
                string requestName = $"{nameof(GetEntitiesByField)} : Entity Name = {entityName} : Field Name = {fieldName} : Field Value = {fieldValue}";
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.GET);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeGetEntitiesByField != null)
                {
                    OnBeforeGetEntitiesByField(this, new RestServiceGetEntitiesByFieldEventArgsCore(entityName, userName, _context, entityType, fieldName, fieldValue, null));
                }
                List<object> outputEntities = _context.GetEntitiesByField(entityType, fieldName, fieldValue, false, userName).Contents;
                if (OnAfterGetEntitiesByField != null)
                {
                    OnAfterGetEntitiesByField(this, new RestServiceGetEntitiesByFieldEventArgsCore(entityName, userName, _context, entityType, fieldName, fieldValue, outputEntities));
                }
                string serializedText = GOC.Instance.JsonSerializer.SerializeToText(outputEntities);
                AuditResponse(requestName, serializedText);
                return Ok(serializedText);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpPut, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON)]
        public virtual IActionResult PutEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PutEntity)} : Entity Name : {entityName}";
                ValidateRequestMethod(HttpVerb.PUT);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GOC.Instance.JsonSerializer.DeserializeFromText(entityType, serializedText);
                AuditRequest(requestName, serializedText);
                if (OnBeforePut != null)
                {
                    OnBeforePut(this, new RestServicePutEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                _context.Save(entityType, new List<object>() { inputEntity }, userName, false);
                if (OnAfterPut != null)
                {
                    OnAfterPut(this, new RestServicePutEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                string responseMessage = string.Format("{0} saved successfully.", entityName);
                AuditResponse(requestName, responseMessage);
                return Ok(responseMessage);
            }
            finally
            {
                DisposeEntityContext();
            }
        }

        [HttpPost, Route("{entityName}")]
        [Consumes(MimeContentType.TEXT_PLAIN, MimeContentType.APPLICATION_JSON)]
        public virtual IActionResult PostEntity(string entityName, [FromBody] string serializedText)
        {
            try
            {
                string requestName = $"{nameof(PostEntity)} : Entity Name = {entityName}";
                ValidateRequestMethod(HttpVerb.POST);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                object inputEntity = GOC.Instance.JsonSerializer.DeserializeFromText(entityType, serializedText);
                AuditRequest(requestName, serializedText);
                if (OnBeforePost != null)
                {
                    OnBeforePost(this, new RestServicePostEntityEventArgsCore(entityName, userName, _context, entityType, inputEntity));
                }
                _context.Insert(entityType, new List<object>() { inputEntity }, userName, false);
                if (OnAfterPost != null)
                {
                    OnAfterPost(this, new RestServicePostEntityEventArgsCore(
                        entityName, userName, _context, entityType, inputEntity));
                }
                string responseMessage = string.Format("{0} saved successfully.", entityName);
                AuditResponse(requestName, responseMessage);
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
                AuditRequest(requestName, null);
                ValidateRequestMethod(HttpVerb.DELETE);
                string userName = GetCurrentUserName();
                Type entityType = GetEntityType(entityName);
                if (OnBeforeDelete != null)
                {
                    OnBeforeDelete(this, new RestServiceDeleteEntityEventArgsCore(
                        entityName, userName, _context, entityType, entityId));
                }
                _context.DeleteBySurrogateKey(entityType, new List<object>() { entityId }, userName);
                if (OnAfterDelete != null)
                {
                    OnAfterDelete(this, new RestServiceDeleteEntityEventArgsCore(
                        entityName, userName, _context, entityType, entityId));
                }
                string responseMessage = string.Format("{0} deleted successfully.", entityName);
                AuditResponse(requestName, responseMessage);
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
