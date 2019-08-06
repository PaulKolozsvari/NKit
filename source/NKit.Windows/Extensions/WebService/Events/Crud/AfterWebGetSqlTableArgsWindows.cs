namespace NKit.Extensions.WebService.Events.Crud
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

    public class AfterWebGetSqlTableArgsWindows : AfterWebServiceRequestArgsWindows
    {
        #region Constructors

        public AfterWebGetSqlTableArgsWindows(
            string uri,
            HttpVerb method,
            string contentType,
            string accept,
            string userAgent,
            ISerializer inputSerializer,
            ISerializer outputSerializer,
            string tableName,
            string filters,
            Type outputEntityType,
            List<object> outputEntities)
            : base(
            uri,
            method,
            contentType,
            accept,
            userAgent,
            null,
            inputSerializer,
            outputSerializer)
        {
            _tableName = tableName;
            _filters = filters;
            _outputEntityType = outputEntityType;
            _outputEntities = outputEntities;
        }

        #endregion //Constructors

        #region Fields

        protected string _tableName;
        protected string _filters;
        protected Type _outputEntityType;
        protected List<object> _outputEntities;

        #endregion //Fields

        #region Properties

        public string TableName
        {
            get { return _tableName; }
        }

        public string Filters
        {
            get { return _filters; }
        }

        public Type OutputEntityType
        {
            get { return _outputEntityType; }
        }

        public List<object> OutputEntities
        {
            get { return _outputEntities; }
        }

        #endregion //Properties
    }
}