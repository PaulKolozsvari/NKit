﻿namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.Logging;

    #endregion //Using Directives

    public class NKitLoggingSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Whether or not to log to the NKitLogEntry database table.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = false, DisplayName = "Log to NKit Log Entry Database Table", Description = "Whether or not to log to the NKitLogEntry database table.", CategorySequenceId = 0)]
        public bool LogToNKitLogEntryDatabaseTable { get; set; }

        /// <summary>
        /// Whether or not to log to a text log file in the executing directory.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to a text log file in the executing directory.", CategorySequenceId = 2)]
        public bool LogToFile { get; set; }

        /// <summary>
        /// Whether or not to log to the Windows Event Log.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the Windows Event Log.", CategorySequenceId = 2)]
        public bool LogToWindowsEventLog { get; set; }

        /// <summary>
        /// Whether or not to log to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the console.", CategorySequenceId = 3)]
        public bool LogToConsole { get; set; }

        /// <summary>
        /// The name of the text log file to log to. The log file is written in the executing directory.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the text log file to log to. The log file is written in the executing directory.", CategorySequenceId = 4)]
        public string LogFileName { get; set; }

        /// <summary>
        /// The name of the event source to use when logging to the Windows Event Log.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the event source to use when logging to the Windows Event Log.", CategorySequenceId = 5)]
        public string EventSourceName { get; set; }

        /// <summary>
        /// The name of the Windows Event Log to log to.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the Windows Event Log to log to.", CategorySequenceId = 6)]
        public string EventLogName { get; set; }

        /// <summary>
        /// The color to use when logging information entries to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The color to use when logging information entries to the console.", CategorySequenceId = 7)]
        public ConsoleColor ConsoleInformationLogEntriesColor { get; set; }

        /// <summary>
        /// The color to use when logging error entries to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The color to use when logging error entries to the console.", CategorySequenceId = 7)]
        public ConsoleColor ConsoleErrorLogEntriesColor { get; set; }

        /// <summary>
        /// The color to use when logging warning entries to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The color to use when logging warning entries to the console.", CategorySequenceId = 7)]
        public ConsoleColor ConsoleWarningLogEntriesColor { get; set; }

        /// <summary>
        /// The color to use when logging critical entries to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The color to use when logging critical entries to the console.", CategorySequenceId = 7)]
        public ConsoleColor ConsoleCriticalLogEntriesColor { get; set; }

        /// <summary>
        /// The color to use when logging debug entries to the console.
        /// </summary>
        [NKitSettingInfo("Logging", AutoFormatDisplayName = true, Description = "The color to use when logging debug entries to the console.", CategorySequenceId = 7)]
        public ConsoleColor ConsoleDebugEntriesLogEntriesColor { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the NKitLoggingSettings configuration section from the appsettings.json file and deserializes to an instance of NKitLoggingSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitLoggingSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitLoggingSettings>(configuration);
        }

        /// <summary>
        /// Register Configuration from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static NKitLoggingSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitLoggingSettings>(configuration, services);
        }

        #endregion //Methods
    }
}