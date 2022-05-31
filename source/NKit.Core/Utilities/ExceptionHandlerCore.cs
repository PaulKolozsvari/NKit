namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Data.DB.LINQ;
    using NKit.Settings.Default;
    using NKit.Utilities.Email;
    using NKit.Utilities.Logging;

    #endregion //Using Directives

    /// <summary>
    /// A helper class for handling exceptions i.e. logging and displaying them.
    /// </summary>
    public class ExceptionHandlerCore
    {
        #region Methods

        /// <summary>
        /// Gets the complete error message including the exception message, inner exception message (if it exists) and stack trace.
        /// </summary>
        /// <param name="ex">Exception whose message to be retrieved.</param>
        /// <param name="includeStackTrace">Whether to include the exeption's stack trace in the message.</param>
        /// <returns></returns>
        public static string GetCompleteExceptionMessage(Exception ex, bool includeStackTrace)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(ex.Message);
            if (ex.InnerException != null)
            {
                result.AppendLine(ex.InnerException.Message);
            }
            if (includeStackTrace)
            {
                result.AppendLine(ex.StackTrace);
            }
            return result.ToString();
        }

        public static bool HandleException(
            ILogger logger, 
            Exception exception,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            return HandleException(logger, exception, eventDetailsMessage: null, true, out string emailErrorMessage, out string emailLogErrorMessage, null, emailClientService, dbContext);
        }

        public static bool HandleException(
            ILogger logger, 
            Exception exception, 
            List<EmailNotificationRecipient> emailNotificationRecipients,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            return HandleException(logger, exception, eventDetailsMessage: null, true, out string emailErrorMessage, out string emailLogErrorMessage, emailNotificationRecipients, emailClientService, dbContext);
        }

        public static bool HandleException(
            ILogger logger,
            Exception exception,
            out string emailErrorMessage,
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            return HandleException(logger, exception, eventDetailsMessage: null, true, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients, emailClientService, dbContext);
        }

        public static bool HandleException(
            ILogger logger,
            Exception exception,
            bool emailException,
            out string emailErrorMessage,
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            return HandleException(logger, exception, eventDetailsMessage: null, emailException, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients, emailClientService, dbContext);
        }

        public static bool HandleException(
            ILogger logger,
            Exception exception,
            string eventDetailsMessage,
            out string emailErrorMessage,
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            return HandleException(logger, exception, eventDetailsMessage, true, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients, emailClientService, dbContext);
        }

        public static bool HandleException(
            ILogger logger,
            Exception exception,
            string eventDetailsMessage,
            bool emailException,
            out string emailErrorMessage,
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients,
            NKitEmailClientService emailClientService,
            NKitDbContext dbContext)
        {
            try
            {
                emailErrorMessage = string.Empty;
                emailLogMessageText = string.Empty;
                bool closeApplication = false;
                if (exception == null)
                {
                    throw new NullReferenceException("exception to be handled may not be null.");
                }
                UserThrownException userThrownException = exception as UserThrownException;
                if (userThrownException != null)
                {
                    closeApplication = userThrownException.CloseApplication;
                }
                else
                {
                }
                if (logger != null)
                {
                    logger.LogError(exception, (new LogError(exception, eventDetailsMessage, LoggingLevel.Normal).ToString()));
                }
                if (emailException && (emailClientService != null))
                {
                    emailClientService.SendExceptionEmailNotification(exception, out emailErrorMessage, out emailLogMessageText, true, emailNotificationRecipients);
                }
                if (dbContext != null)
                {
                    dbContext.LogExceptionToNKitLogEntry(exception, eventId: null, true);
                }
                return closeApplication;
            }
            catch (Exception ex)
            {
                Exception wrappedException = new Exception(ex.Message, exception);
                Console.WriteLine(LogError.GetErrorMessageFromException(new Exception(ex.Message, exception), eventDetailsMessage));
                if(logger != null)
                {
                    logger.LogError(ex, new LogError(wrappedException, eventDetailsMessage, LoggingLevel.Normal).ToString());
                }
                emailErrorMessage = string.Empty;
                emailLogMessageText = string.Empty;
                return true;
            }
        }

        #endregion //Methods
    }
}
