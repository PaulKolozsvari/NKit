namespace NKit.Web.Service.WcfRest.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    #region Delegates

    public delegate void OnBeforeGetEntitiesHandlerWindows(object sender, RestServiceGetEntitiesEventArgsWindows e);
    public delegate void OnAfterGetEntitiesHandlerWindows(object sender, RestServiceGetEntitiesEventArgsWindows e);

    public delegate void OnBeforeGetEntityByIdHandlerWindows(object sender, RestServiceGetEntityByIdEventArgsWindows e);
    public delegate void OnAfterGetEntityByIdHandlerWindows(object sender, RestServiceGetEntityByIdEventArgsWindows e);

    public delegate void OnBeforeGetEntitiesByFieldHandlerWindows(object sender, RestServiceGetEntitiesByFieldEventArgsWindows e);
    public delegate void OnAfterGetEntitiesByFieldHandlerWindows(object sender, RestServiceGetEntitiesByFieldEventArgsWindows e);

    public delegate void OnBeforePutEntityHandlerWindows(object sender, RestServicePutEntityEventArgsWindows e);
    public delegate void OnAfterPutEntityHandlerWindows(object sender, RestServicePutEntityEventArgsWindows e);

    public delegate void OnBeforePostEntityHandlerWindows(object sender, RestServicePostEntityEventArgsWindows e);
    public delegate void OnAfterPostEntityHandlerWindows(object sender, RestServicePostEntityEventArgsWindows e);

    public delegate void OnBeforeDeleteEntityHandlerWindows(object sender, RestServiceDeleteEntityEventArgsWindows e);
    public delegate void OnAfterDeleteEntityHandlerWindows(object sender, RestServiceDeleteEntityEventArgsWindows e);

    #endregion //Delegates
}
