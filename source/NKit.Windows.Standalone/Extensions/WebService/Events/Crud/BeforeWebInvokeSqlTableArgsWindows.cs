﻿namespace NKit.Extensions.WebService.Events.Crud
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

    public class BeforeWebInvokeSqlTableArgsWindows : BeforeWebServiceRequestArgsWindows
    {
        #region Constructors

        public BeforeWebInvokeSqlTableArgsWindows(
            string uri,
            HttpVerb method,
            string contentType,
            string accept,
            string userAgent,
            string inputString,
            ISerializer inputSerializer,
            ISerializer outputSerializer,
            string tableName,
            List<object> inputEntities,
            Type inputEntityType)
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
            _tableName = tableName;
            _inputEntities = inputEntities;
            _inputEntityType = inputEntityType;
        }


        #endregion //Constructors

        #region Fields

        protected string _tableName;
        protected List<object> _inputEntities;
        protected Type _inputEntityType;

        #endregion //Fields

        #region Properties

        public string TableName
        {
            get { return _tableName; }
        }

        public List<object> InputEntities
        {
            get { return _inputEntities; }
        }

        public Type InputEntityType
        {
            get { return _inputEntityType; }
        }

        #endregion //Properties
    }
}