namespace NKit.Extensions.WebService.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Utilities.Serialization;
    using NKit.Web.Client;

    #endregion //Using Directives

    public class AfterWebServiceRequestArgsWindows : WebServiceRequestArgsWindows
    {
        #region Constructors

        public AfterWebServiceRequestArgsWindows(
            string uri,
            HttpVerb method,
            string contentType,
            string accept,
            string userAgent,
            string inputString,
            ISerializer inputSerializer,
            ISerializer outputSerializer)
            : base(
                uri,
                method,
                contentType,
                accept,
                userAgent,
                inputString,
                inputSerializer,
                outputSerializer)
        {
        }

        #endregion //Constructors

        #region Fields

        protected bool _overrideHttpStatusResult;
        protected HttpStatusCode _httpStatusCode;
        protected string _httpStatusMessage;
        protected bool _overrideResponseOutputContent;
        protected string _outputString;

        #endregion //Fields

        #region Properties

        public bool OverrideHttpStatusResult
        {
            get { return _overrideHttpStatusResult; }
            set { _overrideHttpStatusResult = true; }
        }

        public HttpStatusCode HttpStatusCode
        {
            get { return _httpStatusCode; }
            set { _httpStatusCode = value; }
        }

        public string HttpStatusMessage
        {
            get { return _httpStatusMessage; }
            set { _httpStatusMessage = value; }
        }

        public bool OverrideResponseOutputContent
        {
            get { return _overrideResponseOutputContent; }
            set { _overrideResponseOutputContent = value; }
        }

        public string OutputString
        {
            get { return _outputString; }
            set { _outputString = value; }
        }

        #endregion //Properties
    }
}