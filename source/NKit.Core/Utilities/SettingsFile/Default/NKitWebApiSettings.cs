namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public class NKitWebApiSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Sets the serializer type that is used to serialize the responses from the NKitWebApiController. 'XML' or 'JSON' is supported.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the serializer type that is used to serialize/deserialize the requests/responses from the NKitWebApiController. 'XML' or 'JSON' is supported.", CategorySequenceId = 0)]
        public SerializerType SerializerType { get; set; }

        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.", CategorySequenceId = 1)]
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Whether or not to include the stack trace in the web response when an unhandled exception occurs.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not to include the stack trace in the web response when an unhandled exception occurs.", CategorySequenceId = 2)]
        public bool IncludeExceptionStackTraceInErrorResponse { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the NKitWebApiSettings configuration section from the appsettings.json file and deserializes to an instance of NKitWebApiSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitWebApiSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitWebApiSettings>(configuration);
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        public static NKitWebApiSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitWebApiSettings>(configuration, services);
        }

        #endregion //Methods
    }
}