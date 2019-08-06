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

    public class RestServiceGetEntitiesEventArgsWindows : RestServiceEventArgsWindows
    {
        #region Constructors

        public RestServiceGetEntitiesEventArgsWindows(
            string entityName,
            Nullable<Guid> userId,
            string userName,
            LinqEntityContextWindows entityContext,
            Type entityType,
            List<object> outputEntities)
            : base(entityName, userId, userName, entityContext, entityType)
        {
            _outputEntities = outputEntities;
        }

        #endregion //Constructors

        #region Fields

        private List<object> _outputEntities;

        #endregion //Fields

        #region Properties

        public List<object> OutputEntities
        {
            get { return _outputEntities; }
        }

        #endregion //Properties
    }
}
