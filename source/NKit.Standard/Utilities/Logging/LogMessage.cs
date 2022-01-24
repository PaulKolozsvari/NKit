namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion //Using Directives

    /// <summary>
    /// A class to hold some a message and the date/time. To be used when logging.
    /// </summary>
    public partial class LogMessage
    {
        #region Constructors

        public LogMessage(string message, LogMessageType logMessageType, LoggingLevel loggingLevel) : 
            this(message, logMessageType, loggingLevel, DateTime.Now, null)
        {
        }

        public LogMessage(string message, LogMessageType logMessageType, LoggingLevel loggingLevel, Nullable<ConsoleColor> consoleColor) : 
            this(message, logMessageType, loggingLevel, DateTime.Now, consoleColor)
        {
        }

        public LogMessage(string message, LogMessageType logMessageType, LoggingLevel loggingLevel, DateTime datetime) :
            this(message, logMessageType, loggingLevel, datetime, null)
        {
        }

        public LogMessage(string message, LogMessageType logMessageType, LoggingLevel loggingLevel, DateTime datetime, Nullable<ConsoleColor> consoleColor)
        {
            _message = message;
            _logMessageType = logMessageType;
            _loggingLevel = loggingLevel;
            _date = datetime;
            _consoleColor = consoleColor;
        }

        #endregion //Constructors

        #region Fields

        protected string _message;
        protected LogMessageType _logMessageType;
        protected LoggingLevel _loggingLevel;
        protected DateTime _date;
        protected Nullable<ConsoleColor> _consoleColor;

        #endregion //Fields

        #region Properties

        /// <summary>
        /// The message to log.
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        public LogMessageType LogMessageType
        {
            get { return _logMessageType; }
        }

        public ConsoleColor ConsoleColor
        {
            get
            {
                if (_consoleColor.HasValue)
                {
                    return _consoleColor.Value;
                }
                switch (_logMessageType)
                {
                    case LogMessageType.Exception:
                        return ConsoleColor.Red;
                    case LogMessageType.Error:
                        return ConsoleColor.Red;
                    case LogMessageType.Information:
                        return ConsoleColor.White;
                    case LogMessageType.Warning:
                        return ConsoleColor.Yellow;
                    case LogMessageType.SuccessAudit:
                        return ConsoleColor.DarkYellow;
                    case LogMessageType.FailureAudit:
                        return ConsoleColor.DarkRed;
                    case LogMessageType.ProgressAudit:
                        return ConsoleColor.Gray;
                    default:
                        return ConsoleColor.White;
                }
            }
        }
        public LoggingLevel LoggingLevel
        {
            get { return _loggingLevel; }
        }

        /// <summary>
        /// The date/time to record in the log when logging this LogInfo.
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(string.Format("Date Time : {0}", _date.ToString()));
            result.AppendLine(string.Format("{0} : {1}", _logMessageType.ToString(), _message));
            return result.ToString();
        }

        #endregion //Methods
    }
}