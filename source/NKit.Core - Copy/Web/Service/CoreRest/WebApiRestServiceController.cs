namespace NKit.Web.Service.CoreRest
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Web.Service.CoreRest.Events;

    #endregion //Using Directives

    public class WebApiRestServiceController
    {
        #region Fields

        public event OnBeforeGetEntitiesHandlerWindows OnBeforeGetEntities;
        public event OnAfterGetEntitiesHandlerWindows OnAfterGetEntities;

        public event OnBeforeGetEntityByIdHandlerWindows OnBeforeGetEntityById;
        public event OnAfterGetEntityByIdHandlerWindows OnAfterGetEntityById;

        public event OnBeforeGetEntitiesByFieldHandlerWindows OnBeforeGetEntitiesByField;
        public event OnAfterGetEntitiesByFieldHandlerWindows OnAfterGetEntitiesByField;

        public event OnBeforePutEntityHandlerWindows OnBeforePut;
        public event OnAfterPutEntityHandlerWindows OnAfterPut;

        public event OnBeforePostEntityHandlerWindows OnBeforePost;
        public event OnAfterPostEntityHandlerWindows OnAfterPost;

        public event OnBeforeDeleteEntityHandlerWindows OnBeforeDelete;
        public event OnAfterDeleteEntityHandlerWindows OnAfterDelete;

        protected bool _auditServiceCalls;
        protected Nullable<Guid> _serviceInstanceId;

        #endregion //Fields
    }
}
