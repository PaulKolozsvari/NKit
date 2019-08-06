namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Diagnostics;
    using NKit.Data;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    /// <summary>
    /// A helper class that can be used to messages to a
    /// log file and Windows EventLog that will be written in the assembly's executing directory.
    /// </summary>
    public partial class Logger
    {
        #region Constructors

        public Logger()
        {
            Initialize(false, false, LoggingLevel.Normal, DEFAULT_LOG_FILE_NAME);
        }

        public Logger(
            bool logToFile,
            bool logToConsole,
            LoggingLevel loggingLevel,
            string logFileName)
        {
            Initialize(logToFile, logToConsole, loggingLevel, logFileName);
        }

        #endregion //Constructors

        protected void Initialize(
            bool logToFile,
            bool logToConsole,
            LoggingLevel loggingLevel,
            string logFileName)
        {
            _logToFile = logToFile;
            _logToConsole = logToConsole;

            _logFileName = logFileName;
            _loggingLevel = loggingLevel;
        }

        #region Constants

        /// <summary>
        /// The log file name where all errors and info etc. will be logged to in the same
        /// directory as the executing assembly.
        /// </summary>
        public static string DEFAULT_LOG_FILE_NAME = "NKitServerToolkitLog.txt";
        /// <summary>
        /// The maximun length of a message that can be loggedd to the Windows Event Log.
        /// </summary>
        //public static int MAXIMUM_EVENT_LOG_MESSAGE_SIZE = 32766;
        public static int MAXIMUM_EVENT_LOG_MESSAGE_SIZE = 10000;

        #endregion //Constants

        #region Fields

        protected bool _logToFile;
        protected bool _logToConsole;

        protected string _logFileName;
        protected LoggingLevel _loggingLevel;

        #endregion //Fields

        #region Properties

        public bool LogToFile
        {
            get { return _logToFile; }
            set { _logToFile = value; }
        }

        public bool LogToConsole
        {
            get { return _logToConsole; }
            set { _logToConsole = value; }
        }

        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
            set { _loggingLevel = value; }
        }

        public string LogFileName
        {
            get { return _logFileName; }
            set { _logFileName = value; }
        }

        #endregion //Properties

        #region Methods

        public void LogMessageToFile(LogMessage logMessage)
        {
            string logFilePath = Path.Combine(Information.GetExecutingDirectory(), _logFileName);
            using (StreamWriter writer = new StreamWriter(logFilePath, true) { AutoFlush = true })
            {
                writer.WriteLine(logMessage.ToString());
            }
        }

        public void LogSettings(Settings settings)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Application Settings:");
            result.AppendLine();
            result.Append(settings.ToString());
            LogMessage(new LogMessage(result.ToString(), LogMessageType.Information, LoggingLevel.Normal));
        }

        public virtual void LogMessage(LogMessage logMessage)
        {
            if ((_loggingLevel == LoggingLevel.None) || //Logger is configured to not log anything or ... 
                (logMessage.LoggingLevel == Logging.LoggingLevel.None) ||
                (_loggingLevel < logMessage.LoggingLevel))
            {
                /*The LogMessage's logging level is higher than what the Logger 
                 * is configured to log e.g. Logger is configured to Minimum logging
                 but the message is of LoggingLevel maximum, in which case the message
                 should not be logged.*/
                return;
            }
            if (_logToConsole)
            {
                Console.WriteLine(logMessage.ToString());
            }
            if (_logToFile && !string.IsNullOrEmpty(_logFileName))
            {
                LogMessageToFile(logMessage);
            }
        }

        #endregion //Methods
    }
}