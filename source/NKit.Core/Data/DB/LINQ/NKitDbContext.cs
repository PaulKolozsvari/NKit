namespace NKit.Core.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Data.DB;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Settings.Default;
    using NKit.Utilities;

    #endregion //Using Directives

    public class NKitDbContext : DbContext
    {
        #region Constructors

        public NKitDbContext(DbContextOptions options, IOptions<NKitDbRepositorySettings> databaseOptions) : base(options)
        {
            DataValidator.ValidateObjectNotNull(databaseOptions, nameof(databaseOptions), nameof(NKitDbContext));
            DataValidator.ValidateStringNotEmptyOrNull(databaseOptions.Value.DatabaseConnectionString, nameof(databaseOptions.Value.DatabaseConnectionString), nameof(NKitDbContext));
            _dbRepositorySettings = databaseOptions.Value;
        }

        #endregion //Constructors

        #region Fields

        protected NKitDbRepositorySettings _dbRepositorySettings;

        #endregion //Fields

        #region Db Sets

        public virtual DbSet<NKitLogEntry> NKitLogEntry { get; set; }
        public virtual DbSet<NKitHttpExceptionResponse> NKitHttpExceptionResponse { get; set; }

        #endregion //Db Sets

        #region Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _dbRepositorySettings.AutoConfigureDatabaseProvider)
            {
                switch (_dbRepositorySettings.DatabaseProviderName)
                {
                    case NKitDbProviderName.SqlServer:
                        optionsBuilder.UseSqlServer(_dbRepositorySettings.DatabaseConnectionString,
                            sqlServerOptionsBuilder =>
                            {
                                sqlServerOptionsBuilder.MigrationsAssembly(_dbRepositorySettings.EntityFrameworkMigrationsAssemblyName);
                                sqlServerOptionsBuilder.CommandTimeout(_dbRepositorySettings.DatabaseCommandTimeoutSeconds);
                            });
                        break;
                    case NKitDbProviderName.Sqlite:
                        optionsBuilder.UseSqlite(_dbRepositorySettings.DatabaseConnectionString, sqliteOptions => //https://kontext.tech/column/dotnet_framework/275/sqlite-in-net-core-with-entity-framework-core
                        {
                            sqliteOptions.MigrationsAssembly(_dbRepositorySettings.EntityFrameworkMigrationsAssemblyName);
                            sqliteOptions.CommandTimeout(_dbRepositorySettings.DatabaseCommandTimeoutSeconds);
                        });
                        break;
                    default:
                        throw new ArgumentException($"Unsupported {nameof(NKitDbProviderName)} of {_dbRepositorySettings.DatabaseProviderName} set in {nameof(_dbRepositorySettings.DatabaseProviderName)}.");
                }
            }
            base.OnConfiguring(optionsBuilder);
        }

        #endregion //Methods
    }
}
