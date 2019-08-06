namespace NKit.Web.Client
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Web;
    using NKit.Data;

    #endregion //Using Directives

    public partial class WebServiceClientCache : EntityCacheGeneric<string, IWebService>
    {
    }
}