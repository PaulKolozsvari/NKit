namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public class NKitSettings
    {
        #region Constructors

        public NKitSettings()
        {
            Type type = this.GetType();
            _name = type.Name;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;

        #endregion //Fields

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// The name of the application.
        /// </summary>
        [NKitSettingInfo("Application", AutoFormatDisplayName = true, Description = "The name of the application. This setting should be loaded in the GOC for it to be displayed relevant places.", CategorySequenceId = 0)]
        public string ApplicationName { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static S GetSettings<S>(IConfiguration configuration) where S : NKitSettings
        {
            return configuration.GetSection(typeof(S).Name).Get<S>();
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        public static S RegisterConfiguration<S>(IConfiguration configuration, IServiceCollection services) where S : NKitSettings
        {
            IConfigurationSection section = configuration.GetSection(typeof(S).Name);
            if (section == null)
            {
                return null;
            }
            services.Configure<S>(section);
            return GetSettings<S>(configuration);
        }

        #endregion //Methods
    }
}
