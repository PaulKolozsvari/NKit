namespace NKit.Web.Service.CoreRest.Events
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

    public class RestServiceDeleteEntityEventArgsCore : RestServiceEventArgsCore
    {
        #region Constructors

        public RestServiceDeleteEntityEventArgsCore(
            string entityName,
            Nullable<Guid> userId,
            string userName,
            LinqEntityContextCore entityContext,
            Type entityType,
            string entityId)
            : base(entityName, userId, userName, entityContext, entityType)
        {
            _entityId = entityId;
        }

        #endregion //Constructors

        #region Fields

        private string _entityId;

        #endregion //Fields

        #region Properties

        public string EntityId
        {
            get { return _entityId; }
        }

        #endregion //Properties
    }
}
