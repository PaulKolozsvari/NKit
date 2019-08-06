namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using NKit.Utilities.Logging;
    using NKit.Data;

    #endregion //Using Directives

    /// <summary>
    /// A helper class for handling exceptions i.e. logging and displaying them.
    /// </summary>
    public class ExceptionHandlerWindows : ExceptionHandler
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
            return HandleException(exception, null, null, null, true);
        }

        public static bool HandleException(Exception exception, bool emailException)
        {
            return HandleException(exception, null, null, null, emailException);
        }

        public static bool HandleException(Exception exception, string eventDetailsMessage)
        {
            return HandleException(exception, null, null, eventDetailsMessage, true);
        }

        public static bool HandleException(Exception exception, string eventDetailsMessage, bool emailException)
        {
            return HandleException(exception, null, null, eventDetailsMessage, emailException);
        }

        public static bool HandleException(Exception exception, Form form, KeyEventHandler keyEventHandler, string eventDetailsMessage)
        {
            return HandleException(exception, form, keyEventHandler, eventDetailsMessage, true);
        }

        public static bool HandleException(Exception exception, Form form, KeyEventHandler keyEventHandler, string eventDetailsMessage, bool emailException)
        {
            try
            {
                bool closeApplication = false;
                if (exception == null)
                {
                    throw new NullReferenceException("exception to be handled may not be null.");
                }
                if (GOCWindows.Instance.ShowMessageBoxOnException)
                {
                    UIHelperWindows.DisplayException(exception, form, keyEventHandler, eventDetailsMessage);
                }
                if (GOCWindows.Instance.ShowWindowsFormsNotificationOnException && GOCWindows.Instance.NotifyIcon != null)
                {
                    string applicationName = string.IsNullOrEmpty(GOCWindows.Instance.ApplicationName) ? "Notification" : GOCWindows.Instance.ApplicationName;
                    GOCWindows.Instance.NotifyIcon.ShowBalloonTip(10, applicationName, exception.Message, ToolTipIcon.Error);
                }
                UserThrownException userThrownException = exception as UserThrownException;
                if (userThrownException != null)
                {
                    closeApplication = userThrownException.CloseApplication;
                    GOCWindows.Instance.Logger.LogMessage(new LogError(exception, eventDetailsMessage, userThrownException.LoggingLevel));
                }
                else
                {
                    GOCWindows.Instance.Logger.LogMessage(new LogError(exception, eventDetailsMessage, LoggingLevel.Normal));
                }
                if (emailException && GOCWindows.Instance.SendEmailOnException)
                {
                    if (GOCWindows.Instance.EmailClient == null)
                    {
                        throw new Exception(string.Format("{0} enabled, but {1} not set.",
                            EntityReaderGeneric<GOCWindows>.GetPropertyName(p => p.SendEmailOnException, false),
                            EntityReaderGeneric<GOCWindows>.GetPropertyName(p => p.EmailClient, false)));
                    }
                    GOCWindows.Instance.EmailClient.SendExceptionEmailNotification(exception, out string errorMessage, GOCWindows.Instance.AppendHostNameToExceptionEmails);
                }
                return closeApplication;
            }
            catch (Exception ex)
            {
                Exception wrappedException = new Exception(ex.Message, exception);
                Console.WriteLine(LogError.GetErrorMessageFromException(new Exception(ex.Message, exception), eventDetailsMessage));
                GOCWindows.Instance.Logger.LogMessageToFile(new LogError(wrappedException, eventDetailsMessage, LoggingLevel.Minimum));
                return true;
            }
        }

        #endregion //Methods
    }
}