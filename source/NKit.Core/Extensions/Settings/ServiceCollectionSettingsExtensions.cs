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
    using NKit.Data;
    using NKit.Data.DB.LINQ;
    using NKit.Settings;
    using NKit.Settings.Default;
    using NKit.Utilities;
    using NKit.Utilities.Email;
    using NKit.Utilities.Logging;
    using NKit.Web.Service.RestApi.ContentMapping;

    #endregion //Using Directives

    public static class ServiceCollectionSettingsExtensions
    {
        #region Methods

        #region Register Settings Methods

        /// <summary>
        /// Register default Configurations from the appsettings.json which will be made available as IOptions to all services in the DI service container.
        /// </summary>
        /// <param name="services">The IServiceCollection passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="configuration">The IConfiguration passed into the ConfigureServices method in the Startup class.</param>
        /// <param name="generalSettings">Returns the General settings that have been registered.</param>
        /// <param name="webApiSettings">Returns the Web API settings that have been registered.</param>
        /// <param name="httpExceptionHandlerMiddlewareSettings">Returns the HTTP Exception Handler Middler settings that have been registered.</param>
        /// <param name="webApiClientSettings">Returns the Web API Client settings that have been registered.</param>
        /// <param name="databaseSettings">Returns the Database settings that have been registered.</param>
        /// <param name="loggingSettings">Returns the Logging settings that have been registered.</param>
        /// <param name="emailSettings">Returns the Email settings that have been registered.</param>
        /// <param name="loggerCategoryName">Optional category name of the ILogger that will be created to log the settings being registered.</param>
        public static void RegisterDefaultNKitSettings(
            this IServiceCollection services,
            IConfiguration configuration,
            out NKitGeneralSettings generalSettings,
            out NKitWebApiControllerSettings webApiSettings,
            out NKitHttpExceptionHandlerMiddlewareSettings httpExceptionHandlerMiddlewareSettings,
            out NKitWebApiClientSettings webApiClientSettings,
            out NKitDbContextSettings databaseSettings,
            out NKitLoggingSettings loggingSettings,
            out NKitEmailClientServiceSettings emailSettings,
            string loggerCategoryName)
        {
            ILogger logger = !string.IsNullOrEmpty(loggerCategoryName) ? NKitLoggingHelper.CreateLogger(loggerCategoryName, NKitLoggingSettings.GetSettings(configuration)) : null;
            RegisterDefaultNKitSettings(services, configuration,
                out generalSettings,
                out webApiSettings,
                out httpExceptionHandlerMiddlewareSettings,
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
        /// <param name="httpExceptionHandlerMiddlewareSettings">Returns the HTTP Exception Handler Middler settings that have been registered.</param>
        /// <param name="webApiClientSettings">Returns the Web API Client settings that have been registered.</param>
        /// <param name="databaseSettings">Returns the Database settings that have been registered.</param>
        /// <param name="loggingSettings">Returns the Logging settings that have been registered.</param>
        /// <param name="emailSettings">Returns the Email settings that have been registered.</param>
        /// <param name="logger">Optional ILogger which if specified is used to log all the NKit settings read from the appsettings.json file.</param>
        public static void RegisterDefaultNKitSettings(
            this IServiceCollection services,
            IConfiguration configuration,
            out NKitGeneralSettings generalSettings,
            out NKitWebApiControllerSettings webApiSettings,
            out NKitHttpExceptionHandlerMiddlewareSettings httpExceptionHandlerMiddlewareSettings,
            out NKitWebApiClientSettings webApiClientSettings,
            out NKitDbContextSettings databaseSettings,
            out NKitLoggingSettings loggingSettings,
            out NKitEmailClientServiceSettings emailSettings,
            ILogger logger)
        {
            generalSettings = NKitGeneralSettings.RegisterConfiguration(configuration, services);
            webApiSettings = NKitWebApiControllerSettings.RegisterConfiguration(configuration, services);
            httpExceptionHandlerMiddlewareSettings = NKitHttpExceptionHandlerMiddlewareSettings.RegisterConfiguration(configuration, services);
            webApiClientSettings = NKitWebApiClientSettings.RegisterConfiguration(configuration, services);
            databaseSettings = NKitDbContextSettings.RegisterConfiguration(configuration, services);
            loggingSettings = NKitLoggingSettings.RegisterConfiguration(configuration, services);
            emailSettings = NKitEmailClientServiceSettings.RegisterConfiguration(configuration, services);
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

            logMessage.AppendLine($"*** {nameof(NKitWebApiControllerSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(webApiSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitHttpExceptionHandlerMiddlewareSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(httpExceptionHandlerMiddlewareSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitDbContextSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(databaseSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitLoggingSettings)} ***");
            logMessage.AppendLine(GOC.Instance.JsonSerializer.SerializeToText(loggingSettings));
            logMessage.AppendLine();

            logMessage.AppendLine($"*** {nameof(NKitEmailClientServiceSettings)} ***");
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

        #endregion //Methods
    }
}
