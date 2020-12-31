namespace NKit.Web.Service.CoreRest.Middleware
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    #region Application Builder - Extension Methods

    /// <summary>
    /// Extension method used to add the middleware to the HTTP request pipeline.
    /// </summary>
    public static class ApplicationBuilderMiddlewareExtensionsCore
    {
        public static IApplicationBuilder UseHttpStatusCodeExceptionMiddleware<D>(this IApplicationBuilder applicationBuilder) where D : DbContextCrudTransactionsRepositoryCore
        {
            return applicationBuilder.UseMiddleware<HttpStatusCodeExceptionMiddlewareCore<D>>();
        }
    }

    #endregion //Application Builder - Extension Methods
}
