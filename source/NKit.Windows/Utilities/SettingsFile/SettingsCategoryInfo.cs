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

    public partial class SettingsCategoryInfo
    {
        #region Constructors

        public SettingsCategoryInfo(Settings settings, string category)
        {
            _settings = settings;
            if (string.IsNullOrEmpty(category))
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be set to null when constructing a {1}.",
                    EntityReaderGeneric<SettingsCategoryInfo>.GetPropertyName(p => p.Category, false),
                    this.GetType().FullName));
            }
            _category = category;
        }

        #endregion //Constructors

        #region Fields

        protected Settings _settings;
        protected string _category;

        #endregion //Fields

        #region Properties

        public Settings Settings
        {
            get { return _settings; }
        }

        public string Category
        {
            get { return _category; }
        }

        #endregion //Properties
    }
}