namespace NKit.Web.Service.RestApi.Middleware
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Core.Utilities.Email;
    using NKit.Web.Service.RestApi.Exceptions;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    /// <summary>
    /// Middleware that enables you trap exceptions and automatically log them to the ApplLogger as well as well as to the NKitLogEntry database table if it exists in the database.
    /// Using Middleware to trap Exceptions in Asp.Net Core: https://blogs.msdn.microsoft.com/brandonh/2017/07/31/using-middleware-to-trap-exceptions-in-asp-net-core/
    /// ASP.NET Core - Middleware Pipeline: https://www.tutorialsteacher.com/core/aspnet-core-middleware
    /// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    /// </summary>
    public class NKitHttpExceptionHandlerMiddleware<D> where D : NKitDbContextRepository
    {
        #region Constructors

        public NKitHttpExceptionHandlerMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
        }

        #endregion //Constructors

        #region Constants

        private const string RESPONSE_STARTED_LOG_ERROR_MESSAGE = "The response has already started, the HTTP status code middleware will not be executed.";

        #endregion //Constants

        #region Fields

        protected readonly RequestDelegate _next;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected IServiceProvider _serviceProvider;

        #endregion //Fields

        #region Methods

        public async Task Invoke(
            HttpContext context,
            D dbContextRepo,
            IOptions<NKitWebApiSettings> webApiOptions, 
            IOptions<NKitDatabaseSettings> databaseOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailService emailService)
        {
            try
            {
                await _next(context);
            }
            catch (NKitHttpStatusCodeException ex)
            {
                string message = HandleException(ex, ex.EventId, context, dbContextRepo, webApiOptions, databaseOptions, loggingOptions, logger, emailService);
                context.Response.Clear();
                context.Response.StatusCode = (int)ex.StatusCode;
                context.Response.ContentType = ex.ContentType;
                await context.Response.WriteAsync(message);
                return;
            }
            catch (Exception ex)
            {
                string message = HandleException(ex, null, context, dbContextRepo, webApiOptions, databaseOptions, loggingOptions, logger, emailService);
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = MimeContentType.TEXT_PLAIN;
                await context.Response.WriteAsync(message);
                return;
            }
            finally
            {
                if (dbContextRepo != null)
                {
                    dbContextRepo.Dispose();
                }
            }
        }

        private string HandleException(
            Exception ex,
            Nullable<EventId> eventId,
            HttpContext context,
            D dbContextRepo,
            IOptions<NKitWebApiSettings> webApiOptions,
            IOptions<NKitDatabaseSettings> databaseOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailService emailService)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(RESPONSE_STARTED_LOG_ERROR_MESSAGE);
                throw ex;
            }
            string message = ExceptionHandler.GetCompleteExceptionMessage(ex, webApiOptions.Value.IncludeExceptionStackTraceInErrorResponse);
            if (eventId.HasValue)
            {
                logger.Log(LogLevel.Error, eventId.Value, ex, message);
            }
            else
            {
                logger.Log(LogLevel.Error, ex, message);
            }
            dbContextRepo.LogExceptionToNKitLogEntry(ex, eventId, webApiOptions.Value.IncludeExceptionStackTraceInErrorResponse);
            emailService.EmailClient.SendExceptionEmailNotification(ex, out string errorMessage, out string emailLogMessageText, true, null);
            return message;
        }

        #endregion //Methods
    }
}