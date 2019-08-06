namespace NKit.Web.Service
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion //Using Directives

    public class ServiceFunctionResultWindows<E> : ServiceResultWindows
    {
        #region Constructors

        public ServiceFunctionResultWindows()
        {
            Code = ServiceResultCodeWindows.Success;
            Message = null;
        }

        public ServiceFunctionResultWindows(ServiceResultWindows result)
        {
            Code = result.Code;
            Message = result.Message;
        }

        #endregion //Constructors

        #region Properties

        public E Contents { get; set; }

        #endregion //Properties
    }
}