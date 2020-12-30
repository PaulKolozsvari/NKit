namespace NKit.Core.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using NKit.Data;

    #endregion //Using Directives

    public class AppLogger
    {
        #region Fields

        private static ILoggerFactory _loggerFactory;

        #endregion //Fields

        #region Properties

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = new LoggerFactory();
                }
                return _loggerFactory;
            }
            set
            {
                DataValidator.ValidateObjectNotNull(value, nameof(LoggerFactory), nameof(AppLogger));
                _loggerFactory = value;
            }
        }

        #endregion //Properties

        #region Methods

        public static ILogger CreateLogger()
        {
            return CreateLogger(nameof(AppLogger));
        }

        public static ILogger CreateLogger(string cateforyName)
        {
            return LoggerFactory.CreateLogger(cateforyName);
        }

        #endregion //Methods
    }
}
