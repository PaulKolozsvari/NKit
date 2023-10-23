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
    using System.IO;
    using System.Reflection;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.Email;
    using NKit.Settings.Default;

    #endregion //Using Directives

    /// <summary>
    /// Base controller that contains all the utility methods that may be required by any controller implementing the NKit framework e.g. logging of requests, accessing the NKitDbRepository
    /// that manages an underlying DbContext etc. 
    /// However this base controller does not contain any CRUD (Create, Read, Update, Delete) actions.
    /// It can be useful when your controller would prefer to not expose the underlying database's data through CRUD actions.
    /// N.B. Must register HttpContextAccessor as a service in Startup.ConfigureServices of your app: services.AddHttpContextAccessor();
    /// </summary>
    /// <typeparam name="D">The NKitDbRepository that manages an underlying DbContext.</typeparam>
    [ApiController]
    public class NKitWebApiController<D> : ControllerBase where D : NKitDbContext
    {
        #region Constructors

        public NKitWebApiController(
            D dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitWebApiControllerSettings> webApiControllerOptions,
            IOptions<NKitDbContextSettings> databaseOptions,
            IOptions<NKitEmailClientServiceSettings> emailOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            IOptions<NKitWebApiClientSettings> webApiClientOptions,
            NKitEmailClientService emailClientService,
            ILogger logger)
        {
            DataValidator.ValidateObjectNotNull(dbContext, nameof(dbContext), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(httpContextAccessor, nameof(httpContextAccessor), nameof(NKitWebApiController<D>));

            DataValidator.ValidateObjectNotNull(generalOptions, nameof(generalOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(webApiControllerOptions, nameof(webApiControllerOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(databaseOptions, nameof(databaseOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(emailOptions, nameof(emailOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitWebApiController<D>));
            DataValidator.ValidateObjectNotNull(webApiClientOptions, nameof(webApiClientOptions), nameof(NKitWebApiController<D>));

            _serviceInstanceId = Guid.NewGuid();

            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;

            _generalSettings = generalOptions.Value;
            _webApiControllerSettings = webApiControllerOptions.Value;
            _dbRepositorySettings = databaseOptions.Value;
            _emailSettings = emailOptions.Value;
            _loggingSettings = loggingOptions.Value;
            _webApiClientSettings = webApiClientOptions.Value;

            _emailClientService = emailClientService;
            _logger = logger;
        }

        #endregion //Constructors

        #region Fields

        private readonly Nullable<Guid> _serviceInstanceId;

        private readonly NKitEmailClientService _emailClientService;
        private readonly ILogger _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly D _dbContext;

        private readonly NKitGeneralSettings _generalSettings;
        private readonly NKitWebApiControllerSettings _webApiControllerSettings;
        private readonly NKitDbContextSettings _dbRepositorySettings;
        private readonly NKitEmailClientServiceSettings _emailSettings;
        private readonly NKitLoggingSettings _loggingSettings;
        private readonly NKitWebApiClientSettings _webApiClientSettings;

        #endregion //Fields

        #region Properties

        /// <summary>
        /// A unique GUID of the current instance of this controller. A new instance of this controller is created per request.
        /// </summary>
        protected Nullable<Guid> ServiceInstanceId { get { return _serviceInstanceId; } }

        /// <summary>
        /// An email client service that can be used to send out out emails as per configuration in the NKitEmailClientServiceSettings section in the appsettings.json file.
        /// </summary>
        protected NKitEmailClientService EmailClientService { get { return _emailClientService; } }

        /// <summary>
        /// Logger being supplied to the controller through dependency injection.
        /// </summary>
        protected ILogger Logger { get { return _logger; } }

        /// <summary>
        /// HTTP Context Accessor which can be used to access the HTTP context providing information about the web request.
        /// </summary>
        protected IHttpContextAccessor HttpContextAccessor { get { return _httpContextAccessor; } }

        /// <summary>
        /// The NKitDbContext which can be used to access data in the database.
        /// </summary>
        protected D DbContext { get { return _dbContext; } }

        /// <summary>
        /// The NKitGeneralSettings settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitGeneralSettings GeneralSettings { get { return _generalSettings; } }

        /// <summary>
        /// The NKitWebApiController settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitWebApiControllerSettings WebApiControllerSettings { get { return _webApiControllerSettings; } }

        /// <summary>
        /// The NKitDbRepository settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitDbContextSettings DbRepositorySettings { get { return _dbRepositorySettings; } }

        /// <summary>
        /// The NKitEmailCllient settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitEmailClientServiceSettings EmailSettings { get { return _emailSettings; } }

        /// <summary>
        /// The NKitEmailCllient settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitLoggingSettings LoggingSettings { get { return _loggingSettings; } }

        /// <summary>
        /// The NKitWebApiClientSettings settings that were configured in the appsettings.json file.
        /// </summary>
        protected NKitWebApiClientSettings WebApiClientSettings { get { return _webApiClientSettings; } }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Serializes an object to text using the current serializer.
        /// </summary>
        /// <param name="entity">Object to serialize</param>
        /// <returns>Returns the serialized text.</returns>
        protected string SerializeToText(object entity)
        {
            return GetSerializer().SerializeToText(entity, GetNKitSerializerModelTypes());
        }

        /// <summary>
        /// Deserializes a given text to an object of the specified type.
        /// </summary>
        /// <param name="type">The type to deserialize to.</param>
        /// <param name="text">The text to deserialize.</param>
        /// <returns>Returns the object that has been deserialized.</returns>
        protected object DeserializeFromText(Type type, string text)
        {
            return GetSerializer().DeserializeFromText(type, GetNKitSerializerModelTypes(), text);
        }

        /// <summary>
        /// Logs a web request to the Logger as well as to the NKitLogEntry database table, including all parameters of the current web request
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        /// <param name="actionName">The name of the action handing the current web request/response.</param>
        /// <param name="requestEntity">The object received in the web request (if any).</param>
        protected void LogWebRequest(string actionName, object requestEntity, out string requestMessage)
        {
            requestMessage = requestEntity != null ? GetSerializer().SerializeToText(requestEntity, GetNKitSerializerModelTypes()) : null;
            LogWebRequest(actionName, requestMessage);
        }

        /// <summary>
        /// Logs a web request to the Logger as well as to the NKitLogEntry database table, including all parameters of the current web request
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        /// <param name="actionName">The name of the action handing the current web request/response.</param>
        /// <param name="requestMessage">The message payload/contents of the web request (if any).</param>
        protected void LogWebRequest(string actionName, string requestMessage)
        {
            if (!_webApiControllerSettings.LogRequests && !_webApiControllerSettings.LogRequestsInDatabaseNKitLogEntry)
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
            logMessageBuilder.AppendLine(GetFullWebRequestInfoMessage());
            string logMessage = logMessageBuilder.ToString();
            if (_logger != null && _webApiControllerSettings.LogRequests)
            {
                _logger.LogInformation(logMessage);
            }
            if (_dbContext != null && _webApiControllerSettings.LogRequestsInDatabaseNKitLogEntry)
            {
                _dbContext.LogWebActionActivityToNKitLogEntry(nameof(NKitWebApiController<D>), actionName, logMessage, new EventId(27, "Request"));
            }
        }

        /// <summary>
        /// Logs a web response to the Logger as well as to the NKitLogEntry database table, including all parameters of the current web request
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        /// <param name="actionName">The name of the action handing the current web request/response.</param>
        /// <param name="responseEntity">The object being returned in the web response.</param>
        protected void LogWebResponse(string actionName, object responseEntity, out string responseMessage)
        {
            LogWebResponse(actionName, responseEntity, setResponseContentType: true, out responseMessage);
        }

        /// <summary>
        /// Logs a web response to the Logger as well as to the NKitLogEntry database table, including all parameters of the current web request
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        /// <param name="actionName">The name of the action handing the current web request/response.</param>
        /// <param name="responseEntity">The object being returned in the web response.</param>
        /// <param name="setResponseContentType">Whether or not to set the response content type based on the ResponseContentType in the WebApiControllerSettings.</param>
        protected void LogWebResponse(string actionName, object responseEntity, bool setResponseContentType, out string responseMessage)
        {
            responseMessage = responseEntity != null ? GetSerializer().SerializeToText(responseEntity, GetNKitSerializerModelTypes()) : null;
            LogWebResponse(actionName, responseMessage);
            if (setResponseContentType)
            {
                Response.ContentType = WebApiControllerSettings.ResponseContentType;
            }
        }

        /// <summary>
        /// Logs a web response to the Logger as well as to the NKitLogEntry database table, including all parameters of the current web request
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        /// <param name="actionName">The name of the action handing the current web request/response.</param>
        /// <param name="responseMessage">The message payload/contents of the web response (if any).</param>
        protected void LogWebResponse(string actionName, string responseMessage)
        {
            if (!_webApiControllerSettings.LogResponses && !_webApiControllerSettings.LogResponsesInDatabaseNKitLogEntry)
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
            logMessageBuilder.AppendLine(GetFullWebRequestInfoMessage());
            string logMessage = logMessageBuilder.ToString();
            if (_logger != null && _webApiControllerSettings.LogResponses)
            {
                _logger.LogInformation(logMessage);
            }
            if (_dbContext != null && _webApiControllerSettings.LogResponsesInDatabaseNKitLogEntry)
            {
                _dbContext.LogWebActionActivityToNKitLogEntry(nameof(NKitWebApiController<D>), actionName, logMessage, new EventId(28, "Response"));
            }
        }

        /// <summary>
        /// Gets a full message of all the details about the running web request.
        /// e.g. server hostname, request URI, number of threads running etc.
        /// </summary>
        private string GetFullWebRequestInfoMessage()
        {
            StringBuilder result = new StringBuilder();
            ThreadHelper.GetCurrentThreadCount(out int workerThreadsRunning, out int completionPortThreadsRunning);
            result.AppendLine($"Server: {GetCurrentServerHostName()}");
            result.AppendLine($"Request URI: {GetCurrentWebRequestUri()}");
            result.AppendLine($"Request Method: {GetCurrentWebRequestMethod()}");
            result.AppendLine($"Service Instance ID: {_serviceInstanceId}");
            result.AppendLine($"Worker Threads Running: {workerThreadsRunning}");
            result.AppendLine($"Completion Port Threads Running: {completionPortThreadsRunning}");
            result.AppendLine($"Total Threads Running: {GetTotalThreadsRunningCountInCurrentProcess()}");
            result.AppendLine($"Current Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            return result.ToString();
        }

        /// <summary>
        /// Gets the current server hostname which the application is running on.
        /// </summary>
        protected string GetCurrentServerHostName()
        {
            return _httpContextAccessor.HttpContext.Request.Host.Value;
        }

        /// <summary>
        /// Gets the total number of threads currently running in the current process.
        /// </summary>
        /// <returns></returns>
        protected int GetTotalThreadsRunningCountInCurrentProcess()
        {
            return ThreadHelper.GetTotalThreadsRunningCountInCurrentProcess();
        }

        /// <summary>
        /// Gets the number of worker threads and completion port threads currently running.
        /// </summary>
        /// <param name="workerThreadsRunning">Number of work threads currently running.</param>
        /// <param name="completionPortThreadsRunning">Number of completion port threads currently running.</param>
        protected void GetCurrentThreadCount(out int workerThreadsRunning, out int completionPortThreadsRunning)
        {
            ThreadHelper.GetCurrentThreadCount(out workerThreadsRunning, out completionPortThreadsRunning);
        }

        /// <summary>
        /// Gets the current managed thread's ID.
        /// </summary>
        protected int GetCurrentManagedThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }


        /// <summary>
        /// Gets the current web request's URI.
        /// </summary>
        protected string GetCurrentWebRequestUri()
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

        protected virtual string GetUserAgent()
        {
            return GetHeader("User-Agent", throwExceptionOnNotFound: false);
        }

        protected virtual string GetUserHostAddress()
        {
            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        /// <summary>
        /// Gets the current web request's method (verb) e.g. GET, PUT, POST, DELETE.
        /// </summary>
        protected string GetCurrentWebRequestMethod()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        /// <summary>
        /// Gets all the current web request's HTTP headers in raw format.
        /// </summary>
        /// <returns></returns>
        protected string GetAllHeadersFullString()
        {
            return _httpContextAccessor.HttpContext.Request.Headers.ToString();
        }

        /// <summary>
        /// Gets all the current web request's HTTP headers formatted which each header her line of text with {HEADER}={VALUE}.
        /// </summary>
        protected string GetAllHeadersFormatted()
        {
            StringBuilder result = new StringBuilder();
            foreach (var key in _httpContextAccessor.HttpContext.Request.Headers.Keys)
            {
                result.AppendLine(string.Format("{0}={1}", key, _httpContextAccessor.HttpContext.Request.Headers[key]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the value of a specific HTTP header in the current web request.
        /// </summary>
        /// <param name="key">The specific HTTP header to be retrieved.</param>
        /// <param name="throwExceptionOnNotFound">Whether or not an exception need to be thrown if the header does nto exist. Otherwise it returns an empty string.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Validates that the current web request's method of a specific method (verb).
        /// Throws an exception if the current web request's method (verb) does not match the specified one.
        /// </summary>
        /// <param name="verb">The HTTP method (verb) to check for.</param>
        protected virtual void ValidateRequestMethod(HttpVerb verb)
        {
            ValidateRequestMethod(verb.ToString());
        }

        /// <summary>
        /// Validates that the current web request's method of a specific method (verb).
        /// Throws an exception if the current web request's method (verb) does not match the specified one.
        /// </summary>
        /// <param name="verb">The HTTP method (verb) to check for.</param>
        protected virtual void ValidateRequestMethod(string verb)
        {
            if (GetCurrentWebRequestMethod() != verb)
            {
                throw new Exception($"Unexpected Method of {GetCurrentWebRequestMethod()} on incoming POST Request {GetCurrentWebRequestUri()}.");
            }
        }

        /// <summary>
        /// Gets the identify (user name) of the current user making the web request i.e. based on the credentials passed in the web request.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Uses reflection to look for and return a .NET type in the DbContext that is managed by the NKitDbRepository.
        /// The search is done by looking in the assembly and namespace configured in EntityFrameworkModelsAssembly and EntityFrameworkModelsNamespace settings of the NKitDbRepositorySettings section of the appsettings.json file.
        /// Returns the .NET entity type from the DbContext. Throws an exception if it is not found.
        /// </summary>
        /// <param name="entityName">The name of the entity type to look for.</param>
        protected virtual Type GetEntityType(string entityName)
        {
            Type result = AssemblyReader.FindType(_dbRepositorySettings.EntityFrameworkModelsAssembly, _dbRepositorySettings.EntityFrameworkModelsNamespace, entityName, false);
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

        /// <summary>
        /// Disposes the NKitDbRepository which in turn disposes the DbContext which it manages.
        /// </summary>
        protected void DisposeEntityContext()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }

        /// <summary>
        /// Returns either a JSON or XML serializer based on the SerializerType set in the NKitWebApiSettings section of the appsettings.json file.
        /// </summary>
        /// <returns></returns>
        protected ISerializer GetSerializer()
        {
            return GOC.Instance.GetSerializer(_webApiControllerSettings.SerializerType);
        }

        /// <summary>
        /// Gets the NKit .NET model types used by this NKitWebApiController. 
        /// This is usefull to pass into the XML serializer as extra types which the deserializer needs to be made aware of in order to serialize or deserialize
        /// other objects that derive from these types.
        /// </summary>
        /// <returns></returns>
        protected Type[] GetNKitSerializerModelTypes()
        {
            return new Type[] { typeof(NKitBaseModel), typeof(NKitHttpExceptionResponse), typeof(NKitLogEntry) };
        }

        /// <summary>
        /// Sets the current response ContentType to the ContentType configured in the ResponseContentType setting in the WebApiControllerSettings section of the appsettings.json file.
        /// </summary>
        /// <returns>Returns the ResponseContentType setting in the WebApiControllerSettings section of the appsettings.json file</returns>
        protected string SetNKitResponseContentType()
        {
            Response.ContentType = WebApiControllerSettings.ResponseContentType;
            return WebApiControllerSettings.ResponseContentType;
        }

        #endregion //Methods
    }
}