﻿namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Logging;
    using NKit.Data;
    using NKit.Utilities.Email;

    #endregion //Using Directives

    /// <summary>
    /// A helper class for handling exceptions i.e. logging and displaying them.
    /// </summary>
    public partial class ExceptionHandler
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
            result.AppendLine($"Exception: {ex.Message}");
            if (!string.IsNullOrEmpty(ex.Source))
            {
                result.AppendLine($"Source: {ex.Source}");
            }
            string className = ex.TargetSite != null ? ex.TargetSite.DeclaringType.FullName : string.Empty;
            string functionName = ex.TargetSite != null ? ex.TargetSite.Name : string.Empty;
            result.AppendLine($"Class Name: {className}");
            result.AppendLine($"Function Name: {functionName}");
            if (ex.InnerException != null)
            {
                result.AppendLine($"Inner Exception: {ex.InnerException.Message}");
            }
            if (includeStackTrace)
            {
                result.AppendLine("Stack Trace:");
                result.AppendLine(ex.StackTrace);
            }
            return result.ToString();
        }

        public static bool HandleException(Exception exception)
        {
            return HandleException(exception, out string emailErrorMessage, out string emailLogErrorMessage, null);
        }

        public static bool HandleException(Exception exception, List<EmailNotificationRecipient> emailNotificationRecipients)
        {
            return HandleException(exception, out string emailErrorMessage, out string emailLogErrorMessage, emailNotificationRecipients);
        }

        public static bool HandleException(Exception exception, out string emailErrorMessage, out string emailLogMessageText, List<EmailNotificationRecipient> emailNotificationRecipients)
        {
            return HandleException(exception, null, true, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients);
        }

        public static bool HandleException(Exception exception, bool emailException, out string emailErrorMessage, out string emailLogMessageText, List<EmailNotificationRecipient> emailNotificationRecipients)
        {
            return HandleException(exception, null, emailException, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients);
        }

        public static bool HandleException(
            Exception exception, 
            string eventDetailsMessage, 
            out string emailErrorMessage, 
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients)
        {
            return HandleException(exception, eventDetailsMessage, true, out emailErrorMessage, out emailLogMessageText, emailNotificationRecipients);
        }

        public static bool HandleException(
            Exception exception, 
            string eventDetailsMessage, 
            bool emailException, 
            out string emailErrorMessage, 
            out string emailLogMessageText,
            List<EmailNotificationRecipient> emailNotificationRecipients)
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
                    GOC.Instance.Logger.LogMessage(new LogError(exception, eventDetailsMessage, userThrownException.LoggingLevel));
                }
                else
                {
                    GOC.Instance.Logger.LogMessage(new LogError(exception, eventDetailsMessage, LoggingLevel.Normal));
                }
                if (emailException && GOC.Instance.SendEmailOnException)
                {
                    if (GOC.Instance.EmailClient == null)
                    {
                        throw new Exception(string.Format("{0} enabled, but {1} not set.",
                            EntityReaderGeneric<GOC>.GetPropertyName(p => p.SendEmailOnException, false),
                            EntityReaderGeneric<GOC>.GetPropertyName(p => p.EmailClient, false)));
                    }
                    GOC.Instance.EmailClient.SendExceptionEmailNotification(exception, out emailErrorMessage, out emailLogMessageText, GOC.Instance.AppendHostNameToExceptionEmails, emailNotificationRecipients);
                }
                return closeApplication;
            }
            catch (Exception ex)
            {
                Exception wrappedException = new Exception(ex.Message, exception);
                Console.WriteLine(LogError.GetErrorMessageFromException(new Exception(ex.Message, exception), eventDetailsMessage));
                GOC.Instance.Logger.LogMessageToFile(new LogError(wrappedException, eventDetailsMessage, LoggingLevel.Minimum));
                emailErrorMessage = string.Empty;
                emailLogMessageText = string.Empty;
                return true;
            }
        }

        #endregion //Methods
    }
}