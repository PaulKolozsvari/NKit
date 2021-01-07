namespace NKit.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.EventLog;
    using NKit.Core.Utilities;
    using NKit.Extensions;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public static class LoggingHostBuilderExtensions
    {
        #region Methods

        public static IHostBuilder ConfigureNKitLogging(this IHostBuilder hostBuilder)
        {
            string appSettingsFileName = NKitInformation.GetAspNetCoreEnvironmentAppSettingsFileName();
            IConfigurationRoot configurationBuilder = new ConfigurationBuilder().AddJsonFile(appSettingsFileName).Build();
            NKitLoggingSettings loggingSettings = NKitLoggingSettings.GetSettings(configurationBuilder);
            hostBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddDebug();
                loggingBuilder.AddEventSourceLogger();
                if (loggingSettings.LogToConsole)
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddNKitLogger(configuration =>
                    {
                        configuration.Color = loggingSettings.ConsoleInformationLogEntriesColor;
                        configuration.LogLevel = LogLevel.Information;
                    });
                    loggingBuilder.AddNKitLogger(configuration =>
                    {
                        configuration.Color = loggingSettings.ConsoleErrorLogEntriesColor;
                        configuration.LogLevel = LogLevel.Error;
                    });
                    loggingBuilder.AddNKitLogger(configuration =>
                    {
                        configuration.Color = loggingSettings.ConsoleWarningLogEntriesColor;
                        configuration.LogLevel = LogLevel.Warning;
                    });
                    loggingBuilder.AddNKitLogger(configuration =>
                    {
                        configuration.Color = loggingSettings.ConsoleCriticalLogEntriesColor;
                        configuration.LogLevel = LogLevel.Critical;
                    });
                    loggingBuilder.AddNKitLogger(configuration =>
                    {
                        configuration.Color = loggingSettings.ConsoleDebugEntriesLogEntriesColor;
                        configuration.LogLevel = LogLevel.Debug;
                    });
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
            return hostBuilder;
        }

        #endregion //Methods
    }
}
