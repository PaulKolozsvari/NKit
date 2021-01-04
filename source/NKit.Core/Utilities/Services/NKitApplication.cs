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
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Web.Service.RestApi.ContentMapping;
    using NKit.Web.Service.RestApi.Middleware;

    #endregion //Using Directives

    public class NKitApplication
    {
        #region Methods

        public static void RegisterNKitServices<D, R>(
            IConfiguration configuration, 
            IServiceCollection services,
            bool writeNKitSettingsToConsole) where D : NKitDbContext where R : NKitDbContextRepository
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(NKitApplication));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(NKitApplication));

            NKitSettings.RegisterConfigurations(configuration, services, true, true, true, true, true);

            NKitWebApiSettings webApiSettings = NKitWebApiSettings.GetSettings(configuration);
            NKitDatabaseSettings databaseSettings = NKitDatabaseSettings.GetSettings(configuration);
            NKitLoggingSettings loggingSettings = NKitLoggingSettings.GetSettings(configuration);
            NKitEmailSettings emailSettings = NKitEmailSettings.GetSettings(configuration);
            NKitWebApiClientSettings webApiClientSettings = NKitWebApiClientSettings.GetSettings(configuration);

            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddDbContext<D>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(databaseSettings.DatabaseConnectionString, sqlServerOptionsBuilder => sqlServerOptionsBuilder.CommandTimeout(databaseSettings.DatabaseCommandTimeout)), ServiceLifetime.Transient);
            services.AddDbContext<D>(ServiceLifetime.Transient);
            services.AddTransient<R>();
            services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, new NKitWebApiControllerInputFormatter()));

            if (writeNKitSettingsToConsole)
            {
                Console.WriteLine($"*** {nameof(NKitWebApiSettings)} ***");
                Console.WriteLine(GOC.Instance.JsonSerializer.SerializeToText(webApiSettings));
                Console.WriteLine();

                Console.WriteLine($"*** {nameof(NKitDatabaseSettings)} ***");
                Console.WriteLine(GOC.Instance.JsonSerializer.SerializeToText(databaseSettings));
                Console.WriteLine();

                Console.WriteLine($"*** {nameof(NKitLoggingSettings)} ***");
                Console.WriteLine(GOC.Instance.JsonSerializer.SerializeToText(loggingSettings));
                Console.WriteLine();

                Console.WriteLine($"*** {nameof(NKitEmailSettings)} ***");
                Console.WriteLine(GOC.Instance.JsonSerializer.SerializeToText(emailSettings));
                Console.WriteLine();

                Console.WriteLine($"*** {nameof(NKitWebApiClientSettings)} ***");
                Console.WriteLine(GOC.Instance.JsonSerializer.SerializeToText(webApiClientSettings));
                Console.WriteLine();
            }
        }

        public static void RegisterNKitMiddleware<R>(IApplicationBuilder app) where R : NKitDbContextRepository
        {
            app.UseHttpStatusCodeExceptionMiddleware<R>();
        }

        #endregion //Methods
    }
}
