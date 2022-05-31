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
    using Microsoft.Extensions.DependencyInjection;

    #endregion //Using Directives

    public class NKitLoggingManager<D, E> where D : NKitDbContext where E : NKitEmailClientService
    {
        #region Constructors

        public NKitLoggingManager(
            IServiceProvider serviceProvider,
            IOptions<NKitLoggingSettings> loggingOptions,
            E emailClientService)
        {
            DataValidator.ValidateObjectNotNull(serviceProvider, nameof(serviceProvider), nameof(NKitLoggingManager<D, E>));
            DataValidator.ValidateObjectNotNull(loggingOptions, nameof(loggingOptions), nameof(NKitLoggingManager<D, E>));
            DataValidator.ValidateObjectNotNull(emailClientService, nameof(emailClientService), nameof(NKitLoggingManager<D, E>));
            _serviceProvider = serviceProvider;
            _loggingSettings = loggingOptions.Value;
            _emailClientService = emailClientService;
        }

        #endregion //Constructors

        #region Fields

        private IServiceProvider _serviceProvider;
        private NKitLoggingSettings _loggingSettings;
        private E _emailClientService;

        #endregion //Fields

        #region Properties

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        protected NKitLoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
        }

        public E EmailClientService
        {
            get { return _emailClientService; }
        }

        #endregion //Properties


        #region Methods

        protected ILogger CreateLogger(string category)
        {
            return NKitLoggingHelper.CreateLogger(category, _loggingSettings); ;
        }

        public virtual void Log(
            string category,
            string logMessage,
            LogMessageType logMessageType,
            string source,
            string className,
            string functionName,
            string stackTrace,
            int eventId)
        {
            CreateLogger(category).LogInformation(logMessage);
            if (_loggingSettings.LogToNKitLogEntryDatabaseTable)
            {
                using (IServiceScope scope = ServiceProvider.CreateScope())
                {
                    using(D dbContext = scope.ServiceProvider.GetService<D>())
                    {
                        dbContext.LogMessageToNKitLogEntry(logMessage, source, className, functionName, stackTrace, eventId, logMessageType.ToString());
                    }
                }
            }
        }

        #endregion //Methods
    }
}
