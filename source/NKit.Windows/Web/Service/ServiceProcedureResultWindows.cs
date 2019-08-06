namespace NKit.Web.Service
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion //Using Directives

    public class ServiceProcedureResultWindows : ServiceResultWindows
    {
        #region Constructors

        public ServiceProcedureResultWindows()
        {
            Code = ServiceResultCodeWindows.Success;
            Message = null;
        }

        public ServiceProcedureResultWindows(ServiceResultWindows result)
        {
            Code = result.Code;
            Message = result.Message;
        }

        #endregion //Constructors
    }
}