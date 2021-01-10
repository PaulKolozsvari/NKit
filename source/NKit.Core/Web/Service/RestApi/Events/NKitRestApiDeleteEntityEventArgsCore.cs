namespace NKit.Web.Service.RestApi.Events
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

    public class NKitRestApiDeleteEntityEventArgsCore : NKitRestApiEventArgsCore
    {
        #region Constructors

        public NKitRestApiDeleteEntityEventArgsCore(
            string entityName,
            string userName,
            NKitDbRepository entityContext,
            Type entityType,
            string entityId)
            : base(entityName, userName, entityContext, entityType)
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
