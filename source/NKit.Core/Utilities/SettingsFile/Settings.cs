﻿namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities;

    #endregion //Using Directives

    public class Settings
    {
        #region Constructors

        public Settings()
        {
            Type type = this.GetType();
            _name = type.Name;
            _filePath = Path.Combine(Information.GetExecutingDirectory(), string.Format("{0}.xml", type.Name));
        }

        public Settings(string filePath)
        {
            Type type = this.GetType();
            _name = type.Name;
            _filePath = filePath;
        }

        public Settings(string name, string filePath)
        {
            _name = name;
            _filePath = filePath;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;
        protected string _filePath;

        #endregion //Fields

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        /// <summary>
        /// The name of the application.
        /// </summary>
        [SettingInfo("Application", AutoFormatDisplayName = true, Description = "The name of the application. This setting should be loaded in the GOC for it to be displayed relevant places.", CategorySequenceId = 0)]
        public string ApplicationName { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static S GetSettings<S>(IConfiguration configuration) where S : Settings
        {
            return configuration.GetSection(typeof(S).Name).Get<S>();
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterConfiguration<S>(IConfiguration configuration, IServiceCollection services) where S : Settings
        {
            services.Configure<S>(configuration.GetSection(typeof(S).Name));
        }

        #endregion //Methods
    }
}
