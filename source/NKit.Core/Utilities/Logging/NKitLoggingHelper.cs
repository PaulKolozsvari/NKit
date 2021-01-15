namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.EventLog;
    using NKit.Settings.Default;

    #endregion //Using Directives

    public class NKitLoggingHelper
    {
        #region Methods

        /// <summary>
        /// Creates a LoggerFactory to create an ILogger. This method can be used to create an ILogger inside a .NET Core Startup.ConfigureServices method
        /// (where the logger has not been created since the DI container has not been created) thus allowing you to log activity that occurs inside the Startup.ConfigureServices method.
        /// </summary>
        /// <param name="categoryName">Category name for the logger to create. By convention this would typically be the name of the class that the logger is being created in.</param>
        /// <param name="loggingSettings">The IConfiguration received in the Startup class.</param>
        /// <returns></returns>
        public static ILogger CreateLogger(string categoryName, NKitLoggingSettings loggingSettings)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddDebug();
                loggingBuilder.AddEventSourceLogger();
                if (loggingSettings.LogToConsole)
                {
                    loggingBuilder.AddConsole();
                }
                if (loggingSettings.LogToWindowsEventLog)
                {
                    loggingBuilder.AddEventLog(new EventLogSettings()
                    {
                        LogName = loggingSettings.EventLogName,
                        SourceName = loggingSettings.EventSourceName,
                    });
                }
            });
            ILogger result = loggerFactory.CreateLogger(categoryName);
            return result;
        }

        #endregion //Methods
    }
}
