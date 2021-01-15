namespace NKit.Web.Service.RestApi.Events
{
    #region Using Directives

    using NKit.Data.DB.LINQ;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Core.Data.DB.LINQ;

    #endregion //Using Directives

    public class NKitRestApiDeleteAllEntitiesEventArgs : NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiDeleteAllEntitiesEventArgs(
            string entityName, 
            string userName,
            NKitDbContext dbContext, 
            Type entityType) : 
            base(entityName, userName, dbContext, entityType)
        {
        }

        #endregion //Constructors
    }
}
