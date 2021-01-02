namespace NKit.Core.Utilities.Services
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Core.Data.DB.LINQ;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities.SettingsFile;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Web.Service.RestApi.ContentMapping;
    using NKit.Web.Service.RestApi.Middleware;

    #endregion //Using Directives

    public class NKitApplication
    {
        #region Methods

        public static void RegisterNKitServices<D, R>(IConfiguration configuration, IServiceCollection services) where D : NKitDbContext where R : NKitDbContextRepository
        {
            NKitSettings.RegisterConfigurations(configuration, services, true, true, true, true, true);
            NKitDatabaseSettings databaseSettings = NKitDatabaseSettings.GetSettings(configuration);
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDbContext<D>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(databaseSettings.DatabaseConnectionString, sqlServerOptionsBuilder => sqlServerOptionsBuilder.CommandTimeout(databaseSettings.DatabaseCommandTimeout)), ServiceLifetime.Transient);
            services.AddTransient<R>();
            services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, new NKitWebApiControllerInputFormatter()));
        }

        public static void RegisterNKitMiddleware<R>(IApplicationBuilder app) where R : NKitDbContextRepository
        {
            app.UseHttpStatusCodeExceptionMiddleware<R>();
        }

        #endregion //Methods
    }
}
