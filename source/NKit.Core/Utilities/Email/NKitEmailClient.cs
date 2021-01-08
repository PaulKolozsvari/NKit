namespace NKit.Utilities.Email
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public class NKitEmailClient : EmailClient
    {
        #region Constructors

        public NKitEmailClient(
            IOptions<NKitEmailCllientSettings> emailOptions,
            ILogger<NKitEmailClient> logger) :
            base(emailOptions.Value.EmailNotificationsEnabled,
                emailOptions.Value.ThrowEmailFailExceptions,
                emailOptions.Value.EmailProvider,
                emailOptions.Value.ExchangeSmtpServer,
                emailOptions.Value.ExchangeSmtpUserName,
                emailOptions.Value.ExchangeSmtpPassword,
                emailOptions.Value.ExchangeSmtpPort,
                emailOptions.Value.ExchangeSmtpEnableSsl,
                emailOptions.Value.GMailSmtpServer,
                emailOptions.Value.GMailSmtpUserName,
                emailOptions.Value.GMailSmtpPassword,
                emailOptions.Value.GMailSmtpPort,
                emailOptions.Value.SenderEmailAddress,
                emailOptions.Value.SenderDisplayName,
                emailOptions.Value.ExceptionEmailSubject,
                emailOptions.Value.EmailLoggingEnabled,
                emailOptions.Value.IncludeDefaultEmailRecipients,
                emailOptions.Value.DefaultEmailRecipients)
        {
            _logger = logger;
        }

        #endregion //Constructors

        #region Fields

        protected ILogger _logger;

        #endregion //Fields

        #region Methods

        protected override bool LogEmailNotification(MailMessage email, string subject, out string logMessage)
        {
            if (!_emailLoggingEnabled)
            {
                logMessage = null;
                return false;
            }
            logMessage = GetEmailNotificationLogMessage(email, subject);
            if (_logger != null)
            {
                _logger.LogInformation(logMessage);
            }
            return true;
        }

        #endregion //Methods
    }
}
