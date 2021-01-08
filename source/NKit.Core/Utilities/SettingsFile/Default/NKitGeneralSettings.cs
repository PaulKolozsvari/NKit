namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public class NKitGeneralSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// The name of the application.
        /// </summary>
        [NKitSettingInfo("Application", AutoFormatDisplayName = true, Description = "The name of the application.", CategorySequenceId = 0)]
        public string ApplicationName { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The Configuration object is created read from based on the appsettings.json. The appsettings.json file name is determined by reading the ASPNETCORE_ENVIRONMENT variable i.e. appsettings.{environment}.json or appsettings.json when the environment variable is not set.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitGeneralSettings GetSettings()
        {
            return GetSettings<NKitGeneralSettings>();
        }

        /// <summary>
        /// Reads the NKitDatabaseSettings configuration section from the appsettings.json file and deserializes to an instance of NKitDatabaseSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitGeneralSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitGeneralSettings>(configuration);
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static NKitGeneralSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitGeneralSettings>(configuration, services);
        }

        #endregion //Methods
    }
}
