namespace NKit.Web.Service.RestApi.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NKit.Core.Data.DB.LINQ;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    public class NKitRestApiDeleteEntityEventArgs : NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiDeleteEntityEventArgs(
            string entityName,
            string userName,
            NKitDbContext dbContext,
            Type entityType,
            string entityId)
            : base(entityName, userName, dbContext, entityType)
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
