namespace NKit.Web.Service.RestApi.Events
{
    #region Using Directives

    using NKit.Data.DB.LINQ;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    #endregion //Using Directives

    public class NKitRestApiPostEntityEventArgs : NKitRestApiEventArgs
    {
        #region Constructors

        public NKitRestApiPostEntityEventArgs(
            string entityName,
            string userName,
            NKitDbRepository entityContext,
            Type entityType,
            object inputEntity)
            : base(entityName, userName, entityContext, entityType)
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
