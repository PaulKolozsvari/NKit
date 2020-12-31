namespace NKit.Web.Service.CoreRest.Middleware
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
    using NKit.Core.Utilities.Logging;
    using NKit.Core.Web.Service.CoreRest.Exceptions;
    using NKit.Data.DB.LINQ;

    #endregion //Using Directives

    /// <summary>
    /// Middleeware that enables you trap exceptions and automatically log them to the ApplLogger as well as well as to the NKitLogEntry database table if it exists in the database.
    /// Using Middleware to trap Exceptions in Asp.Net Core: https://blogs.msdn.microsoft.com/brandonh/2017/07/31/using-middleware-to-trap-exceptions-in-asp-net-core/
    /// ASP.NET Core - Middleware Pipeline: https://www.tutorialsteacher.com/core/aspnet-core-middleware
    /// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    /// </summary>
    public class HttpStatusCodeExceptionMiddlewareCore<D> where D : DbContextCrudTransactionsRepositoryCore
    {
        #region Constructors

        public HttpStatusCodeExceptionMiddlewareCore(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = AppLogger.CreateLogger(nameof(HttpStatusCodeExceptionMiddlewareCore<D>));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
            _context = _serviceProvider.GetService<D>();
        }

        #endregion //Constructors

        #region Constants

        private const string RESPONSE_STARTED_LOG_ERROR_MESSAGE = "The response has already started, the HTTP status code middleware will not be executed.";

        #endregion //Constants

        #region Fields

        protected readonly RequestDelegate _next;
        protected readonly ILogger _logger;
        protected IServiceScopeFactory _serviceScopeFactory;
        protected IServiceProvider _serviceProvider;
        protected DbContextCrudTransactionsRepositoryCore _context;

        #endregion //Fields

        #region Methods

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpStatusCodeExceptionCore ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(RESPONSE_STARTED_LOG_ERROR_MESSAGE);
                    throw;
                }
                _logger.LogError(ex.Message);
                if (_context != null)
                {
                    _context.LogExceptionToNKitLogEntry(ex, nameof(HttpStatusCodeExceptionMiddlewareCore<D>));
                }
                context.Response.Clear();
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = ex.ContentType;
                await context.Response.WriteAsync(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning(RESPONSE_STARTED_LOG_ERROR_MESSAGE);
                    throw;
                }
                _logger.LogError(ex.Message);
                if (_context != null)
                {
                    _context.LogExceptionToNKitLogEntry(ex, nameof(HttpStatusCodeExceptionMiddlewareCore<D>));
                }
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = @"text/plain";
                await context.Response.WriteAsync(ex.Message);
                return;
            }
            finally
            {
                //DisposeEntityContext();
            }
        }

        protected void DisposeEntityContext()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }

        #endregion //Methods
    }
}