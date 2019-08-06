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

    public class RestServiceDeleteEntityEventArgsWindows : RestServiceEventArgsWindows
    {
        #region Constructors

        public RestServiceDeleteEntityEventArgsWindows(
            string entityName,
            Nullable<Guid> userId,
            string userName,
            LinqEntityContextWindows entityContext,
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
