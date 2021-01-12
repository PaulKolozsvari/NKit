namespace NKit.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using NKit.Data.DB.LINQ;
    using NKit.Web.Service.RestApi.Middleware;

    #endregion //Using Directives

    #region Methods

    /// <summary>
    /// Extension method used to add the middleware to the HTTP request pipeline.
    /// </summary>
    public static class ApplicationBuilderMiddlewareExtensions
    {
        #region Methods

        /// <summary>
        /// Registers the NKitHttpExceptionHandlerMiddleware
        /// </summary>
        /// <typeparam name="D">The NKitDbContextRepository type being used in the application.</typeparam>
        public static IApplicationBuilder UseNKitHttpExceptionHandlerMiddleware<D>(this IApplicationBuilder applicationBuilder) where D : NKitDbRepository
        {
            return applicationBuilder.UseMiddleware<NKitHttpExceptionHandlerMiddleware<D>>();
        }

        #endregion //Methods
    }

    #endregion //Methods
}