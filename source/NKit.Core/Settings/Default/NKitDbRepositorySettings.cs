namespace NKit.Settings.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Transactions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NKit.Data.DB;

    #endregion //Using Directives

    public class NKitDbRepositorySettings : NKitSettings
    {
        #region Properties

        /// <summary>
        /// The entity framework provider to use for the database i.e. the database type. Valid options are: SqlServer or Sqlite.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The entity framework provider to use for the database i.e. the database type. Valid options are: SqlServer or Sqlite.", CategorySequenceId = 0)]
        public NKitDbProviderName DatabaseProviderName { get; set; }

        /// <summary>
        /// Whether or not to automatically set the database provider and configure it using the settings in this file in the NKitDbContext, or whether you will configure it yourself in your DbContext that inherits from NKitDbContext.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "Whether or not to automatically set the database provider and configure it using the settings in this file in the NKitDbContext, or whether you will configure it yourself in your DbContext inherits from NKitDbContext.", CategorySequenceId = 1)]
        public bool AutoConfigureDatabaseProvider { get; set; }

        /// <summary>
        /// The assembly name (without the .dll fiel extension) that contains or will contain the Entity Framework migrations.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The assembly name (without the .dll fiel extension) that contains or will contain the Entity Framework migrations.", CategorySequenceId = 2)]
        public string EntityFrameworkMigrationsAssemblyName { get; set; }

        /// <summary>
        /// The connection string to the server database.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The connection string to the server database.", CategorySequenceId = 3)]
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// The timeout in seconds of the commands sent to the server database.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The timeout in seconds of the commands sent to the server database.", CategorySequenceId = 4)]
        public int DatabaseCommandTimeoutSeconds { get; set; }

        /// <summary>
        /// The name of the assembly containing the Entity Framework Models classes.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The name of the assembly containing the Entity Framework Models classes.", CategorySequenceId = 5)]
        public string EntityFrameworkModelsAssembly { get; set; }

        /// <summary>
        /// The namespace where the Entity Framework Models classes are located in the assembly.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The namespace where the Entity Framework Models classes are located in the assembly.", CategorySequenceId = 6)]
        public string EntityFrameworkModelsNamespace { get; set; }

        /// <summary>
        /// The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Scope Option to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 7)]
        public TransactionScopeOption DatabaseTransactionScopeOption { get; set; }

        /// <summary>
        /// The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Isolation Level to use on queries to the database that are wrapped in a transaction.", CategorySequenceId = 8)]
        public IsolationLevel DatabaseTransactionIsolationLevel { get; set; }

        /// <summary>
        /// The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The Transaction Timeout in seconds to use on on queries to the database that are wrapped in a transaction.", CategorySequenceId = 9)]
        public int DatabaseTransactionTimeoutSeconds { get; set; }

        /// <summary>
        /// The number of retry attempts if transaction deadlocks occur on queries to the database that are wrapped in a transaction.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The number of retry attempts on if transaction deadlocks occur on queries to the database that are wrapped in a transaction.", CategorySequenceId = 10)]
        public int DatabaseTransactionDeadlockRetryAttempts { get; set; }

        /// <summary>
        /// The number milliseconds to wait before retry attempts on transaction deadlocks.
        /// </summary>
        [NKitSettingInfo("Database", AutoFormatDisplayName = true, Description = "The number milliseconds to wait before retry attempts on transaction deadlocks.", CategorySequenceId = 11)]
        public int DatabaseTransactionDeadlockRetryWaitPeriod { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Reads the configuration section from the appsettings.json file and deserializes it to the specified Settings type.
        /// The Configuration object is created read from based on the appsettings.json. The appsettings.json file name is determined by reading the ASPNETCORE_ENVIRONMENT variable i.e. appsettings.{environment}.json or appsettings.json when the environment variable is not set.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitDbRepositorySettings GetSettings()
        {
            return GetSettings<NKitDbRepositorySettings>();
        }

        /// <summary>
        /// Reads the NKitDatabaseSettings configuration section from the appsettings.json file and deserializes to an instance of NKitDatabaseSettings.
        /// The section name in the appsettings.json file is depetermined based on the name of the Settings type e.g. DatabaseSettings.
        /// </summary>
        public static NKitDbRepositorySettings GetSettings(IConfiguration configuration)
        {
            return GetSettings<NKitDbRepositorySettings>(configuration);
        }

        /// <summary>
        /// Register Configurations from the appsettings.json which will be made available as IOptions to all services.
        /// </summary>
        /// <param name="services"></param>
        public static NKitDbRepositorySettings RegisterConfiguration(IConfiguration configuration, IServiceCollection services)
        {
            return RegisterConfiguration<NKitDbRepositorySettings>(configuration, services);
        }

        #endregion //Methods
    }
}
