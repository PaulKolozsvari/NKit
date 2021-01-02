namespace NKit.Web.Service.RestApi.Events
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

    public delegate void OnBeforeGetEntityByIdHandlerCore(object sender, NKitRestApiGetEntityByIdEventArgsCore e);
    public delegate void OnAfterGetEntityByIdHandlerCore(object sender, NKitRestApiGetEntityByIdEventArgsCore e);

    public delegate void OnBeforeGetEntitiesHandlerCore(object sender, NKitRestApiGetEntitiesEventArgsCore e);
    public delegate void OnAfterGetEntitiesHandlerCore(object sender, NKitRestApiGetEntitiesEventArgsCore e);

    public delegate void OnBeforePutEntityHandlerCore(object sender, NKitRestApiPutEntityEventArgsCore e);
    public delegate void OnAfterPutEntityHandlerCore(object sender, NKitRestApiPutEntityEventArgsCore e);

    public delegate void OnBeforePostEntityHandlerCore(object sender, NKitRestApiPostEntityEventArgsCore e);
    public delegate void OnAfterPostEntityHandlerCore(object sender, NKitRestApiPostEntityEventArgsCore e);

    public delegate void OnBeforeDeleteEntityHandlerCore(object sender, NKitRestApiDeleteEntityEventArgsCore e);
    public delegate void OnAfterDeleteEntityHandlerCore(object sender, NKitRestApiDeleteEntityEventArgsCore e);

    #endregion //Delegates
}
