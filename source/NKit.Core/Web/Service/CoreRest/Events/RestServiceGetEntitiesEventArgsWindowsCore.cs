﻿namespace NKit.Web.Service.CoreRest.Events
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

    public class RestServiceGetEntitiesEventArgsWindowsCore : RestServiceEventArgsCore
    {
        #region Constructors

        public RestServiceGetEntitiesEventArgsWindowsCore(
            string entityName,
            Nullable<Guid> userId,
            string userName,
            LinqEntityContextCore entityContext,
            Type entityType,
            List<object> outputEntities)
            : base(entityName, userId, userName, entityContext, entityType)
        {
            _outputEntities = outputEntities;
        }

        #endregion //Constructors

        #region Fields

        private List<object> _outputEntities;

        #endregion //Fields

        #region Properties

        public List<object> OutputEntities
        {
            get { return _outputEntities; }
        }

        #endregion //Properties
    }
}
