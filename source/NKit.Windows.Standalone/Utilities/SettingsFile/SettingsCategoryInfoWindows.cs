namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public class SettingsCategoryInfoWindows : SettingsCategoryInfo
    {
        #region Constructors

        public SettingsCategoryInfoWindows(SettingsWindows settings, string category) : base(settings, category)
        {
        }

        #endregion //Constructors

        #region Properties

        public SettingsWindows Settings
        {
            get { return _settings as SettingsWindows; }
        }

        public string Category
        {
            get { return _category; }
        }

        #endregion //Properties
    }
}