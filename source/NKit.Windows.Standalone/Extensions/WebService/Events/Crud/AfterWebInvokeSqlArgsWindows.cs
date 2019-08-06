namespace NKit.Extensions.WebService.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Utilities.Serialization;
    using NKit.Web.Client;

    #endregion //Using Directives

    public class AfterWebInvokeSqlArgsWindows : AfterWebServiceRequestArgsWindows
    {
        #region Constructors

        public AfterWebInvokeSqlArgsWindows(
            string uri,
            HttpVerb method,
            string contentType,
            string accept,
            string userAgent,
            string inputString,
            ISerializer inputSerializer,
            ISerializer outputSerializer,
            string typeName,
            string responseText)
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
            _typeName = typeName;
        }

        #endregion //Constructors

        #region Fields

        private string _typeName;
        private string _responseText;

        #endregion //Fields

        #region Properties

        public string TypeName
        {
            get { return _typeName; }
        }

        public string ResponseText
        {
            get { return _responseText; }
        }

        #endregion //Properties
    }
}