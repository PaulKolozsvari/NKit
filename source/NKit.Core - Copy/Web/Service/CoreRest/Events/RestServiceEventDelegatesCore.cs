namespace NKit.Web.Service.CoreRest.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    #region Delegates

    public delegate void OnBeforeGetEntitiesHandlerWindows(object sender, RestServiceGetEntitiesEventArgsWindowsCore e);
    public delegate void OnAfterGetEntitiesHandlerWindows(object sender, RestServiceGetEntitiesEventArgsWindowsCore e);

    public delegate void OnBeforeGetEntityByIdHandlerWindows(object sender, RestServiceGetEntityByIdEventArgsWindowsCore e);
    public delegate void OnAfterGetEntityByIdHandlerWindows(object sender, RestServiceGetEntityByIdEventArgsWindowsCore e);

    public delegate void OnBeforeGetEntitiesByFieldHandlerWindows(object sender, RestServiceGetEntitiesByFieldEventArgsCore e);
    public delegate void OnAfterGetEntitiesByFieldHandlerWindows(object sender, RestServiceGetEntitiesByFieldEventArgsCore e);

    public delegate void OnBeforePutEntityHandlerWindows(object sender, RestServicePutEntityEventArgsWindowsCore e);
    public delegate void OnAfterPutEntityHandlerWindows(object sender, RestServicePutEntityEventArgsWindowsCore e);

    public delegate void OnBeforePostEntityHandlerWindows(object sender, RestServicePostEntityEventArgsWindowsCore e);
    public delegate void OnAfterPostEntityHandlerWindows(object sender, RestServicePostEntityEventArgsWindowsCore e);

    public delegate void OnBeforeDeleteEntityHandlerWindows(object sender, RestServiceDeleteEntityEventArgsCore e);
    public delegate void OnAfterDeleteEntityHandlerWindows(object sender, RestServiceDeleteEntityEventArgsCore e);

    #endregion //Delegates
}
