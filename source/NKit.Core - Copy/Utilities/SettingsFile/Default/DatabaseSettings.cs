namespace NKit.Core.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Transactions;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public class DatabaseSettings : Settings
    {
        /// <summary>
        /// The connection string to the server database.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The connection string to the server database.", CategorySequenceId = 0)]
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// The timeout in milliseconds of the commands sent to the server database.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The timeout in milliseconds of the commands sent to the server database.", CategorySequenceId = 1)]
        public int DatabaseCommandTimeout { get; set; }

        /// <summary>
        /// The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 2)]
        public TransactionScopeOption DatabaseTransactionScopeOption { get; set; }

        /// <summary>
        /// The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 3)]
        public IsolationLevel DatabaseTransactionIsolationLevel { get; set; }

        /// <summary>
        /// The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.", CategorySequenceId = 4)]
        public int DatabaseTransactionTimeoutSeconds { get; set; }

        /// <summary>
        /// The number of retry attempts if transaction deadlocks occur on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The number of retry attempts on if transaction deadlocks occur on queries to the database that are wrapped in a transaction.", CategorySequenceId = 5)]
        public int DatabaseTransactionDeadlockRetryAttempts { get; set; }

        /// <summary>
        /// The number milliseconds to wait before retry attempts on transaction deadlocks.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The number milliseconds to wait before retry attempts on transaction deadlocks.", CategorySequenceId = 6)]
        public int DatabaseTransactionDeadlockRetryWaitPeriod { get; set; }
    }
}
