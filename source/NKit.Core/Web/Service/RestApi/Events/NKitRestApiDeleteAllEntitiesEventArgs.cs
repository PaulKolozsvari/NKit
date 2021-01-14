namespace NKit.Web.Service.RestApi.Events
{
    #region Using Directives

    using NKit.Data.DB.LINQ;
    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class NKitRestApiDeleteAllEntitiesEventArgs : NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiDeleteAllEntitiesEventArgs(
            string entityName, 
            string userName, 
            NKitDbRepository entityContext, 
            Type entityType) : 
            base(entityName, userName, entityContext, entityType)
        {
        }

        #endregion //Constructors
    }
}
