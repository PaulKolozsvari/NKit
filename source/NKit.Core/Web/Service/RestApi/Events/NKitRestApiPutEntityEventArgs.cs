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

    public class NKitRestApiPutEntityEventArgs : NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiPutEntityEventArgs(
            string entityName,
            string userName,
            NKitDbContext dbContext,
            Type entityType,
            object inputEntity)
            : base(entityName, userName, dbContext, entityType)
        {
            _inputEntity = inputEntity;
        }

        #endregion //Constructors

        #region Fields

        private object _inputEntity;

        #endregion //Fields

        #region Properties

        public object InputEntity
        {
            get { return _inputEntity; }
        }

        #endregion //Properties
    }
}
