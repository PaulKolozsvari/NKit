﻿namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    #region Application Builder - Extension Methods

    public static class DbContextApplicationBuilderExtensions
    {
        #region Methods

        /// <summary>
        /// Updates database to the latest migration on the given DbContext.
        /// </summary>
        public static void UpdateNKitDatabase<D>(this IApplicationBuilder applicationBuilder) where D : DbContext
        {
            NKitDbRepositorySettings dbContextSettings = NKitDbRepositorySettings.GetSettings();
            if (dbContextSettings == null)
            {
                throw new Exception($"Cannot update NKitDatabase when {nameof(NKitDbRepositorySettings)} have not been specified in the {NKitInformation.GetAspNetCoreEnvironmentAppSettingsFileName()} file.");
            }
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
