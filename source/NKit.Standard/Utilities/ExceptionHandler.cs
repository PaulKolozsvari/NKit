namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Logging;
    using NKit.Data;

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

        public static bool HandleException(Exception exception)
        {
            return HandleException(exception, null, true);
        }

        public static bool HandleException(Exception exception, bool emailException)
        {
            return HandleException(exception, null, emailException);
        }

        public static bool HandleException(Exception exception, string eventDetailsMessage)
        {
            return HandleException(exception, eventDetailsMessage, true);
        }

        public static bool HandleException(Exception exception, string eventDetailsMessage, bool emailException)
        {
            try
            {
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
                    GOC.Instance.EmailClient.SendExceptionEmailNotification(exception, out string errorMessage, GOC.Instance.AppendHostNameToExceptionEmails);
                }
                return closeApplication;
            }
            catch (Exception ex)
            {
                Exception wrappedException = new Exception(ex.Message, exception);
                Console.WriteLine(LogError.GetErrorMessageFromException(new Exception(ex.Message, exception), eventDetailsMessage));
                GOC.Instance.Logger.LogMessageToFile(new LogError(wrappedException, eventDetailsMessage, LoggingLevel.Minimum));
                return true;
            }
        }

        #endregion //Methods
    }
}