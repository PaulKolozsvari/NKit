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
    using NKit.Extensions;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public static class LoggingHostBuilderExtensions
    {
        #region Methods

        /// <summary>
        /// Configures the logging providers based on the NKitLoggingSettings in the appsettings.xml file.
        /// For more information: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0
        /// Logging Levels: https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-5.0
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureNKitLogging(this IHostBuilder hostBuilder)
        {
            NKitLoggingSettings loggingSettings = NKitLoggingSettings.GetSettings();
            hostBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddDebug();
                loggingBuilder.AddEventSourceLogger();
                loggingBuilder.SetMinimumLevel(loggingSettings.MinimumLogLevel); //https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-5.0
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
