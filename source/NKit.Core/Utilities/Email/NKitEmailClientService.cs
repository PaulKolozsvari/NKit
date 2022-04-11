namespace NKit.Utilities.Email
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Core.Data.DB.LINQ;
    using NKit.Settings.Default;

    #endregion //Using Directives

    public class NKitEmailClientService : EmailClient
    {
        #region Constructors

        public NKitEmailClientService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitWebApiControllerSettings> webApiControllerOptions,
            IOptions<NKitDbContextSettings> databaseOptions,
            IOptions<NKitEmailClientServiceSettings> emailOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            IOptions<NKitWebApiClientSettings> webApiClientOptions,
            ILogger<NKitEmailClientService> logger,
            IWebHostEnvironment environment) :
            base(emailOptions.Value.EmailNotificationsEnabled,
                emailOptions.Value.ThrowEmailFailExceptions,
                emailOptions.Value.EmailProvider,
                emailOptions.Value.ExchangeSmtpServer,
                emailOptions.Value.ExchangeSmtpUserName,
                emailOptions.Value.ExchangeSmtpPassword,
                emailOptions.Value.ExchangeSmtpPort,
                emailOptions.Value.ExchangeSmtpEnableSsl,
                emailOptions.Value.GMailSmtpServer,
                emailOptions.Value.GMailSmtpUserName,
                emailOptions.Value.GMailSmtpPassword,
                emailOptions.Value.GMailSmtpPort,
                emailOptions.Value.SenderEmailAddress,
                emailOptions.Value.SenderDisplayName,
                emailOptions.Value.ExceptionEmailSubject,
                emailOptions.Value.EmailLoggingEnabled,
                emailOptions.Value.IncludeDefaultEmailRecipients,
                emailOptions.Value.DefaultEmailRecipients)
        {
            _httpContextAccessor = httpContextAccessor;

            _generalSettings = generalOptions.Value;
            _webApiControllerSettings = webApiControllerOptions.Value;
            _dbRepositorySettings = databaseOptions.Value;
            _emailSettings = emailOptions.Value;
            _loggingSettings = loggingOptions.Value;
            _webApiClientSettings = webApiClientOptions.Value;

            _logger = logger;
            _environment = environment;
        }

        #endregion //Constructors

        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly NKitGeneralSettings _generalSettings;
        private readonly NKitWebApiControllerSettings _webApiControllerSettings;
        private readonly NKitDbContextSettings _dbRepositorySettings;
        private readonly NKitEmailClientServiceSettings _emailSettings;
        private readonly NKitLoggingSettings _loggingSettings;
        private readonly NKitWebApiClientSettings _webApiClientSettings;

        protected readonly ILogger _logger;
        protected readonly IWebHostEnvironment _environment;

        #endregion //Fields

        #region Properties

        /// <summary>
        /// HTTP Context Accessor which can be used to access the HTTP context providing information about the web request.
        /// </summary>
        protected IHttpContextAccessor HttpContextAccessor { get { return _httpContextAccessor; } }

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
        /// Logger being supplied to the controller through dependency injection.
        /// </summary>
        protected ILogger Logger { get { return _logger; } }


        /// <summary>
        /// Provides information about a web hosting environment the application is running in.
        /// </summary>
        protected IWebHostEnvironment Environment { get { return _environment; } }

        #endregion //Properties

        #region Methods

        protected override bool LogEmailNotification(MailMessage email, string subject, out string logMessage)
        {
            if (!_emailLoggingEnabled)
            {
                logMessage = null;
                return false;
            }
            logMessage = GetEmailNotificationLogMessage(email, subject);
            if (_logger != null)
            {
                _logger.LogInformation(logMessage);
            }
            return true;
        }

        /// <summary>
        /// Gets all the absolute file paths of all the files in the sub directory specified within the web root directory i.e. wwwroot.
        /// </summary>
        /// <param name="relativeDirectoryPath"></param>
        /// <returns></returns>
        public string[] GetAbsoluteFilesPathsInWebRootDirectory(string relativeDirectoryPath)
        {
            return Directory.GetFiles(Path.Combine(Environment.WebRootPath, relativeDirectoryPath));
        }

        /// <summary>
        /// Gets the absolute file path of a file in the web root directory (i.e. wwwroot) given the relative file path.
        /// </summary>
        /// <param name="relativeFilePath"></param>
        /// <returns></returns>
        public string GetAbsoluteFilePathInWebRootDirectory(string relativeFilePath)
        {
            return Path.Combine(Environment.WebRootPath, relativeFilePath);
        }

        #endregion //Methods
    }
}
