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
    public partial class LoggerWindows : Logger
    {
        #region Constructors

        public LoggerWindows() : base()
        {
        }

        public LoggerWindows(
            bool logToFile,
            bool logToWindowsEventLog,
            bool logToConsole,
            LoggingLevel loggingLevel, 
            string logFileName, 
            EventLog eventLog) : base(logToFile, logToConsole, loggingLevel, logFileName)
        {
            Initialize(logToWindowsEventLog, eventLog);
        }

        public LoggerWindows(
            bool logToFile,
            bool logToWindowsEventLog,
            bool logToConsole,
            LoggingLevel loggingLevel,
            string logFileName,  
            string eventSourceName, 
            string eventLogName) : base(logToFile, logToConsole, loggingLevel, logFileName)
        {
            Initialize(logToWindowsEventLog, new EventLog() { Source = eventSourceName, Log = eventLogName });
        }

        #endregion //Constructors

        protected void Initialize(
            bool logToWindowsEventLog,
            EventLog eventLog)
        {
            _logToWindowsEventLog = logToWindowsEventLog;
            if (eventLog == null)
            {
                return;
            }
            _eventLog = eventLog;
            //if (EventLog.SourceExists(_eventLog.Source))
            //{
            //    EventLog.DeleteEventSource(_eventLog.Source);
            //}
            //if (EventLog.Exists(_eventLog.Log))
            //{
            //    EventLog.Delete(_eventLog.Log);
            //}
            if (!System.Diagnostics.EventLog.SourceExists(_eventLog.Source))
            {
                if (EventLog.Exists(_eventLog.Log))
                {
                    EventLog.Delete(_eventLog.Log);
                }
                System.Diagnostics.EventLog.CreateEventSource(_eventLog.Source, _eventLog.Log);
            }
        }

        #region Fields

        protected bool _logToWindowsEventLog;
        protected EventLog _eventLog;

        #endregion //Fields

        #region Properties

        public bool LogToWindowsEventLog
        {
            get { return _logToWindowsEventLog; }
            set { _logToWindowsEventLog = value; }
        }

        public EventLog EventLog
        {
            get { return _eventLog; }
            set { _eventLog = value; }
        }

        #endregion //Properties

        #region Methods

        public void LogMessageToFile(LogMessage logMessage)
        {
            string logFilePath = Path.Combine(InformationWindows.GetExecutingDirectory(), _logFileName);
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

        public override void LogMessage(LogMessage logMessage)
        {
            base.LogMessage(logMessage);
            if (_logToWindowsEventLog && (_eventLog != null))
            {
                string logMessageString = logMessage.ToString();
                if (logMessageString.Length > MAXIMUM_EVENT_LOG_MESSAGE_SIZE)
                {
                    logMessageString = logMessageString.Substring(0, MAXIMUM_EVENT_LOG_MESSAGE_SIZE);
                }
                switch (logMessage.LogMessageType)
                {
                    case LogMessageType.Exception:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.Error);
                        break;
                    case LogMessageType.Error:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.Error);
                        break;
                    case LogMessageType.Information:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.Information);
                        break;
                    case LogMessageType.Warning:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.Warning);
                        break;
                    case LogMessageType.SuccessAudit:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.SuccessAudit);
                        break;
                    case LogMessageType.FailureAudit:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.FailureAudit);
                        break;
                    case LogMessageType.ProgressAudit:
                        _eventLog.WriteEntry(logMessageString, EventLogEntryType.Information);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion //Methods
    }
}