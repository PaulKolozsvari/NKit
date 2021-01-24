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

    public class NKitWebApiControllerSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Sets the host URL which this API can be accessed from.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the host URL which this API can be accessed from.", CategorySequenceId = 0)]
        public string HostUrl { get; set; }

        /// <summary>
        /// Sets the serializer type that is used to serialize the responses from the NKitWebApiController. 'XML' or 'JSON' is supported.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the serializer type that is used to serialize/deserialize the requests/responses from the NKitWebApiController. 'XML' or 'JSON' is supported.", CategorySequenceId = 1)]
        public SerializerType SerializerType { get; set; }

        /// <summary>
        /// Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Sets the default content type that the NKitWebApiController returns e.g. 'text/plain', 'application/json' or 'application/xml' is supported.", CategorySequenceId = 2)]
        public string ResponseContentType { get; set; }

        /// <summary>
        /// Whether or not the log requests to the NKitWebApiController in the logger.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log requests to the NKitWebApiController in the logger.", CategorySequenceId = 3)]
        public bool LogRequests { get; set; }

        /// <summary>
        /// Whether or not the log reponses from the NKitWebApiController in the logger.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log reponses from the NKitWebApiController in the logger.", CategorySequenceId = 4)]
        public bool LogResponses { get; set; }

        /// <summary>
        /// Whether or not the log requests to the database table NKitLogEntry.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log requests to the database table NKitLogEntry.", CategorySequenceId = 5)]
        public bool LogRequestsInDatabaseNKitLogEntry { get; set; }

        /// <summary>
        /// Whether or not the log responses to the database table NKitLogEntry.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not the log responses from to the database table NKitLogEntry.", CategorySequenceId = 6)]
        public bool LogResponsesInDatabaseNKitLogEntry { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The Configuration object is created read from based on the appsettings.json. The appsettings.json file name is determined by reading the ASPNETCORE_ENVIRONMENT variable i.e. appsettings.{environment}.json or appsettings.json when the environment variable is not set.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitWebApiControllerSettings GetSettings()
        {
            return GetSettings<NKitWebApiControllerSettings>();
        }

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