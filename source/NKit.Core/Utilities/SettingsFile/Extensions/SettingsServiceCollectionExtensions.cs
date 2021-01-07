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
    using Microsoft.Extensions.Logging.EventLog;
    using NKit.Core.Data.DB.LINQ;
    using NKit.Core.Utilities.Email;
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Web.Service.RestApi.ContentMapping;

    #endregion //Using Directives

    public static class SettingsServiceCollectionExtensions
    {
        #region Methods

        #region Utility Methods

        /// <summary>
        /// Creates a LoggerFactory to use to create an ILogger. This method can be used to create an ILogger inside a .NET Core Startup.ConfigureServices method
        /// (where the logger has not been created since the DI container has not been created) thus allowing you to log activity that occurs inside the Startup.ConfigureServices method.
        /// </summary>
        /// <typeparam name="T">The type acting as the ILogger category. When called from the Start class, the category can be the Startup class.</typeparam>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <returns></returns>
        public static ILogger CreateLogger(string categoryName, NKitLoggingSettings loggingSettings)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddDebug();
                loggingBuilder.AddEventSourceLogger();
                if (loggingSettings.LogToConsole)
                {
                    loggingBuilder.AddConsole();
                }
                if (loggingSettings.LogToWindowsEventLog)
                {
                    loggingBuilder.AddEventLog(new EventLogSettings()
                    {
                        LogName = loggingSettings.EventLogName,
                        SourceName = loggingSettings.EventSourceName,
                    });
                }
            });
            ILogger result = loggerFactory.CreateLogger(categoryName);
            return result;
        }

        #endregion //Utility Methods

        #region Register Settings Methods

        /// <summary>
        /// Register default Configurations from the appsettings.json which will be made available as IOptions to all services in the DI service container.
        /// </summary>
        /// <param name="services">The IServiceCollection passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="configuration">The IConfiguration passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="generalSettings">Returns the General settings that have been registered.</param>
        /// <param name="webApiSettings">Returns the Web API settings that have been registered.</param>
        /// <param name="webApiClientSettings">Returns the Web API Client settings that have been registered.</param>
        /// <param name="databaseSettings">Returns the Database settings that have been registered.</param>
        /// <param name="loggingSettings">Returns the Logging settings that have been registered.</param>
        /// <param name="emailSettings">Returns the Email settings that have been registered.</param>
        /// <param name="loggerCategoryName">Optional category name of the ILogger that will be created to log the settings being registered.</param>
        public static void RegisterDefaultNKitSettings(
            this IServiceCollection services,
            IConfiguration configuration,
            out NKitGeneralSettings generalSettings,
            out NKitWebApiSettings webApiSettings,
            out NKitWebApiClientSettings webApiClientSettings,
            out NKitDatabaseSettings databaseSettings,
            out NKitLoggingSettings loggingSettings,
            out NKitEmailSettings emailSettings,
            string loggerCategoryName)
        {
            ILogger logger = !string.IsNullOrEmpty(loggerCategoryName) ? CreateLogger(loggerCategoryName, NKitLoggingSettings.GetSettings(configuration)) : null;
            RegisterDefaultNKitSettings(services, configuration, 
                out generalSettings,
                out webApiSettings,
                out webApiClientSettings,
                out databaseSettings,
                out loggingSettings,
                out emailSettings,
                logger);
        }

        /// <summary>
        /// Register default Configurations from the appsettings.json which will be made available as IOptions to all services in the DI service container.
        /// </summary>
        /// <param name="services">The IServiceCollection passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="configuration">The IConfiguration passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="generalSettings">Returns the General settings that have been registered.</param>
        /// <param name="webApiSettings">Returns the Web API settings that have been registered.</param>
        /// <param name="webApiClientSettings">Returns the Web API Client settings that have been registered.</param>
        /// <param name="databaseSettings">Returns the Database settings that have been registered.</param>
        /// <param name="loggingSettings">Returns the Logging settings that have been registered.</param>
        /// <param name="emailSettings">Returns the Email settings that have been registered.</param>
        /// <param name="logger">Optional ILogger which if specified is used to log all the NKit settings read from the appsettings.json file.</param>
        public static void RegisterDefaultNKitSettings(
            this IServiceCollection services,
            IConfiguration configuration,
            out NKitGeneralSettings generalSettings,
            out NKitWebApiSettings webApiSettings,
            out NKitWebApiClientSettings webApiClientSettings,
            out NKitDatabaseSettings databaseSettings,
            out NKitLoggingSettings loggingSettings,
            out NKitEmailSettings emailSettings,
            ILogger logger)
        {
            generalSettings = NKitGeneralSettings.RegisterConfiguration(configuration, services);
            webApiSettings = NKitWebApiSettings.RegisterConfiguration(configuration, services);
            webApiClientSettings = NKitWebApiClientSettings.RegisterConfiguration(configuration, services);
            databaseSettings = NKitDatabaseSettings.RegisterConfiguration(configuration, services);
            loggingSettings = NKitLoggingSettings.RegisterConfiguration(configuration, services);
            emailSettings = NKitEmailSettings.RegisterConfiguration(configuration, services);
            if (logger == null)
            {
                return;
            }
            StringBuilder logMessage = new StringBuilder();
            logMessage.AppendLine($"*** Default NKit Settings registered from : {NKitInformation.GetAspNetCoreEnvironmentAppSettingsFileName()} ***");
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitGeneralSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(generalSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitWebApiSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(webApiSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitDatabaseSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(databaseSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitLoggingSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(loggingSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitEmailSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(emailSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitWebApiClientSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(webApiClientSettings));
            logMessage.AppendLine();

            logger.LogInformation(logMessage.ToString());
        }

        /// <summary>
        /// Registers the specified NKitSettings type from the appsettings.json which will be made available as IOptions to all services in the DI service container.
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void RegisterNKitSettings<S>(this IServiceCollection services, IConfiguration configuration) where S : NKitSettings
        {
            NKitSettings.RegisterConfiguration<S>(configuration, services);
        }

        #endregion //Register Settings Methods

        #region Register Services Methods

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// A new Ilogger gets created to log the settings if the loggingCategoryName is specified.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <typeparam name="R">The NKitDbContextRepository used to mnage the NKitDbContext.</typeparam>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="registerEmailService">Whether or not to register NKitEmailService to enable sending emails and/or sending emails from the NKitExceptionHandlerMiddleware</param>
        /// <param name="registerEntityFrameworkSqlServerProvider">Whether or not to register Sql Server as the provider for the Entity Framework NKitDbContext i.e. sometimes it may be preferable to configure your own provider on the DbContext's OnConfiguring method, hence not requiring this Sql Server provider registration here.</param>
        /// <param name="registerControllerInputFormatter">Whether or not to register the controller formatters i.e. allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.</param>
        /// <param name="loggerCategoryName">Optional category name of the ILogger that will be created to log the settings being registered.</param>
        public static void RegisterDefaultNKitServices<D, R>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerEmailService,
            bool registerEntityFrameworkSqlServerProvider,
            bool registerControllerInputFormatter,
            string loggerCategoryName) where D : NKitDbContext where R : NKitDbContextRepository
        {
            ILogger logger = !string.IsNullOrEmpty(loggerCategoryName) ? CreateLogger(loggerCategoryName, NKitLoggingSettings.GetSettings(configuration)) : null;
            RegisterDefaultNKitServices<D, R>(services, configuration, registerEmailService, registerEntityFrameworkSqlServerProvider, registerControllerInputFormatter, logger);
        }

        /// <summary>
        /// Reads all the NKit settings from the appsettings.json file and registers all the services required by NKit.
        /// </summary>
        /// <typeparam name="D">The Entity Framework NKitDbContext to be used to manage the database.</typeparam>
        /// <typeparam name="R">The NKitDbContextRepository used to mnage the NKitDbContext.</typeparam>
        /// <param name="configuration">The IConfiguration received in the Startup class.</param>
        /// <param name="services">The DI services container received in the Startup class.</param>
        /// <param name="registerEmailService">Whether or not to register NKitEmailService to enable sending emails and/or sending emails from the NKitExceptionHandlerMiddleware</param>
        /// <param name="registerEntityFrameworkSqlServerProvider">Whether or not to register Sql Server as the provider for the Entity Framework NKitDbContext i.e. sometimes it may be preferable to configure your own provider on the DbContext's OnConfiguring method, hence not requiring this Sql Server provider registration here.</param>
        /// <param name="registerControllerInputFormatter">Whether or not to register the controller formatters i.e. allowing POST/PUT inputs requests in the formats provided in the NKit MimeContentType class.</param>
        /// <param name="logger">Optional ILogger which if specified is used to log all the NKit settings read from the appsettings.json file.</param>
        public static void RegisterDefaultNKitServices<D, R>(
            this IServiceCollection services,
            IConfiguration configuration,
            bool registerEmailService,
            bool registerEntityFrameworkSqlServerProvider,
            bool registerControllerInputFormatter,
            ILogger logger) where D : NKitDbContext where R : NKitDbContextRepository
        {
            DataValidator.ValidateObjectNotNull(configuration, nameof(configuration), nameof(SettingsServiceCollectionExtensions));
            DataValidator.ValidateObjectNotNull(services, nameof(services), nameof(SettingsServiceCollectionExtensions));
            NKitWebApiSettings webApiSettings = NKitWebApiSettings.GetSettings(configuration);
            NKitDatabaseSettings databaseSettings = NKitDatabaseSettings.GetSettings(configuration);
            NKitLoggingSettings loggingSettings = NKitLoggingSettings.GetSettings(configuration);
            NKitEmailSettings emailSettings = NKitEmailSettings.GetSettings(configuration);
            NKitWebApiClientSettings webApiClientSettings = NKitWebApiClientSettings.GetSettings(configuration);
            if (registerEmailService)
            {
                services.AddTransient<NKitEmailService>();
            }
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            if (registerEntityFrameworkSqlServerProvider)
            {
                services.AddDbContext<D>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(databaseSettings.DatabaseConnectionString, sqlServerOptionsBuilder => sqlServerOptionsBuilder.CommandTimeout(databaseSettings.DatabaseCommandTimeout)), ServiceLifetime.Transient); //Register the NKitDbContext.
            }
            else
            {
                services.AddDbContext<D>(ServiceLifetime.Transient); //Register the NKitDbContext.
            }
            services.AddTransient<R>(); //Register the NKitDbContextRepository.
            if (registerControllerInputFormatter)
            {
                services.AddControllers(mvcOptions => mvcOptions.InputFormatters.Insert(mvcOptions.InputFormatters.Count, new NKitWebApiControllerInputFormatter()));
            }
        }

        #endregion //Register Services Methods

        #endregion //Methods
    }
}
