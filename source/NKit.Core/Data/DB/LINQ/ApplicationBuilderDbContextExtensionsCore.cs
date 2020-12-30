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

    public static class ApplicationBuilderDbContextExtensionsCore
    {
        #region Methods

        /// <summary>
        /// Updates database to the latest migration on the given DbContext.
        /// </summary>
        /// <param name="applicationBuilder"></param>
        /// <returns></returns>
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
    }
}
