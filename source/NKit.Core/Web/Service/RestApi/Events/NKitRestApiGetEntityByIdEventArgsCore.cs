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

    public class NKitRestApiGetEntityByIdEventArgsCore : NKitRestApiEventArgsCore
    {
        #region Constructors

        public NKitRestApiGetEntityByIdEventArgsCore(
            string entityName,
            string userName,
            NKitDbRepository entityContext,
            Type entityType,
            string entityId,
            object outputEntity)
            : base(entityName, userName, entityContext, entityType)
        {
            _entityId = entityId;
            _outputEntity = outputEntity;
        }

        #endregion //Constructors

        #region Fields

        private string _entityId;
        private object _outputEntity;

        #endregion //Fields

        #region Properties

        public string EntityId
        {
            get { return _entityId; }
        }

        public object OutputEntity
        {
            get { return _outputEntity; }
        }

        #endregion //Properties
    }
}
