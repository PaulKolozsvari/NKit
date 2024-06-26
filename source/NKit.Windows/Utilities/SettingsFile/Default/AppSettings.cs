﻿namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using NKit.Utilities.Email;
    using NKit.Utilities.Logging;
    using System.Collections.Generic;

    #endregion //Using Directives

    /// <summary>
    /// Default settings that any type of application can make use of.
    /// </summary>
    public partial class AppSettings : Settings
    {
        #region Constructors

        public AppSettings() : base()
        {
            DefaultEmailRecipients = new List<EmailNotificationRecipient>();
        }

        public AppSettings(string filePath) : base(filePath)
        {
            DefaultEmailRecipients = new List<EmailNotificationRecipient>();
        }

        public AppSettings(string name, string filePath) : base(name, filePath)
        {
            DefaultEmailRecipients = new List<EmailNotificationRecipient>();
        }

        #endregion //Constructors

        #region Properties

        #region Logging

        /// <summary>
        /// Whether or not to log to a text log file in the executing directory.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to a text log file in the executing directory.", CategorySequenceId = 0)]
        public bool LogToFile { get; set; }

        /// <summary>
        /// Whether or not to log to the Windows Event Log.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the Windows Event Log.", CategorySequenceId = 1)]
        public bool LogToWindowsEventLog { get; set; }

        /// <summary>
        /// Whether or not to log to the console.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the console.", CategorySequenceId = 2)]
        public bool LogToConsole { get; set; }

        /// <summary>
        /// Whether or not to log to the NKitLogEntry database table.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "Whether or not to log to the NKitLogEntry database table.", CategorySequenceId = 3)]
        public bool LogToDatabase { get; set; }

        /// <summary>
        /// The name of the text log file to log to. The log file is written in the executing directory.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the text log file to log to. The log file is written in the executing directory.", CategorySequenceId = 4)]
        public string LogFileName { get; set; }

        /// <summary>
        /// The name of the event source to use when logging to the Windows Event Log.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the event source to use when logging to the Windows Event Log.", CategorySequenceId = 5)]
        public string EventSourceName { get; set; }

        /// <summary>
        /// The name of the Windows Event Log to log to.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The name of the Windows Event Log to log to.", CategorySequenceId = 6)]
        public string EventLogName { get; set; }

        /// <summary>
        /// The extent of messages being logged: None = logging is disabled, Minimum = logs server start/stop and exceptions, Normal = logs additional information messages, Maximum = logs all requests and responses to the server.
        /// </summary>
        [SettingInfo("Logging", AutoFormatDisplayName = true, Description = "The extent of messages being logged: None = logging is disabled, Minimum = logs server start/stop and exceptions, Normal = logs additional information messages, Maximum = logs all requests and responses to the server.", CategorySequenceId = 7)]
        public LoggingLevel LoggingLevel { get; set; }

        #endregion //Logging        

        #region Email

        /// <summary>
        /// Whether or not email notifications should be enabled.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not email notifications should be enabled.", CategorySequenceId = 0)]
        public bool EmailNotificationsEnabled { get; set; }

        /// <summary>
        /// Whether exceptions that occur when sending emails should be thrown or handled.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether exceptions that occur when sending emails should be thrown or handled.", CategorySequenceId = 1)]
        public bool ThrowEmailFailExceptions { get; set; }

        /// <summary>
        /// The email (SMTP server) service provider that should be used for sending emails e.g. GMail/Exchange.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The email (SMTP server) service provider that should be used for sending emails e.g. GMail/Exchange.", CategorySequenceId = 2)]
        public EmailProvider EmailProvider { get; set; }

        /// <summary>
        /// The hostname of the Exchange email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The hostname of the Exchange email server.", CategorySequenceId = 3)]
        public string ExchangeSmtpServer { get; set; }

        /// <summary>
        /// The user name to use when authenticating against the Exchange email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The user name to use when authenticating against the Exchange email server.", CategorySequenceId = 4)]
        public string ExchangeSmtpUserName { get; set; }

        /// <summary>
        /// The password to use when authenticating against the Exchange email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The password to use when authenticating against the Exchange email server.", CategorySequenceId = 5)]
        public string ExchangeSmtpPassword { get; set; }

        /// <summary>
        /// The port to connect to when connecting to the Exchange email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The port to connect to when connecting to the Exchange email server.", CategorySequenceId = 6)]
        public int ExchangeSmtpPort { get; set; }

        /// <summary>
        /// Whether or not to connect to the Exchange server using SSL (Secure Socket Layer).
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to connect to the Exchange server using SSL (Secure Socket Layer).", CategorySequenceId = 7)]
        public bool ExchangeSmtpEnableSsl { get; set; }

        /// <summary>
        /// The hostname of the GMail server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The hostname of the GMail server.", CategorySequenceId = 8)]
        public string GMailSmtpServer { get; set; }

        /// <summary>
        /// The user name to use when authenticating against the GMail email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The user name to use when authenticating against the GMail email server.", CategorySequenceId = 9)]
        public string GMailSmtpUserName { get; set; }

        /// <summary>
        /// The password to use when authenticating against the GMail email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The password to use when authenticating against the GMail email server.", CategorySequenceId = 10)]
        public string GMailSmtpPassword { get; set; }

        /// <summary>
        /// The port to connect to when connecting to the GMail email server.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The port to connect to when connecting to the GMail email server.", CategorySequenceId = 11)]
        public int GMailSmtpPort { get; set; }

        /// <summary>
        /// The email address to display of the email sender.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The email address to display of the email sender.", CategorySequenceId = 12)]
        public string SenderEmailAddress { get; set; }

        /// <summary>
        /// The display name of the email sender.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The display name of the email sender.", CategorySequenceId = 12)]
        public string SenderDisplayName { get; set; }

        /// <summary>
        /// The subject to display on emails sent out due to exceptions handled by the Exception Handler.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The subject to display on emails sent out due to exceptions handled by the Exception Handler.", CategorySequenceId = 13)]
        public string ExceptionEmailSubject { get; set; }

        /// <summary>
        /// The subject to display on emails sent out due to exceptions handled by the Exception Handler.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to send emails on exceptions handled by the Exception Handler.", CategorySequenceId = 14)]
        public bool SendEmailOnException { get; set; }

        /// <summary>
        /// Whether or not add a line to every exception email sent out that includes the hostname of the machine running this software and thereby initiating the email.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not add a line to every exception email sent out that includes the hostname of the machine running this software and thereby initiating the email.", CategorySequenceId = 15)]
        public bool AppendHostNameToExceptionEmails { get; set; }

        /// <summary>
        /// Whether or not activity related to the sending of emails.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not to log activity related to the sending of emails.", CategorySequenceId = 16)]
        public bool EmailLoggingEnabled { get; set; }

        /// <summary>
        /// Whether or not include the list of default recipients in every email sent out.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "Whether or not include the list of default recipients in every email sent out.", CategorySequenceId = 17)]
        public bool IncludeDefaultEmailRecipients { get; set; }

        /// <summary>
        /// The default list of recipients that should be included in every email sent out i.e. if default recipients are configured to be included.
        /// </summary>
        [SettingInfo("Email", AutoFormatDisplayName = true, Description = "The default list of recipients that should be included in every email sent out i.e. if default recipients are configured to be included.", CategorySequenceId = 18)]
        public List<EmailNotificationRecipient> DefaultEmailRecipients { get; set; }

        #endregion //Email

        #region Threading

        /// <summary>
        /// The Minimum number of threads available in the Worker Thread Pool for each CPU in the machine.
        /// </summary>
        [SettingInfo("Threading", AutoFormatDisplayName = true, Description = "The Minimum number of threads available in the Worker Thread Pool for each CPU in the machine.", CategorySequenceId = 18)]
        public int MinimumWorkerThreadCount { get; set; }

        /// <summary>
        /// The Minimum number of threads available in the IOCP (IO Completion Port) Thread Pool for each CPU in the machine.
        /// </summary>
        [SettingInfo("Threading", AutoFormatDisplayName = true, Description = "The Minimum number of threads available in the IOCP (IO Completion Port) Thread Pool.", CategorySequenceId = 18)]
        public int MinimumCompletionPortThreadCount { get; set; }

        #endregion //Threading

        #endregion //Properties
    }
}
