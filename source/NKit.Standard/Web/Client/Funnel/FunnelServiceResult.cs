namespace NKit.Web.Client.Funnel
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection;
    using NKit.Utilities;
    using NKit.Data;

    #endregion //Using Directives

    public partial class FunnelServiceResult
    {
        #region Constructors

        public FunnelServiceResult()
        {
        }

        public FunnelServiceResult(FunnelServiceResultCode code, string message)
        {
            Code = code;
            Message = message;
        }

        #endregion //Constructors

        #region Properties

        public FunnelServiceResultCode Code { get; set; }

        public string Message { get; set; }

        #endregion //Properties
    }
}