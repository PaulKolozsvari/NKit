namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile;

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
    }
}
