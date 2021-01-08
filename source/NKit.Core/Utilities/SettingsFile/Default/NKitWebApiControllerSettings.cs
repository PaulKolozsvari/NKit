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

    public class NKitWebApiControllerSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Sets the serializer type that is used to serialize the responses from the NKitWebApiController. 'XML' or 'JSON' is supported.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the serializer type that is used to serialize/deserialize the requests/responses from the NKitWebApiController. 'XML' or 'JSON' is supported.", CategorySequenceId = 0)]
        public SerializerType SerializerType { get; set; }

        /// <summary>
        /// Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.", CategorySequenceId = 1)]
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Whether or not the log requests to the NKitWebApiController in the logger.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log requests to the NKitWebApiController in the logger.", CategorySequenceId = 2)]
        public bool LogRequests { get; set; }

        /// <summary>
        /// Whether or not the log reponses from the NKitWebApiController in the logger.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log reponses from the NKitWebApiController in the logger.", CategorySequenceId = 3)]
        public bool LogResponses { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the NKitWebApiSettings configuration section from the appsettings.json file and deserializes to an instance of NKitWebApiSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitWebApiControllerSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitWebApiControllerSettings>(configuration);
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        public static NKitWebApiControllerSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitWebApiControllerSettings>(configuration, services);
        }

        #endregion //Methods
    }
}