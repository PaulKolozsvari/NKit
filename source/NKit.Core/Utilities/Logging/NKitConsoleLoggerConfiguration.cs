namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    #endregion //Using Directives

    public class NKitConsoleLoggerConfiguration
    {
        #region Properties

        public int EventId { get; set; }

        public LogLevel LogLevel { get; set; }

        public ConsoleColor Color { get; set; }

        #endregion //Properties
    }
}
