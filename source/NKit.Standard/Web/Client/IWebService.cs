namespace NKit.Web.Client
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public partial interface IWebService
    {
        #region Properties

        string Name { get; set; }

        string WebServiceBaseUrl { get; set; }

        #endregion //Properties
    }
}