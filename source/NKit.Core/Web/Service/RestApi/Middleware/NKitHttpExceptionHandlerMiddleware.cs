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
    using NKit.Data.DB.LINQ.Models;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.Email;
    using NKit.Settings.Default;
    using System.IO;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Http.Extensions;

    #endregion //Using Directives

    /// <summary>
    /// Middleware that enables you trap exceptions and automatically log them to the ApplLogger as well as well as to the NKitLogEntry database table if it exists in the database.
    /// Using Middleware to trap Exceptions in Asp.Net Core: https://blogs.msdn.microsoft.com/brandonh/2017/07/31/using-middleware-to-trap-exceptions-in-asp-net-core/
    /// ASP.NET Core - Middleware Pipeline: https://www.tutorialsteacher.com/core/aspnet-core-middleware
    /// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    /// </summary>
    public class NKitHttpExceptionHandlerMiddleware<D> where D : NKitDbContext
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
            D dbContext,
            IOptions<NKitHttpExceptionHandlerMiddlewareSettings> middlewareSettings, 
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailClientService emailClient)
        {
            try
            {
                if (middlewareSettings != null && middlewareSettings.Value != null && middlewareSettings.Value.LogAllRequests)
                {
                    LogRequest(context, logger);
                }
                await _next(context);
            }
            catch (NKitHttpStatusCodeException ex)
            {
                HandleException(ex, ex.EventId, context, dbContext, middlewareSettings, logger, emailClient);
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
                HandleException(ex, null, context, dbContext, middlewareSettings, logger, emailClient);
                int httpStatusCode = (int)HttpStatusCode.InternalServerError;
                NKitHttpExceptionResponse response = new NKitHttpExceptionResponse(ex, httpStatusCode, null, null, middlewareSettings.Value.IncludeStackTraceInExceptionResponse);
                string responseText = response.GetResponseText(middlewareSettings.Value.SerializerType, middlewareSettings.Value.ResponseContentType);
                context.Response.Clear();
                context.Response.StatusCode = httpStatusCode;
                context.Response.ContentType = response.ContentType;
                context.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = ex.Message;
                await context.Response.WriteAsync(responseText);
                return;
            }
            finally
            {
                if (dbContext != null)
                {
                    dbContext.Dispose();
                }
            }
        }

        private async void LogRequest(HttpContext context, ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger)
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.AppendLine($"URI: {context.Request.GetDisplayUrl()}");
            messageBuilder.AppendLine($"Host: {context.Request.Host.Host}");
            string body = await GetRequestBody(context);
            if (!string.IsNullOrEmpty(body))
            {
                messageBuilder.AppendLine($"{body}");
            }
            string message = messageBuilder.ToString();
            logger.LogInformation(message);
        }

        private async Task<string> GetRequestBody(HttpContext context)
        {
            string result = string.Empty;
            context.Request.EnableBuffering();
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader stream = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                result = await stream.ReadToEndAsync();
            }
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private void HandleException(
            Exception ex,
            Nullable<EventId> eventId,
            HttpContext context,
            D dbContext,
            IOptions<NKitHttpExceptionHandlerMiddlewareSettings> middlewareSettings,
            ILogger<NKitHttpExceptionHandlerMiddleware<D>> logger,
            NKitEmailClientService emailClient)
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
            dbContext.LogExceptionToNKitLogEntry(ex, eventId, middlewareSettings.Value.IncludeExceptionStackTraceInDatabaseNKitLogEntry);
            if (middlewareSettings.Value.SendEmailOnException)
            {
                emailClient.SendExceptionEmailNotification(ex, out string errorMessage, out string emailLogMessageText, middlewareSettings.Value.AppendHostNameToExceptionEmails, null);
            }
        }

        #endregion //Methods
    }
}