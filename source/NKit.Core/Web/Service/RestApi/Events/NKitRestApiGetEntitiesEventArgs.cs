﻿namespace NKit.Web.Service.RestApi.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    public class NKitRestApiGetEntitiesEventArgs: NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiGetEntitiesEventArgs(
            string entityName,
            string userName,
            NKitDbContext dbContext,
            Type entityType,
            string fieldName,
            string fieldValue,
            List<object> outputEntities)
            : base(entityName, userName, dbContext, entityType)
        {
            _fieldName = fieldName;
            _fieldValue = fieldValue;
            _outputEntities = outputEntities;
        }

        #endregion //Constructors

        #region Fields

        private string _fieldName;
        private string _fieldValue;
        private List<object> _outputEntities;

        #endregion //Fields

        #region Properties

        public string FieldName
        {
            get { return _fieldName; }
        }

        public string FieldValue
        {
            get { return _fieldValue; }
        }

        public List<object> OutputEntities
        {
            get { return _outputEntities; }
        }

        #endregion //Properties
    }
}
