namespace NKit.Web.MVC.Controllers
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Settings.Default;
    using NKit.Utilities;
    using NKit.Utilities.Email;
    using NKit.Utilities.Serialization;
    using NKit.Web.Client;
    using Microsoft.AspNetCore.Http.Extensions;
    using System.Security.Claims;
    using System.IO;
    using System.Reflection;
    using NKit.Web.MVC.Models;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    public class NKitMvcController<D, E> : Controller where D : NKitDbContext where E : NKitEmailClientService
    {
        #region Constructors

        public NKitMvcController(
            D dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitWebApiControllerSettings> webApiControllerOptions,
            IOptions<NKitDbContextSettings> databaseOptions,
            IOptions<NKitEmailClientServiceSettings> emailOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            IOptions<NKitWebApiClientSettings> webApiClientOptions,
            E emailClientService,
            ILogger logger,
            IWebHostEnvironment environment)
        {
            DataValidator.ValidateObjectNotNull(dbContext, nameof(dbContext), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(httpContextAccessor, nameof(httpContextAccessor), nameof(NKitMvcController<D, E>));

            DataValidator.ValidateObjectNotNull(generalOptions, nameof(generalOptions), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(webApiControllerOptions, nameof(webApiControllerOptions), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(databaseOptions, nameof(databaseOptions), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(emailOptions, nameof(emailOptions), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitMvcController<D, E>));
            DataValidator.ValidateObjectNotNull(webApiClientOptions, nameof(webApiClientOptions), nameof(NKitMvcController<D, E>));

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

            _environment = environment;
        }

        #endregion //Constructors

        #region Constants

        protected const string CONFIRMATION_DIALOG_PARTIAL_VIEW_NAME = "_ConfirmationDialog";
        protected const string CONFIRMATION_DIALOG_DIV_ID = "dlgConfirmation"; //Used to manage div making up the dialog.

        protected const string WAIT_DIALOG_PARTIAL_VIEW_NAME = "_WaitDialog";
        protected const string WAIT_DIALOG_DIV_ID = "dlgWait";

        protected const string INFORMATION_DIALOG_PARTIAL_VIEW_NAME = "_InformationDialog";
        protected const string INFORMATION_DIALOG_DIV_ID = "dlgInformation";

        protected const string WEB_REQUEST_ACTIVITY_SOURCE_APPLICATION = "WebSite";

        #endregion //Constants

        #region Fields

        private readonly Nullable<Guid> _serviceInstanceId;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly D _dbContext;

        private readonly NKitGeneralSettings _generalSettings;
        private readonly NKitWebApiControllerSettings _webApiControllerSettings;
        private readonly NKitDbContextSettings _dbRepositorySettings;
        private readonly NKitEmailClientServiceSettings _emailSettings;
        private readonly NKitLoggingSettings _loggingSettings;
        private readonly NKitWebApiClientSettings _webApiClientSettings;

        private readonly E _emailClientService;
        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _environment; 

        #endregion //Fields

        #region Properties

        /// <summary>
        /// A unique GUID of the current instance of this controller. A new instance of this controller is created per request.
        /// </summary>
        protected Nullable<Guid> ServiceInstanceId { get { return _serviceInstanceId; } }

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

        /// <summary>
        /// An email client service that can be used to send out out emails as per configuration in the NKitEmailClientServiceSettings section in the appsettings.json file.
        /// </summary>
        protected E EmailClientService { get { return _emailClientService; } }

        /// <summary>
        /// Logger being supplied to the controller through dependency injection.
        /// </summary>
        protected ILogger Logger { get { return _logger; } }

        /// <summary>
        /// Provides information about a web hosting environment the application is running in.
        /// </summary>
        protected IWebHostEnvironment Environment { get { return _environment; } }

        #endregion //Properties

        #region Methods

        protected bool IsRequestAuthenticated()
        {
            return (this.User != null) && (this.User.Identity != null) && this.User.Identity.IsAuthenticated;
        }

        protected virtual void SetViewBagSearchFieldIdentifier<T>(FilterModelCore<T> model) where T : class
        {
            ViewBag.SearchFieldIdentifier = model.SearchFieldIdentifier;
        }

        protected virtual RedirectToActionResult HandleException(Exception ex)
        {
            ExceptionHandlerCore.HandleException(Logger, ex, DbContext.GetErrorEmailNotificationRecipientsFailSafe(), EmailClientService, DbContext);
            return RedirectToError(ex.Message);
        }

        protected string GetAbsoluteFilePathFromRequest(string relativePath)
        {
            return Request.PathBase + relativePath[1..]; //https://stackoverflow.com/questions/50603901/asp-net-core-replacement-for-virtualpathutility
        }

        /// <summary>
        /// Gets all the absolute file paths of all the files in the sub directory specified within the web root directory i.e. wwwroot.
        /// </summary>
        /// <param name="relativeDirectoryPath"></param>
        /// <returns></returns>
        protected string[] GetAbsoluteFilesPathsInWebRootDirectory(string relativeDirectoryPath)
        {
            return Directory.GetFiles(Path.Combine(Environment.WebRootPath, relativeDirectoryPath));
        }

        /// <summary>
        /// Gets the absolute file path of a file in the web root directory (i.e. wwwroot) given the relative file path.
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <returns></returns>
        protected string GetAbsoluteFilePathInWebRootDirectory(string relativeFilePath)
        {
            return Path.Combine(Environment.WebRootPath, relativeFilePath);
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
                _dbContext.LogWebActionActivityToNKitLogEntry(nameof(NKitMvcController<D, E>), actionName, logMessage, new EventId(27, "Request"));
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
                _dbContext.LogWebActionActivityToNKitLogEntry(nameof(NKitMvcController<D, E>), actionName, logMessage, new EventId(28, "Response"));
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

        protected async Task<string> GetRequestBodyAsync()
        {
            string result = string.Empty;
            Request.EnableBuffering();
            Request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader stream = new StreamReader(Request.Body, leaveOpen: true))
            {
                result = await stream.ReadToEndAsync();
            }
            return result;
        }

        protected string GetRequestBody()
        {
            string result = string.Empty;
            Request.EnableBuffering();
            Request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader stream = new StreamReader(Request.Body, leaveOpen: true))
            {
                result = stream.ReadToEnd();
            }
            return result;
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

        #region MVC Methods

        protected virtual string GetCurrentActionName()
        {
            string result = this.ControllerContext.RouteData.Values["action"].ToString();
            return result;
        }

        protected virtual string GetCurrentControllerName()
        {
            string result = this.ControllerContext.RouteData.Values["controller"].ToString();
            return result;
        }

        protected virtual string GetWebRequestVerb()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        protected virtual string GetFullRequestUrl()
        {
            return _httpContextAccessor.HttpContext.Request.GetDisplayUrl(); //You need to have a using statement to include the GetDisplayUrl extendion method in your file: using Microsoft.AspNetCore.Http.Extensions
        }

        protected virtual string GetFullRequestReferrerUrl()
        {
            return _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
        }

        protected virtual string GetUserAgent()
        {
            return GetHeader("User-Agent", throwExceptionOnNotFound: false);
        }

        protected virtual string GetUserHostAddress()
        {
            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

        protected virtual string GetUserHostName()
        {
            return _httpContextAccessor.HttpContext.Request.Host.Host;
        }

        protected virtual bool IsCrawler()
        {
            //return Request.Browser == null ? false : Request.Browser.Crawler;
            throw new NotImplementedException(); //TODO Above line is for .NET Framework only, must implement it in .NET Core.
        }

        protected virtual bool IsMobileDevice()
        {
            //return Request.Browser == null ? false : Request.Browser.IsMobileDevice;
            throw new NotImplementedException(); //TODO Above line is for .NET Framework only, must implement it in .NET Core.
        }

        protected virtual string GetMobileDeviceManufacturer()
        {
            //return IsMobileDevice() ? Request.Browser.MobileDeviceManufacturer : null;
            throw new NotImplementedException(); //TODO Above line is for .NET Framework only, must implement it in .NET Core.
        }

        protected virtual string GetMobileDeviceModel()
        {
            //return IsMobileDevice() ? Request.Browser.MobileDeviceModel : null;
            throw new NotImplementedException(); //TODO Above line is for .NET Framework only, must implement it in .NET Core.
        }

        protected virtual string GetPlatform()
        {
            //return Request.Browser == null ? null : Request.Browser.Platform;
            throw new NotImplementedException(); //TODO Above line is for .NET Framework only, must implement it in .NET Core.
        }

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        //    string actionName = filterContext.ActionDescriptor.ActionName;
        //    if (actionName.Contains("Dialog") || actionName.ToLower().Contains("dialog") || controllerName == "WebRequestActivity")
        //    {
        //        return;
        //    }
        //    LogHeaders();
        //    base.OnActionExecuting(filterContext);
        //}

        protected virtual bool ExcludeWebRequestActivityByUserAgent(string currentUserAgent, List<string> userAgentsToExclude)
        {
            string currentUserAgentLower = currentUserAgent.Trim().ToLower();
            foreach (string userAgentText in userAgentsToExclude)
            {
                string s = userAgentText.Trim().ToLower();
                if (currentUserAgentLower.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual string GetClientType(bool isCrawler, bool isMobileDevice)
        {
            string result = null;
            if (isMobileDevice)
            {
                result = "Mobile";
            }
            else if (isCrawler)
            {
                result = "Crawler";
            }
            else
            {
                result = "PC";
            }
            return result;
        }

        protected virtual void LogHeaders()
        {
            string allHeadersFullString = GetAllHeadersFullString();
            string allHeadersFormatted = GetAllHeadersFormatted();
            _logger.LogInformation(allHeadersFormatted);
        }

        protected virtual void SetViewBagErrorMessage(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
        }

        public virtual void SetViewBagInformationMessage(string informationMessage)
        {
            ViewBag.InformationMessage = informationMessage;
        }

        protected virtual JsonResult GetJsonResult(bool success)
        {
            return Json(new { Success = success, ErrorMsg = string.Empty });
        }

        protected virtual JsonResult GetJsonResult(bool success, string errorMessage)
        {
            return Json(new { Success = success, ErrorMsg = errorMessage });
        }

        protected virtual JsonResult GetJsonFileResult(bool success, string fileName)
        {
            return Json(new { Success = success, FileName = fileName });
        }

        protected virtual RedirectToActionResult RedirectToHome()
        {
            return RedirectToAction("Index", "Home");
        }

        protected virtual RedirectToActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Users");
        }

        protected virtual RedirectToActionResult RedirectToError(string message)
        {
            return RedirectToAction("Error", "Information", new { errorMessage = message });
        }

        protected virtual RedirectToActionResult RedirectToInformation(string message)
        {
            return RedirectToAction("Information", "Information", new { informationMessage = message });
        }

        protected virtual RedirectToActionResult RedirectToIndex()
        {
            return RedirectToAction("Index");
        }

        #endregion //MVC Methods

        #region Header Methods

        protected virtual FileContentResult GetCsvFileResult<E>(EntityCacheGeneric<Guid, E> cache) where E : class
        {
            string filePath = Path.GetTempFileName();
            cache.ExportToCsv(filePath, null, false, false);
            string downloadFileName = string.Format("{0}-{1}.csv", typeof(E).Name, DateTime.Now);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            System.IO.File.Delete(filePath);
            return File(fileBytes, "text/plain", downloadFileName);
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText)
        {
            searchText = string.Empty;
            searchParameters = searchParametersString.Split('|');
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length > 0)
            {
                searchText = searchParameters[0];
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            searchParameters = searchParametersString.Split('|');
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 3)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<Guid> parentId)
        {
            searchText = string.Empty;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 2)
            {
                searchText = searchParameters[0];
                Guid entityIdGuid;
                if (Guid.TryParse(searchParameters[1], out entityIdGuid))
                {
                    parentId = entityIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate, 
            out bool filterByDateRange,
            out Nullable<Guid> parentId)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            filterByDateRange = false;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 4)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
                if (bool.TryParse(searchParameters[3], out bool filterByDateRangeParsed))
                {
                    filterByDateRange = filterByDateRangeParsed;
                }
                Guid entityIdGuid;
                if (Guid.TryParse(searchParameters[4], out entityIdGuid))
                {
                    parentId = entityIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate,
            out bool filterByDateRange)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            filterByDateRange = false;
            searchParameters = searchParametersString.Split('|');
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 4)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
                if (bool.TryParse(searchParameters[3], out bool filterByDateRangeParsed))
                {
                    filterByDateRange = filterByDateRangeParsed;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate,
            out string parentName,
            out Nullable<Guid> parentId)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            searchParameters = searchParametersString.Split('|');
            parentName = null;
            parentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 5)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
                parentName = searchParameters[3];
                Guid entityIdGuid;
                if (Guid.TryParse(searchParameters[4], out entityIdGuid))
                {
                    parentId = entityIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate,
            out Nullable<Guid> parentId)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 4)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
                Guid entityIdGuid;
                if (Guid.TryParse(searchParameters[3], out entityIdGuid))
                {
                    parentId = entityIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<Guid> parentId,
            out Nullable<Guid> secondParentId)
        {
            searchText = string.Empty;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            secondParentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 3)
            {
                searchText = searchParameters[0];
                Guid parentIdGuid;
                if (Guid.TryParse(searchParameters[1], out parentIdGuid))
                {
                    parentId = parentIdGuid;
                }
                Guid secondParentIdGuid;
                if (Guid.TryParse(searchParameters[2], out secondParentIdGuid))
                {
                    secondParentId = secondParentIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<Guid> parentId,
            out Nullable<Guid> secondParentId,
            out Nullable<Guid> thirdParentId)
        {
            searchText = string.Empty;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            secondParentId = null;
            thirdParentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 3)
            {
                searchText = searchParameters[0];
                Guid parentIdGuid;
                if (Guid.TryParse(searchParameters[1], out parentIdGuid))
                {
                    parentId = parentIdGuid;
                }
                Guid secondParentIdGuid;
                if (Guid.TryParse(searchParameters[2], out secondParentIdGuid))
                {
                    secondParentId = secondParentIdGuid;
                }
                Guid thirdParentIdGuid;
                if (Guid.TryParse(searchParameters[3], out thirdParentIdGuid))
                {
                    thirdParentId = thirdParentIdGuid;
                }
            }
        }

        protected virtual void GetConfirmationModelFromSearchParametersString(
            string searchParametersString,
            out string[] searchParameters,
            out string searchText,
            out Nullable<DateTime> startDate,
            out Nullable<DateTime> endDate,
            out Nullable<Guid> parentId,
            out Nullable<Guid> secondParentId)
        {
            searchText = string.Empty;
            startDate = null;
            endDate = null;
            searchParameters = searchParametersString.Split('|');
            parentId = null;
            secondParentId = null;
            if (!string.IsNullOrEmpty(searchParametersString) && searchParameters.Length >= 5)
            {
                searchText = searchParameters[0];
                DateTime startDateParsed;
                DateTime endDateParsed;
                if (DateTime.TryParse(searchParameters[1], out startDateParsed))
                {
                    startDate = startDateParsed;
                }
                if (DateTime.TryParse(searchParameters[2], out endDateParsed))
                {
                    endDate = endDateParsed;
                }
                Guid parentIdGuid;
                if (Guid.TryParse(searchParameters[3], out parentIdGuid))
                {
                    parentId = parentIdGuid;
                }
                Guid secondParentIdGuid;
                if (Guid.TryParse(searchParameters[4], out secondParentIdGuid))
                {
                    secondParentId = secondParentIdGuid;
                }
            }
        }

        #endregion //Header Methods

        #endregion //Methods

        #region Actions

        public virtual ActionResult WaitDialog(string message)
        {
            try
            {
                WaitModel model = new WaitModel();
                model.PostBackControllerAction = GetCurrentActionName();
                model.PostBackControllerName = GetCurrentControllerName();
                model.DialogDivId = WAIT_DIALOG_DIV_ID;
                model.WaitMessage = message == null ? string.Empty : message;
                PartialViewResult result = PartialView(WAIT_DIALOG_PARTIAL_VIEW_NAME, model);
                return result;
            }
            catch (Exception ex)
            {
                ExceptionHandlerCore.HandleException(Logger, ex, DbContext.GetErrorEmailNotificationRecipientsFailSafe(), EmailClientService, DbContext);
                return GetJsonResult(false, ex.Message);
            }
        }

        public virtual ActionResult InformationDialog(string message)
        {
            try
            {
                InformationModel model = new InformationModel();
                model.PostBackControllerAction = GetCurrentActionName();
                model.PostBackControllerName = GetCurrentControllerName();
                model.DialogDivId = INFORMATION_DIALOG_DIV_ID;
                model.InformationMessage = message == null ? string.Empty : message;
                PartialViewResult result = PartialView(INFORMATION_DIALOG_PARTIAL_VIEW_NAME, model);
                return result;
            }
            catch (Exception ex)
            {
                ExceptionHandlerCore.HandleException(Logger, ex, DbContext.GetErrorEmailNotificationRecipientsFailSafe(), EmailClientService, DbContext);
                return GetJsonResult(false, ex.Message);
            }
        }

        #endregion //Actions
    }
}
