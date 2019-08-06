namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NKit.Data;
using NKit.Mmc;
using NKit.Mmc.Forms;

    #endregion //Using Directives

    public class SettingItemWindows : SettingItem
    {
        #region Constructors

        public SettingItemWindows(
            string category,
            string settingName,
            object settingValue,
            Type settingType,
            bool autoFormatDisplayName,
            string settingDisplayName,
            string settingDescription,
            int categorySequenceId,
            char passwordChar,
            SettingsControlWindows settingControl,
            SettingsCategoryInfo settingsCategoryInfo) : base(
                category, settingName, settingValue, settingType, autoFormatDisplayName, settingDisplayName, settingDescription, categorySequenceId, passwordChar, settingsCategoryInfo)
        {
            _listViewItem = new ListViewItem(_settingDisplayName);
            if (_passwordChar != '\0')
            {
                _listViewItem.SubItems.Add(DataShaperWindows.MaskPasswordString(_settingValue.ToString(), _passwordChar));
            }
            else
            {
                _listViewItem.SubItems.Add(_settingValue != null ? _settingValue.ToString() : null);
            }
            _listViewItem.SubItems.Add(_settingDescription);
            _listViewItem.Tag = this;
            _settingControl = settingControl;
        }

        #endregion //Constructors

        #region Fields

        protected ListViewItem _listViewItem;
        protected SettingsControlWindows _settingControl;

        #endregion //Fields

        #region Properties

        public ListViewItem ListViewItem
        {
            get { return _listViewItem; }
        }

        #endregion //Properties

        #region Methods

        public void RefreshSettingsByCategory(string settingValue)
        {
            _listViewItem.SubItems[1].Text = settingValue;
            _settingControl.RefreshData();
        }

        #endregion //Methods
    }
}