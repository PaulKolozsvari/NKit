namespace NKit.Web.Service.CoreRest.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    #endregion //Using Directives

    #region Delegates

    public delegate void OnBeforeGetEntitiesHandlerCore(object sender, RestServiceGetEntitiesEventArgsWindowsCore e);
    public delegate void OnAfterGetEntitiesHandlerCore(object sender, RestServiceGetEntitiesEventArgsWindowsCore e);

    public delegate void OnBeforeGetEntityByIdHandlerCore(object sender, RestServiceGetEntityByIdEventArgsWindowsCore e);
    public delegate void OnAfterGetEntityByIdHandlerCore(object sender, RestServiceGetEntityByIdEventArgsWindowsCore e);

    public delegate void OnBeforeGetEntitiesByFieldHandlerCore(object sender, RestServiceGetEntitiesByFieldEventArgsCore e);
    public delegate void OnAfterGetEntitiesByFieldHandlerCore(object sender, RestServiceGetEntitiesByFieldEventArgsCore e);

    public delegate void OnBeforePutEntityHandlerCore(object sender, RestServicePutEntityEventArgsWindowsCore e);
    public delegate void OnAfterPutEntityHandlerCore(object sender, RestServicePutEntityEventArgsWindowsCore e);

    public delegate void OnBeforePostEntityHandlerCore(object sender, RestServicePostEntityEventArgsWindowsCore e);
    public delegate void OnAfterPostEntityHandlerCore(object sender, RestServicePostEntityEventArgsWindowsCore e);

    public delegate void OnBeforeDeleteEntityHandlerCore(object sender, RestServiceDeleteEntityEventArgsCore e);
    public delegate void OnAfterDeleteEntityHandlerCore(object sender, RestServiceDeleteEntityEventArgsCore e);

    #endregion //Delegates
}
