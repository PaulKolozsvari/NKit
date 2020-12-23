namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Logging;

    #endregion //Using Directives

    public class LoggingSettings : Settings
    {
        #region Logging

        /// <summary>
        /// Whether or not to log to a text log file in the executing directory.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to a text log file in the executing directory.", CategorySequenceId = 0)]
        public bool LogToFile { get; set; }

        /// <summary>
        /// Whether or not to log to the Windows Event Log.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the Windows Event Log.", CategorySequenceId = 1)]
        public bool LogToWindowsEventLog { get; set; }

        /// <summary>
        /// Whether or not to log to the console.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the console.", CategorySequenceId = 2)]
        public bool LogToConsole { get; set; }

        /// <summary>
        /// The name of the text log file to log to. The log file is written in the executing directory.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the text log file to log to. The log file is written in the executing directory.", CategorySequenceId = 3)]
        public string LogFileName { get; set; }

        /// <summary>
        /// The name of the event source to use when logging to the Windows Event Log.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the event source to use when logging to the Windows Event Log.", CategorySequenceId = 4)]
        public string EventSourceName { get; set; }

        /// <summary>
        /// The name of the Windows Event Log to log to.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the Windows Event Log to log to.", CategorySequenceId = 5)]
        public string EventLogName { get; set; }

        /// <summary>
        /// The extent of messages being logged: None = logging is disabled, Minimum = logs server start/stop and exceptions, Normal = logs additional information messages, Maximum = logs all requests and responses to the server.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The extent of messages being logged: None = logging is disabled, Minimum = logs server start/stop and exceptions, Normal = logs additional information messages, Maximum = logs all requests and responses to the server.", CategorySequenceId = 6)]
        public LoggingLevel LoggingLevel { get; set; }

        #endregion //Logging        
    }
}
