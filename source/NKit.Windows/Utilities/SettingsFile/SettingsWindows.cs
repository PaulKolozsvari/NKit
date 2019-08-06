namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;
    using NKit.Mmc.Forms;

    #endregion //Using Directives

    public abstract class SettingsWindows : Settings
    {
        #region Constructors

        public SettingsWindows() : base()
        {
        }

        public SettingsWindows(string filePath) : base(filePath)
        {
        }

        public SettingsWindows(string name, string filePath) : base(name, filePath)
        {
        }

        #endregion //Constructors

        #region Methods

        public EntityCacheGeneric<string, SettingItemWindows> GetSettingsByCategory(SettingsCategoryInfo settingsCategoryInfo, SettingsControlWindows settingsControl)
        {
            string categoryLower = settingsCategoryInfo.Category.Trim().ToLower();
            Type settingsType = this.GetType();
            List<SettingItemWindows> settingItems = new List<SettingItemWindows>();
            foreach (PropertyInfo p in settingsType.GetProperties())
            {
                object[] categoryAttributes = p.GetCustomAttributes(typeof(SettingInfoAttribute), true);
                if (categoryAttributes == null)
                {
                    continue;
                }
                foreach (SettingInfoAttribute c in categoryAttributes)
                {
                    if (c.Category.Trim().ToLower() == categoryLower)
                    {
                        SettingItemWindows settingItem = new SettingItemWindows(
                            c.Category,
                            p.Name,
                            EntityReader.GetPropertyValue(p.Name, this, false),
                            p.PropertyType,
                            c.AutoFormatDisplayName,
                            c.DisplayName,
                            c.Description,
                            c.CategorySequenceId,
                            c.PasswordChar,
                            settingsControl,
                            settingsCategoryInfo);
                        settingItems.Add(settingItem);
                    }
                }
            }
            string entityCacheName = string.Format("{0} {1} Settings", DataShaperWindows.ShapeCamelCaseString(settingsType.Name).Replace("Settings", "").Trim(), settingsCategoryInfo.Category);
            EntityCacheGeneric<string, SettingItemWindows> result = new EntityCacheGeneric<string, SettingItemWindows>(entityCacheName);
            settingItems.OrderBy(p => p.CategorySequenceId).ToList().ForEach(p => result.Add(p.SettingName, p));
            return result;
        }

        #endregion //Methods
    }
}
