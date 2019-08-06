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

    public class BeforeWebInvokeSqlArgsWindows : BeforeWebServiceRequestArgsWindows
    {
        #region Constructors

        public BeforeWebInvokeSqlArgsWindows(
            string uri,
            HttpVerb method,
            string contentType,
            string accept,
            string userAgent,
            string inputString,
            ISerializer inputSerializer,
            ISerializer outputSerializer,
            string typeName,
            string sqlQueryString)
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
            _sqlQueryString = sqlQueryString;
        }

        #endregion //Constructors

        #region Fields

        private string _typeName;
        private string _sqlQueryString;

        #endregion //Fields

        #region Properties

        public string TypeName
        {
            get { return _typeName; }
        }

        public string SqlQueryString
        {
            get { return _sqlQueryString; }
        }

        #endregion //Properties
    }
}
