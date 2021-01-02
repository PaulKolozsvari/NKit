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

    public class NKitRestApiEventArgsCore : EventArgs
    {
        #region Constructors

        public NKitRestApiEventArgsCore(
            string entityName,
            string userName,
            NKitDbContextRepository entityContext,
            Type entityType)
        {
            _entityName = entityName;
            _userName = userName;
            _entityContext = entityContext;
            _entityType = entityType;
        }

        #endregion //Constructors

        #region Fields

        private string _entityName;
        private string _userName;
        private NKitDbContextRepository _entityContext;
        private Type _entityType;

        #endregion //Fields

        #region Properties

        public string EntityName
        {
            get { return _entityName; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public NKitDbContextRepository EntityContext
        {
            get { return _entityContext; }
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        #endregion //Properties
    }
}
