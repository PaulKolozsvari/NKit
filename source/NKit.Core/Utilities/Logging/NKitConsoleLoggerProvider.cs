namespace NKit.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using NKit.Data;

    #endregion //Using Directives

    /// <summary>
    /// Responsible for creating NKitLogger instances per category.
    /// https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
    /// </summary>
    public class NKitConsoleLoggerProvider : ILoggerProvider
    {
        #region Constructors

        public NKitConsoleLoggerProvider(NKitConsoleLoggerConfiguration configuration)
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(NKitConsoleLoggerProvider));
            _configuration = configuration;
            _loggers = new ConcurrentDictionary<string, NKitConsoleLogger>();
        }

        #endregion //Constructors

        #region Fields

        protected readonly NKitConsoleLoggerConfiguration _configuration;
        protected readonly ConcurrentDictionary<string, NKitConsoleLogger> _loggers;

        #endregion //Fields

        #region Methods

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new NKitConsoleLogger(name, _configuration));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        #endregion //Methods
    }
}
