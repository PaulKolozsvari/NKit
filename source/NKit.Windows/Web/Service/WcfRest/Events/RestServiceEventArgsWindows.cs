namespace NKit.Web.Service.WcfRest.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    public class RestServiceEventArgsWindows : EventArgs
    {
        #region Constructors

        public RestServiceEventArgsWindows(
            string entityName,
            Nullable<Guid> userId,
            string userName,
            LinqEntityContextWindows entityContext,
            Type entityType)
        {
            _entityName = entityName;
            _userId = userId;
            _userName = userName;
            _entityContext = entityContext;
            _entityType = entityType;
        }

        #endregion //Constructors

        #region Fields

        private string _entityName;
        private Nullable<Guid> _userId;
        private string _userName;
        private LinqEntityContextWindows _entityContext;
        private Type _entityType;
        private bool _cancel;

        #endregion //Fields

        #region Properties

        public string EntityName
        {
            get { return _entityName; }
        }

        public Nullable<Guid> UserId
        {
            get { return _userId; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public LinqEntityContextWindows EntityContext
        {
            get { return _entityContext; }
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
