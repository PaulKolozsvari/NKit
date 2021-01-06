namespace NKit.Core.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using NKit.Data;

    #endregion //Using Directives

    public class NKitConsoleLogger : ILogger
    {
        #region Constructors

        /// <summary>
        /// A custom ILogger providing addtional functionality like logging an exception and all its details and allowing the user to change the Console color.
        /// https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configuration"></param>
        public NKitConsoleLogger(string name, NKitConsoleLoggerConfiguration configuration)
        {
            DataValidator.ValidateStringNotEmptyOrNull(name, nameof(name), nameof(NKitConsoleLogger));
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(NKitConsoleLogger));
            _name = name;
            _configuration = configuration;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;
        protected NKitConsoleLoggerConfiguration _configuration;

        #endregion //Fields

        #region Methods

        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == _configuration.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            if (_configuration.EventId == 0 || _configuration.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = _configuration.Color;
                Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.WriteLine($"     {_name} - {formatter(state, exception)}");
            }
        }

        #endregion //Methods
    }
}
