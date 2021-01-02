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

    public class NKitWebApiSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Whether or not to include the stack trace in the web response when an unhandled exception occurs.
        /// </summary>
        [NKitSettingInfo("Web API", AutoFormatDisplayName = true, Description = "Whether or not to include the stack trace in the web response when an unhandled exception occurs.", CategorySequenceId = 0)]
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
        public static void RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            RegisterConfiguration<NKitWebApiSettings>(configuration, services);
        }

        #endregion //Methods
    }
}
