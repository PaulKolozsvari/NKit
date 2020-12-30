namespace NKit.Web.Service.CoreRest
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Web.Service.CoreRest.Events;

    #endregion //Using Directives

    public class WebApiRestServiceControllerCore
    {
        #region Fields

        public event OnBeforeGetEntitiesHandlerCore OnBeforeGetEntities;
        public event OnAfterGetEntitiesHandlerCore OnAfterGetEntities;

        public event OnBeforeGetEntityByIdHandlerCore OnBeforeGetEntityById;
        public event OnAfterGetEntityByIdHandlerCore OnAfterGetEntityById;

        public event OnBeforeGetEntitiesByFieldHandlerCore OnBeforeGetEntitiesByField;
        public event OnAfterGetEntitiesByFieldHandlerCore OnAfterGetEntitiesByField;

        public event OnBeforePutEntityHandlerCore OnBeforePut;
        public event OnAfterPutEntityHandlerCore OnAfterPut;

        public event OnBeforePostEntityHandlerCore OnBeforePost;
        public event OnAfterPostEntityHandlerCore OnAfterPost;

        public event OnBeforeDeleteEntityHandlerCore OnBeforeDelete;
        public event OnAfterDeleteEntityHandlerCore OnAfterDelete;

        protected bool _auditServiceCalls;
        protected Nullable<Guid> _serviceInstanceId;

        #endregion //Fields
    }
}
