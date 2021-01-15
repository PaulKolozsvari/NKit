namespace NKit.Settings.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.Email;

    #endregion //Using Directives

    public class NKitEmailClientServiceSettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// Whether or not email notifications should be enabled.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not email notifications should be enabled.", CategorySequenceId = 0)]
        public bool EmailNotificationsEnabled { get; set; }

        /// <summary>
        /// Whether exceptions that occur when sending emails should be thrown or handled.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether exceptions that occur when sending emails should be thrown or handled.", CategorySequenceId = 1)]
        public bool ThrowEmailFailExceptions { get; set; }

        /// <summary>
        /// The email (SMTP server) service provider that should be used for sending emails e.g. GMail/Exchange.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The email (SMTP server) service provider that should be used for sending emails e.g. GMail/Exchange.", CategorySequenceId = 2)]
        public EmailProvider EmailProvider { get; set; }

        /// <summary>
        /// The hostname of the Exchange email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The hostname of the Exchange email server.", CategorySequenceId = 3)]
        public string ExchangeSmtpServer { get; set; }

        /// <summary>
        /// The user name to use when authenticating against the Exchange email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The user name to use when authenticating against the Exchange email server.", CategorySequenceId = 4)]
        public string ExchangeSmtpUserName { get; set; }

        /// <summary>
        /// The password to use when authenticating against the Exchange email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The password to use when authenticating against the Exchange email server.", CategorySequenceId = 5)]
        public string ExchangeSmtpPassword { get; set; }

        /// <summary>
        /// The port to connect to when connecting to the Exchange email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The port to connect to when connecting to the Exchange email server.", CategorySequenceId = 6)]
        public int ExchangeSmtpPort { get; set; }

        /// <summary>
        /// Whether or not to connect to the Exchange server using SSL (Secure Socket Layer).
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to connect to the Exchange server using SSL (Secure Socket Layer).", CategorySequenceId = 7)]
        public bool ExchangeSmtpEnableSsl { get; set; }

        /// <summary>
        /// The hostname of the GMail server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The hostname of the GMail server.", CategorySequenceId = 8)]
        public string GMailSmtpServer { get; set; }

        /// <summary>
        /// The user name to use when authenticating against the GMail email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The user name to use when authenticating against the GMail email server.", CategorySequenceId = 9)]
        public string GMailSmtpUserName { get; set; }

        /// <summary>
        /// The password to use when authenticating against the GMail email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The password to use when authenticating against the GMail email server.", CategorySequenceId = 10)]
        public string GMailSmtpPassword { get; set; }

        /// <summary>
        /// The port to connect to when connecting to the GMail email server.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The port to connect to when connecting to the GMail email server.", CategorySequenceId = 11)]
        public int GMailSmtpPort { get; set; }

        /// <summary>
        /// The email address to display of the email sender.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The email address to display of the email sender.", CategorySequenceId = 12)]
        public string SenderEmailAddress { get; set; }

        /// <summary>
        /// The display name of the email sender.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The display name of the email sender.", CategorySequenceId = 12)]
        public string SenderDisplayName { get; set; }

        /// <summary>
        /// The subject to display on emails sent out due to exceptions handled by the Exception Handler.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The subject to display on emails sent out due to exceptions handled by the Exception Handler.", CategorySequenceId = 13)]
        public string ExceptionEmailSubject { get; set; }

        /// <summary>
        /// Whether or not the sending of emails is logged.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to log activity related to the sending of emails.", CategorySequenceId = 16)]
        public bool EmailLoggingEnabled { get; set; }

        /// <summary>
        /// Whether or not include the list of default recipients in every email sent out.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not include the list of default recipients in every email sent out.", CategorySequenceId = 17)]
        public bool IncludeDefaultEmailRecipients { get; set; }

        /// <summary>
        /// The default list of recipients that should be included in every email sent out i.e. if default recipients are configured to be included.
        /// </summary>
        [NKitSettingInfo("Email", AutoFormatDisplayName = true, Description = "The default list of recipients that should be included in every email sent out i.e. if default recipients are configured to be included.", CategorySequenceId = 18)]
        public List<EmailNotificationRecipient> DefaultEmailRecipients { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The Configuration object is created read from based on the appsettings.json. The appsettings.json file name is determined by reading the ASPNETCORE_ENVIRONMENT variable i.e. appsettings.{environment}.json or appsettings.json when the environment variable is not set.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitEmailClientServiceSettings GetSettings()
        {
            return GetSettings<NKitEmailClientServiceSettings>();
        }

        /// <summary>
        /// Reads the NKitEmailSettings configuration section from the appsettings.json file and deserializes to an instance of NKitEmailSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitEmailClientServiceSettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitEmailClientServiceSettings>(configuration);
        }

        /// <summary>
        /// Register Configuration from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static NKitEmailClientServiceSettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitEmailClientServiceSettings>(configuration, services);
        }

        #endregion //Methods
    }
}
