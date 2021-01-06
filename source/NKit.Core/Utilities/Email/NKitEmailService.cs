namespace NKit.Core.Utilities.Email
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Utilities.Email;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public class NKitEmailService
    {
        #region Constructors

        public NKitEmailService(IOptions<NKitEmailSettings> emailOptions)
        {
            DataValidator.ValidateObjectNotNull(emailOptions, nameof(emailOptions), nameof(NKitEmailService));
            _emailClient = GetEmailClient(emailOptions.Value);
        }

        #endregion //Constructors

        #region Fields

        protected readonly EmailClient _emailClient;

        #endregion //Fields

        #region Properties

        public EmailClient EmailClient
        {
            get { return _emailClient; }
        }

        #endregion //Properties

        #region Methods

        protected EmailClient GetEmailClient(NKitEmailSettings emailSettings)
        {
            return new EmailClient(
                emailSettings.EmailNotificationsEnabled,
                emailSettings.ThrowEmailFailExceptions,
                emailSettings.EmailProvider,
                emailSettings.ExchangeSmtpServer,
                emailSettings.ExchangeSmtpUserName,
                emailSettings.ExchangeSmtpPassword,
                emailSettings.ExchangeSmtpPort,
                emailSettings.ExchangeSmtpEnableSsl,
                emailSettings.GMailSmtpServer,
                emailSettings.GMailSmtpUserName,
                emailSettings.GMailSmtpPassword,
                emailSettings.GMailSmtpPort,
                emailSettings.SenderEmailAddress,
                emailSettings.SenderDisplayName,
                emailSettings.ExceptionEmailSubject,
                emailSettings.EmailLoggingEnabled,
                emailSettings.IncludeDefaultEmailRecipients,
                emailSettings.DefaultEmailRecipients);
        }

        #endregion //Methods
    }
}
