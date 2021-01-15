namespace NKit.Settings.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.Serialization;

    #endregion //Using Directives

    public class NKitHttpExceptionHandlerMiddlewareSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Sets the serializer type that is used to serialize the responses from the NKitWebApiController. 'XML' or 'JSON' is supported.
        /// </summary>
        [NKitSettingInfo("HTTP Exception Handler Middleware", AutoFormatDisplayName = true, Description = "Sets the serializer type that is used to serialize/deserialize the requests/responses from the NKitHttpExceptionHandlerMiddleware. 'XML' or 'JSON' is supported.", CategorySequenceId = 0)]
        public SerializerType SerializerType { get; set; }

        /// <summary>
        /// Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.
        /// </summary>
        [NKitSettingInfo("HTTP Exception Handler Middleware", AutoFormatDisplayName = true, Description = "Sets the default content type that the NKitHttpExceptionHandlerMiddlewarereturns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.", CategorySequenceId = 1)]
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Whether or not to include the stack trace in the response returned by the NKitExceptionHandlerMiddleware when an unhandled exception occurs.
        /// </summary>
        [NKitSettingInfo("HTTP Exception Handler Middleware", AutoFormatDisplayName = true, Description = "Whether or not to include the stack trace in the web response when an unhandled exception occurs.", CategorySequenceId = 2)]
        public bool IncludeStackTraceInExceptionResponse { get; set; }

        /// <summary>
        /// Whether or to to include the stack trace in the Logger entry when an exception is logged
        /// </summary>
        [NKitSettingInfo("HTTP Exception Handler Middleware", AutoFormatDisplayName = true, Description = "Whether or to to include the stack trace in the Logger entry when an exception is logged.", CategorySequenceId = 3)]
        public bool IncludeExceptionStackTraceInLoggerEntry { get; set; }

        /// <summary>
        /// Whether or to to include the stack trace in the NKitLogEntry when an exception is logged to the database.
        /// </summary>
        [NKitSettingInfo("HTTP Exception Handler Middleware", AutoFormatDisplayName = true, Description = "Whether or to to include the stack trace in the NKitLogEntry table when an exception is logged to the database.", CategorySequenceId = 4)]
        public bool IncludeExceptionStackTraceInDatabaseNKitLogEntry { get; set; }

        /// <summary>
        /// Whether or not to exception in an email to the default email recipients list specified in the NKitEmailServiceSettings.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to send emails on exceptions handled by the Exception Handler.", CategorySequenceId = 5)]
        public bool SendEmailOnException { get; set; }

        /// <summary>
        /// Whether or not add a line to every exception email sent out that includes the hostname of the machine running this software and thereby initiating the email.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not add a line to every exception email sent out that includes the hostname of the machine running this software and thereby initiating the email.", CategorySequenceId = 6)]
        public bool AppendHostNameToExceptionEmails { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The Configuration object is created read from based on the appsettings.json. The appsettings.json file name is determined by reading the ASPNETCORE_ENVIRONMENT variable i.e. appsettings.{environment}.json or appsettings.json when the environment variable is not set.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitHttpExceptionHandlerMiddlewareSettings GetSettings()
        {
            return GetSettings<NKitHttpExceptionHandlerMiddlewareSettings>();
        }

        /// <summary>
        /// Reads the NKitHttpExceptionHandlerMiddlewareSettings configuration section from the appsettings.json file and deserializes to an instance of NKitHttpExceptionHandlerMiddlewareSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitHttpExceptionHandlerMiddlewareSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitHttpExceptionHandlerMiddlewareSettings>(configuration);
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        public static NKitHttpExceptionHandlerMiddlewareSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitHttpExceptionHandlerMiddlewareSettings>(configuration, services);
        }

        #endregion //Methods
    }
}
