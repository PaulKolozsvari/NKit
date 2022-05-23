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
            RegisterDefaultNKitServices<D>(services, configuration, registerNKitDbContext: true, registerDefaultNKitEmailClient:  true, registerNKitLoggingManager: true, registerControllerInputFormatter: true);
        }

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// The Entity Framework SQL Server Provider is not registered as part of this call.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <typeparam name="E">The NKitEmailClientService to be used to send emails.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        public static void RegisterDefaultNKitServices<D, E>(
            this IServiceCollection services,
            IConfiguration configuration) where D : NKitDbContext where E : NKitEmailClientService
        {
            RegisterDefaultNKitServices<D, E>(services, configuration, registerNKitDbContext: true, registerDefaultNKitEmailClient: true, registerNKitLoggingManager: true, registerControllerInputFormatter: true);
        }

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <param name="registerNKitDbContext">Whether or not to register the NKitDbContext specified by D.</param>
        /// <param name="registerDefaultNKitEmailClient">Whether or not to register the default NKitEmailClient.</param>
        /// <param name="registerNKitLoggingManager">Whether or not to register the default NKitLoggingManager.</param>
        /// <param name="registerControllerInputFormatter">Whether or not to register the controller formatters i.e. allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.</param>
        public static void RegisterDefaultNKitServices<D>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerNKitDbContext,
            bool registerDefaultNKitEmailClient,
            bool registerNKitLoggingManager,
            bool registerControllerInputFormatter) where D : NKitDbContext
        {
            RegisterDefaultNKitServices<D, NKitEmailClientService>(services, configuration, registerNKitDbContext, registerDefaultNKitEmailClient, registerNKitLoggingManager, registerControllerInputFormatter);
        }

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <typeparam name="E">The NKitEmailClientService to be used to send emails.</typeparam>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <param name="registerNKitDbContext">Whether or not to register the NKitDbContext specified by D.</param>
        /// <param name="registerDefaultNKitEmailClient">Whether or not to register the default NKitEmailClient.</param>
        /// <param name="registerNKitLoggingManager">Whether or not to register the default NKitLoggingManager.</param>
        /// <param name="registerControllerInputFormatter">Whether or not to register the controller formatters i.e. allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.</param>
        public static void RegisterDefaultNKitServices<D, E>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerNKitDbContext,
            bool registerDefaultNKitEmailClient,
            bool registerNKitLoggingManager,
            bool registerControllerInputFormatter) where D : NKitDbContext where E : NKitEmailClientService
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
                RegisterNKitEmailClient<E>(services, configuration);
                if(typeof(E) != typeof(NKitEmailClientService))
                {
                    RegisterDefaultNKitEmailClient(services, configuration); //The NKitHttpExceptionHandlerMiddleware and other NKit components that make use of the NKitEmailClientService will not be able to resolve the custom (inherited) E type register in the above line. Therefore we need to register the base NKitEmailClient Service too.
                }
            }
            if (registerNKitLoggingManager)
            {
                RegisterNKitLoggingManager<D, E>(services, configuration);
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
            RegisterNKitEmailClient<NKitEmailClientService>(services, configuration);
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

        public static void RegisterNKitLoggingManager<D, E>(this IServiceCollection services, IConfiguration configuration) where D : NKitDbContext where E : NKitEmailClientService
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(ServiceCollectionSettingsExtensions));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(ServiceCollectionSettingsExtensions));
            NKitLoggingSettings loggingSettings = NKitLoggingSettings.GetSettings(configuration);
            if (loggingSettings == null)
            {
                throw new NullReferenceException($"{nameof(NKitLoggingSettings)} not registered. Must configure {nameof(NKitLoggingSettings)} in the appsettings.json file and registered before calling {nameof(RegisterNKitLoggingManager)}.");
            }
            services.AddTransient<NKitLoggingManager<D, E>>();
        }

        /// <summary>
        /// Registers the default NKitWebApiControllerInputFormatter allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.
        /// </summary>
        /// <param name="services">The DI services container received in the Startup class.</param>
        public static void RegisterNKitEebApiControllerInputFormatter(this IServiceCollection services)
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
