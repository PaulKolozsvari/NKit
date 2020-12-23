namespace NKit.Utilities.SettingsFile
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Data;

    #endregion //Using Directives

    public partial class SettingsCache : EntityCacheGeneric<string, Settings>
    {
    }
}