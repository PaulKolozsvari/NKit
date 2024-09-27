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

    public class NKitRestApiEventArgs : EventArgs
    {
        #region Constructors

        public NKitRestApiEventArgs(
            string entityName,
            string userName,
            NKitDbContext dbContext,
            Type entityType)
        {
            _entityName = entityName;
            _userName = userName;
            _dbContext = dbContext;
            _entityType = entityType;
        }

        #endregion //Constructors

        #region Fields

        private string _entityName;
        private string _userName;
        private NKitDbContext _dbContext;
        private Type _entityType;
        private bool _cancel;

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

        public NKitDbContext DbContext
        {
            get { return _dbContext; }
        }

        public Type EntityType
        {
            get { return _entityType; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        #endregion //Properties
    }
}
