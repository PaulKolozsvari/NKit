namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Transactions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public class DatabaseSettings : Settings
    {
        #region Properties

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
        /// The name of the assembly containing the Entity Framework Models classes.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The name of the assembly containing the Entity Framework Models classes.", CategorySequenceId = 2)]
        public string EntityFrameworkModelsAssembly { get; set; }

        /// <summary>
        /// The namespace where the Entity Framework Models classes are located in the assembly.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The namespace where the Entity Framework Models classes are located in the assembly.", CategorySequenceId = 3)]
        public string EntityFrameworkModelsNamespace { get; set; }

        /// <summary>
        /// The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 4)]
        public TransactionScopeOption DatabaseTransactionScopeOption { get; set; }

        /// <summary>
        /// The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 5)]
        public IsolationLevel DatabaseTransactionIsolationLevel { get; set; }

        /// <summary>
        /// The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.", CategorySequenceId = 6)]
        public int DatabaseTransactionTimeoutSeconds { get; set; }

        /// <summary>
        /// The number of retry attempts if transaction deadlocks occur on queries to the database that are wrapped in a transaction.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The number of retry attempts on if transaction deadlocks occur on queries to the database that are wrapped in a transaction.", CategorySequenceId = 7)]
        public int DatabaseTransactionDeadlockRetryAttempts { get; set; }

        /// <summary>
        /// The number milliseconds to wait before retry attempts on transaction deadlocks.
        /// </summary>
        [SettingInfo("Database", AutoFormatDisplayName = true, Description = "The number milliseconds to wait before retry attempts on transaction deadlocks.", CategorySequenceId = 8)]
        public int DatabaseTransactionDeadlockRetryWaitPeriod { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<DatabaseSettings>(configuration.GetSection(nameof(DatabaseSettings)));
        }

        #endregion //Methods
    }
}
