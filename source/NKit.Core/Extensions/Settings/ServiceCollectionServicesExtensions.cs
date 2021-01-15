namespace NKit.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NKit.Core.Data.DB.LINQ;
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Settings.Default;
    using NKit.Utilities.Email;
    using NKit.Utilities.Logging;
    using NKit.Web.Service.RestApi.ContentMapping;

    #endregion //Using Directives

    public static class ServiceCollectionServicesExtensions
    {
        #region Methods

        #region Register Services Methods

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// The Entity Framework SQL Server Provider is not registered as part of this call.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        public static void RegisterDefaultNKitServices<D>(
            this IServiceCollection services,
            IConfiguration configuration) where D : NKitDbContext
        {
            RegisterDefaultNKitServices<D>(services, configuration, true,  true, true);
        }

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <param name="registerNKitDbContext">Whether or not to register the NKitDbContext specified by D.</param>
        /// <param name="registerDefaultNKitEmailClient">Whether or not to register the default NKitEmailClient.</param>
        /// <param name="registerControllerInputFormatter">Whether or not to register the controller formatters i.e. allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.</param>
        public static void RegisterDefaultNKitServices<D>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerNKitDbContext,
            bool registerDefaultNKitEmailClient,
            bool registerControllerInputFormatter) where D : NKitDbContext
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(ServiceCollectionSettingsExtensions));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(ServiceCollectionSettingsExtensions));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            if (registerNKitDbContext)
            {
                RegisterNKitDbContext<D>(services, configuration);
            }
            if (registerDefaultNKitEmailClient)
            {
                RegisterDefaultNKitEmailClient(services, configuration);
            }
            if (registerControllerInputFormatter)
            {
                services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, new NKitWebApiControllerInputFormatter()));
            }
        }

        /// <summary>
        /// Registers the NKitDbDbContext specified by D and the NKitDbContextRepository specified by R as transient services, 
        /// which make them accessible through the Dependency Injection container and available to all NKit controllers just as the NKitWebApiController and/or NKitWebApiControllerCrud.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        public static void RegisterNKitDbContext<D>(
            this IServiceCollection services,
            IConfiguration configuration) where D : NKitDbContext
        {
            NKitDbContextSettings dbContextSettings = NKitDbContextSettings.GetSettings(configuration);
            if (dbContextSettings != null)
            {
                services.AddDbContext<D>(ServiceLifetime.Transient); //Register the NKitDbContext.
            }
        }

        /// <summary>
        /// Registers the default NKitEmailClient as a transient service based on the settings configured in the NKitEmailCllientSettings section from the appsettings.json file.
        /// This makes it accessible through the Dependency Injection container and available to all NKit controllers just as the NKitWebApiController and/or NKitWebApiControllerCrud.
        /// NKitEmailClientSettings must have been pre registered before calling this method as the NKitEmailClient will be retrieved.
        /// </summary>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        public static void RegisterDefaultNKitEmailClient(this IServiceCollection services, IConfiguration configuration)
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(ServiceCollectionSettingsExtensions));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(ServiceCollectionSettingsExtensions));
            NKitEmailClientServiceSettings emailSettings = NKitEmailClientServiceSettings.GetSettings(configuration);
            if (emailSettings == null)
            {
                throw new NullReferenceException($"{nameof(NKitEmailClientServiceSettings)} not registered. Must configure {nameof(NKitEmailClientServiceSettings)} in the appsettings.json file and registered before calling {nameof(RegisterNKitEmailClient)}.");
            }
            services.AddTransient<NKitEmailClientService>();
        }

        /// <summary>
        /// Registers a custom NKitEmailClient as a transient service based on the settings configured in the NKitEmailCllientSettings section from the appsettings.json file.
        /// This makes it accessible through the Dependency Injection container and available to all NKit controllers just as the NKitWebApiController and/or NKitWebApiControllerCrud.
        /// NKitEmailClientSettings must have been pre registered before calling this method as the NKitEmailClient will be retrieved.
        /// </summary>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        public static void RegisterNKitEmailClient<E>(this IServiceCollection services, IConfiguration configuration) where E : NKitEmailClientService
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(ServiceCollectionSettingsExtensions));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(ServiceCollectionSettingsExtensions));
            NKitEmailClientServiceSettings emailSettings = NKitEmailClientServiceSettings.GetSettings(configuration);
            if (emailSettings == null)
            {
                throw new NullReferenceException($"{nameof(NKitEmailClientServiceSettings)} not registered. Must configure {nameof(NKitEmailClientServiceSettings)} in the appsettings.json file and registered before calling {nameof(RegisterNKitEmailClient)}.");
            }
            services.AddTransient<E>();
        }

        /// <summary>
        /// Registers the default NKitWebApiControllerInputFormatter allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.
        /// </summary>
        /// <param name="services">The DI services container received in the Startup class.</param>
        public static void RegisterNKitwebApiControllerInputFormatter(this IServiceCollection services)
        {
            services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, new NKitWebApiControllerInputFormatter()));
        }

        /// <summary>
        /// Registers a custom NKitWebApiControllerInputFormatter allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.
        /// </summary>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="webApiControllerInputFormatter">The custom NKitWebApiControllerInputFormatter</param>
        public static void RegisterNKitwebApiControllerInputFormatter(this IServiceCollection services, NKitWebApiControllerInputFormatter webApiControllerInputFormatter)
        {
            services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, webApiControllerInputFormatter));
        }

        #endregion //Register Services Methods

        #endregion //Methods
    }
}
