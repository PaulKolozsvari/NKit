namespace NKit.Core.Utilities.Logging
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    #endregion //Using Directives

    public static class ILoggingBuilderExtensions
    {
        #region Methods

        /// <summary>
        /// Adds an NKitLogger provider to the ILoggingBuilder. The provider is responsible for creating the loggers.
        /// </summary>
        public static ILoggingBuilder AddNKitLogger(this ILoggingBuilder builder)
        {
            return builder.AddNKitLogger(new NKitConsoleLoggerConfiguration());
        }

        /// <summary>
        /// Adds an NKitLogger provider to the ILoggingBuilder. The provider is responsible for creating the loggers.
        /// </summary>
        public static ILoggingBuilder AddNKitLogger(this ILoggingBuilder builder, Action<NKitConsoleLoggerConfiguration> configureMethod)
        {
            NKitConsoleLoggerConfiguration configuration = new NKitConsoleLoggerConfiguration();
            configureMethod(configuration);
            return builder.AddNKitLogger(configuration);
        }

        /// <summary>
        /// Adds an NKitLogger provider to the ILoggingBuilder. The provider is responsible for creating the loggers.
        /// </summary>
        public static ILoggingBuilder AddNKitLogger(
            this ILoggingBuilder builder,
            NKitConsoleLoggerConfiguration configuration)
        {
            builder.AddProvider(new NKitConsoleLoggerProvider(configuration));
            return builder;
        }

        #endregion //Methods
    }
}
