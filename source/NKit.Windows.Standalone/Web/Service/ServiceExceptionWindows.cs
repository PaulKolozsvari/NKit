namespace NKit.Web.Service
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion //Using Directives

    public class ServiceExceptionWindows : Exception
    {
        #region Constructors

        public ServiceExceptionWindows(string message, ServiceResultCodeWindows code) : base(message)
        {
            _result = new ServiceResultWindows()
            {
                Message = message,
                Code = code
            };
        }

        #endregion //Constructors

        #region Fields

        private ServiceResultWindows _result;

        #endregion //Fields

        #region Properties

        public ServiceResultWindows Result
        {
            get { return _result; }
        }

        #endregion //Properties
    }
}