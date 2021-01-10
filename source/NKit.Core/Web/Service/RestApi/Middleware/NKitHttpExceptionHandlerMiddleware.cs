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
    using NKit.Web.Service.RestApi.Exceptions;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.Email;

    #endregion //Using Directives

    /// <summary>
    /// Middleware that enables you trap exceptions and automatically log them to the ApplLogger as well as well as to the NKitLogEntry database table if it exists in the database.
    /// Using Middleware to trap Exceptions in Asp.Net Core: https://blogs.msdn.microsoft.com/brandonh/2017/07/31/using-middleware-to-trap-exceptions-in-asp-net-core/
    /// ASP.NET Core - Middleware Pipeline: https://www.tutorialsteacher.com/core/aspnet-core-middleware
    /// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    /// </summary>
    public class NKitHttpExceptionHandlerMiddleware<D> where D : NKitDbRepository
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
            IOptions<NKitHttpExceptionHandlerMiddlewareSettings> middlewareSettings, 
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailClient emailClient)
        {
            try
            {
                await _next(context);
            }
            catch (NKitHttpStatusCodeException ex)
            {
                HandleException(ex, ex.EventId, context, dbContextRepo, middlewareSettings, logger, emailClient);
                NKitHttpExceptionResponse response = new NKitHttpExceptionResponse(ex, middlewareSettings.Value.IncludeStackTraceInExceptionResponse);
                string responseText = response.GetResponseText(middlewareSettings.Value.SerializerType, middlewareSettings.Value.ResponseContentType);
                context.Response.Clear();
                context.Response.StatusCode = (int)ex.StatusCode;
                context.Response.ContentType = response.ContentType;
                await context.Response.WriteAsync(responseText);
                return;
            }
            catch (Exception ex)
            {
                HandleException(ex, null, context, dbContextRepo, middlewareSettings, logger, emailClient);
                int httpStatusCode = (int)HttpStatusCode.InternalServerError;
                NKitHttpExceptionResponse response = new NKitHttpExceptionResponse(ex, httpStatusCode, null, null, middlewareSettings.Value.IncludeStackTraceInExceptionResponse);
                string responseText = response.GetResponseText(middlewareSettings.Value.SerializerType, middlewareSettings.Value.ResponseContentType);
                context.Response.Clear();
                context.Response.StatusCode = httpStatusCode;
                context.Response.ContentType = response.ContentType;
                await context.Response.WriteAsync(responseText);
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

        private void HandleException(
            Exception ex,
            Nullable<EventId> eventId,
            HttpContext context,
            D dbContextRepo,
            IOptions<NKitHttpExceptionHandlerMiddlewareSettings> middlewareSettings,
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailClient emailClient)
        {
            if (context.Response.HasStarted)
            {
                logger.LogWarning(RESPONSE_STARTED_LOG_ERROR_MESSAGE);
                throw ex;
            }
            string message = ExceptionHandler.GetCompleteExceptionMessage(ex, middlewareSettings.Value.IncludeExceptionStackTraceInLoggerEntry);
            if (eventId.HasValue)
            {
                logger.Log(LogLevel.Error, eventId.Value, ex, message);
            }
            else
            {
                logger.Log(LogLevel.Error, ex, message);
            }
            dbContextRepo.LogExceptionToNKitLogEntry(ex, eventId, middlewareSettings.Value.IncludeExceptionStackTraceInDatabaseNKitLogEntry);
            if (middlewareSettings.Value.SendEmailOnException)
            {
                emailClient.SendExceptionEmailNotification(ex, out string errorMessage, out string emailLogMessageText, middlewareSettings.Value.AppendHostNameToExceptionEmails, null);
            }
        }

        #endregion //Methods
    }
}