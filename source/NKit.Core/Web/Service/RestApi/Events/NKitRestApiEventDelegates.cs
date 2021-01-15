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

    public delegate void OnBeforeGetEntityByIdHandler(object sender, NKitRestApiGetEntityByIdEventArgs e);
    public delegate void OnAfterGetEntityByIdHandler(object sender, NKitRestApiGetEntityByIdEventArgs e);

    public delegate void OnBeforeGetEntitiesHandler(object sender, NKitRestApiGetEntitiesEventArgs e);
    public delegate void OnAfterGetEntitiesHandler(object sender, NKitRestApiGetEntitiesEventArgs e);

    public delegate void OnBeforePutEntityHandler(object sender, NKitRestApiPutEntityEventArgs e);
    public delegate void OnAfterPutEntityHandler(object sender, NKitRestApiPutEntityEventArgs e);

    public delegate void OnBeforePostEntityHandler(object sender, NKitRestApiPostEntityEventArgs e);
    public delegate void OnAfterPostEntityHandler(object sender, NKitRestApiPostEntityEventArgs e);

    public delegate void OnBeforeDeleteEntityHandler(object sender, NKitRestApiDeleteEntityEventArgs e);
    public delegate void OnAfterDeleteEntityHandler(object sender, NKitRestApiDeleteEntityEventArgs e);

    public delegate void OnBeforeDeleteAllEntitiesHandler(object sender, NKitRestApiDeleteAllEntitiesEventArgs e);
    public delegate void OnAfterDeleteAllEntitiesHandler(object sender, NKitRestApiDeleteAllEntitiesEventArgs e);

    #endregion //Delegates
}
