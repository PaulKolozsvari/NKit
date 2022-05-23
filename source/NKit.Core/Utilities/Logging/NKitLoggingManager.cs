namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Settings.Default;
    using NKit.Utilities.Email;

    #endregion //Using Directives

    public class NKitLoggingManager<D, E> where D : NKitDbContext where E : NKitEmailClientService
    {
        #region Constructors

        public NKitLoggingManager(IOptions<NKitLoggingSettings> loggingOptions, ILogger<NKitLoggingManager<D, E>> logger, D dbContext, E emailClientService)
        {
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitLoggingManager<D, E>));
            DataValidator.ValidateObjectNotNull(logger, nameof(logger), nameof(NKitLoggingManager<D, E>));
            DataValidator.ValidateObjectNotNull(dbContext, nameof(dbContext), nameof(NKitLoggingManager<D, E>));
            DataValidator.ValidateObjectNotNull(emailClientService, nameof(emailClientService), nameof(NKitLoggingManager<D, E>));
            _loggingSettings = loggingOptions.Value;
            _dbContext = dbContext;
            _logger = logger;
            _emailClientService = emailClientService;
        }

        #endregion //Constructors

        #region Fields

        private NKitLoggingSettings _loggingSettings;
        private ILogger<NKitLoggingManager<D, E>> _logger;
        private D _dbContext;
        private E _emailClientService;

        #endregion //Fields

        #region Properties

        protected NKitLoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
        }

        public ILogger<NKitLoggingManager<D, E>> Logger
        {
            get { return _logger; }
        }

        public D DbContext
        {
            get { return _dbContext; }
        }

        public E EmailClientService
        {
            get { return _emailClientService; }
        }

        #endregion //Properties

        #region Methods

        public virtual void Log(
            string logMessage,
            LogMessageType logMessageType,
            string className,
            string functionName,
            string stackTrace,
            int eventId)
        {
            _logger.LogInformation(logMessage);
            if (_loggingSettings.LogToNKitLogEntryDatabaseTable && _dbContext != null)
            {
                _dbContext.LogMessageToNKitLogEntry(logMessage, Information.GetExecutingAssemblyName(), className, functionName, stackTrace, eventId, logMessageType.ToString());
            }
        }

        #endregion //Methods
    }
}
