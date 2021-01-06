namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    #endregion //Using Directives

    #region Application Builder - Extension Methods

    public static class ApplicationBuilderDbContextExtensions
    {
        #region Methods

        /// <summary>
        /// Updates database to the latest migration on the given DbContext.
        /// </summary>
        public static void UpdateDatabase<D>(this IApplicationBuilder applicationBuilder) where D : DbContext
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<D>())
                {
                    context.Database.Migrate();
                }
            }
        }

        #endregion //Methods

        #endregion Application Builder - Extension Methods
    }
}
