namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Data.SqlClient;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NKit.Data;
    using NKit.Settings.Default;
    using NKit.Data.DB.LINQ;
    using System.Linq.Expressions;
    using NKit.Data.DB.LINQ.Models;
    using Microsoft.Extensions.Logging;
    using NKit.Utilities;
    using NKit.Data.DB;
    using Dapper;
    using NKit.Web.Service;
    using System.Threading;
    using NKit.Utilities.Email;
    using NKit.Data.DB.SQLQuery;
    using NKit.Web.MVC.ViewModels;
    using System.Linq.Dynamic.Core;
    using NKit.Web.MVC.CsvModels;

    #endregion //Using Directives

    /// <summary>
    /// Class inherting from the Entity Framework DbContext providing dynamic CRUD operations, as well as logging to an NKitLogEntry table.
    /// </summary>
    public partial class NKitDbContext : DbContext
    {
        #region Constructors

        public NKitDbContext(
            DbContextOptions options,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitDbContextSettings> dbContextOptions,
            IOptions<NKitLoggingSettings> loggingOptions,
            IOptions<NKitEmailClientServiceSettings> emailClientServiceSettings) : base(options)
        {
            DataValidator.ValidateObjectNotNull(dbContextOptions, nameof(dbContextOptions), nameof(NKitDbContext));
            DataValidator.ValidateObjectNotNull(dbContextOptions.Value, nameof(dbContextOptions.Value), nameof(NKitDbContext));
            DataValidator.ValidateStringNotEmptyOrNull(dbContextOptions.Value.DatabaseConnectionString, nameof(dbContextOptions.Value.DatabaseConnectionString), nameof(NKitDbContext));
            DataValidator.ValidateIntegerNotNegative(dbContextOptions.Value.DatabaseTransactionDeadlockRetryAttempts, nameof(dbContextOptions.Value.DatabaseTransactionDeadlockRetryAttempts), nameof(NKitDbContext));
            DataValidator.ValidateIntegerNotNegative(dbContextOptions.Value.DatabaseTransactionDeadlockRetryWaitPeriod, nameof(dbContextOptions.Value.DatabaseTransactionDeadlockRetryWaitPeriod), nameof(NKitDbContext));

            _generalSettings = generalOptions.Value;
            _dbContextSettings = dbContextOptions.Value;
            _loggingSettings = loggingOptions.Value;
            _emailClientServiceSettings = emailClientServiceSettings.Value;

            base.Database.SetConnectionString(_dbContextSettings.DatabaseConnectionString);
            base.Database.SetCommandTimeout(_dbContextSettings.DatabaseCommandTimeoutSeconds);

            _transactionScopeOption = _dbContextSettings.DatabaseTransactionScopeOption;
            _transactionOptions = new TransactionOptions()
            {
                IsolationLevel = _dbContextSettings.DatabaseTransactionIsolationLevel,
                Timeout = new TimeSpan(0, 0, _dbContextSettings.DatabaseTransactionTimeoutSeconds)
            };
            _transactionDeadlockRetryAttempts = _dbContextSettings.DatabaseTransactionDeadlockRetryAttempts;
            _transactionDeadlockRetryWaitPeriod = _dbContextSettings.DatabaseTransactionDeadlockRetryWaitPeriod;
        }

        #endregion //Constructors

        #region Constants

        #region Sql Server Error Codes

        /// <summary>
        /// Error code on transaction deadlock exeptions.
        /// </summary>
        public const int SQL_SERVER_TRANSACTION_DEADLOCK_ERROR_CODE = 1205;

        #endregion //Sql Server Error Codes

        #region Sqlite Error Codes

        //SQLite EF Core Database Provider Limitations: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations

        /// <summary>
        /// The SQLITE_BUSY result code indicates that the database file could not be written (or in some cases read) because of 
        /// concurrent activity by some other database connection, usually a database connection in a separate process.
        /// https://www.sqlite.org/rescode.html
        /// </summary>
        public const int SQLITE_BUSY_ERROR_CODE = 5;

        /// <summary>
        /// The SQLITE_BUSY_RECOVERY error code is an extended error code for SQLITE_BUSY that indicates that an operation could not 
        /// continue because another process is busy recovering a WAL mode database file following a crash. The SQLITE_BUSY_RECOVERY 
        /// error code only occurs on WAL mode databases.
        /// https://www.sqlite.org/rescode.html
        /// </summary>
        public const int SQLITE_BUSY_RECOVERY_ERROR_CODE = 261;

        /// <summary>
        /// The SQLITE_BUSY_SNAPSHOT error code is an extended error code for SQLITE_BUSY that occurs on WAL mode databases when a 
        /// database connection tries to promote a read transaction into a write transaction but finds that another database connection 
        /// has already written to the database and thus invalidated prior reads
        /// https://www.sqlite.org/rescode.html
        /// </summary>
        public const int SQLITE_BUSY_SNAPSHOT_ERROR_CODE = 517;

        /// <summary>
        /// The SQLITE_BUSY_TIMEOUT error code indicates that a blocking Posix advisory file lock request in the VFS layer failed due 
        /// to a timeout. Blocking Posix advisory locks are only available as a proprietary SQLite extension and even then are only 
        /// supported if SQLite is compiled with the SQLITE_EANBLE_SETLK_TIMEOUT compile-time option.
        /// https://www.sqlite.org/rescode.html
        /// </summary>
        public const int SQLITE_BUSY_TIMEOUT_ERROR_CODE = 773;

        /// <summary>
        /// The SQLITE_LOCKED result code indicates that a write operation could not continue because of a conflict within the same 
        /// database connection or a conflict with a different database connection that uses a shared cache.
        /// https://www.sqlite.org/rescode.html
        /// </summary>
        public const int SQLITE_LOCKED_ERROR_CODE = 6;

        #endregion //Sqlite Error Codes

        #endregion //Constants

        #region Fields

        protected NKitGeneralSettings _generalSettings;
        protected NKitDbContextSettings _dbContextSettings;
        protected NKitLoggingSettings _loggingSettings;
        protected NKitEmailClientServiceSettings _emailClientServiceSettings;

        protected TransactionScopeOption _transactionScopeOption;
        protected TransactionOptions _transactionOptions;
        protected int _transactionDeadlockRetryAttempts;
        protected int _transactionDeadlockRetryWaitPeriod;

        #endregion //Fields

        #region Properties

        #region Settings Properties

        /// <summary>
        /// General settings provided to the DbContext through dependency injection from the appsettings.json file.
        /// </summary>
        public NKitGeneralSettings GeneralSettings
        {
            get { return _generalSettings; }
        }

        /// <summary>
        /// DbContext settings provided to the DbContext through dependency injection from the appsettings.json file.
        /// </summary>
        public NKitDbContextSettings DbContextSettings
        {
            get { return _dbContextSettings; }
        }

        /// <summary>
        /// Logging settings provided to the DbContext through dependency injection from the appsettings.json file.
        /// </summary>
        public NKitLoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
        }

        #endregion //Settings Properties

        #region Transaction Properties

        /// <summary>
        /// TransactionScopeOption to used for constructing a TransactionScope.
        /// </summary>
        public TransactionScopeOption TransactionScopeOption
        {
            get { return _transactionScopeOption; }
        }

        /// <summary>
        /// TransactionOptions to used for constructing a TransactionScope.
        /// </summary>
        public TransactionOptions TransactionOptions
        {
            get { return _transactionOptions; }
        }

        /// <summary>
        /// Number of times to retry an operation in case of a transaction deadlock.
        /// </summary>
        public int TransactionDeadlockRetryAttempts
        {
            get { return _transactionDeadlockRetryAttempts; }
        }

        /// <summary>
        /// Number of millisends to wait before each retry an operation causes a transaction deadlock.
        /// </summary>
        public int TransactionDeadlockRetryWaitPeriod
        {
            get { return _transactionDeadlockRetryWaitPeriod; }
        }

        #endregion //Transaction Properties

        #endregion //Properties

        #region Db Sets

        public virtual DbSet<NKitLogEntry> NKitLogEntry { get; set; }
        public virtual DbSet<NKitHttpExceptionResponse> NKitHttpExceptionResponse { get; set; }

        #endregion //Db Sets

        #region Methods

        #region DbContext Overriden Methods

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _dbContextSettings.AutoConfigureDatabaseProvider)
            {
                switch (_dbContextSettings.DatabaseProviderName)
                {
                    case NKitDbProviderName.SqlServer:
                        optionsBuilder.UseSqlServer(_dbContextSettings.DatabaseConnectionString,
                            sqlServerOptionsBuilder =>
                            {
                                sqlServerOptionsBuilder.MigrationsAssembly(_dbContextSettings.EntityFrameworkMigrationsAssemblyName);
                                sqlServerOptionsBuilder.CommandTimeout(_dbContextSettings.DatabaseCommandTimeoutSeconds);
                            });
                        break;
                    case NKitDbProviderName.Sqlite:
                        optionsBuilder.UseSqlite(_dbContextSettings.DatabaseConnectionString, sqliteOptions => //https://kontext.tech/column/dotnet_framework/275/sqlite-in-net-core-with-entity-framework-core
                        {
                            sqliteOptions.MigrationsAssembly(_dbContextSettings.EntityFrameworkMigrationsAssemblyName);
                            sqliteOptions.CommandTimeout(_dbContextSettings.DatabaseCommandTimeoutSeconds);
                        });
                        break;
                    default:
                        throw new ArgumentException($"Unsupported {nameof(NKitDbProviderName)} of {_dbContextSettings.DatabaseProviderName} set in {nameof(_dbContextSettings.DatabaseProviderName)}.");
                }
            }
            base.OnConfiguring(optionsBuilder);
        }

        #endregion //DbContext Overriden Methods

        #region Utility Methods

        protected bool IsSqlServerDeadlockException(SqlException ex)
        {
            return ex.Number == SQL_SERVER_TRANSACTION_DEADLOCK_ERROR_CODE;
        }

        protected bool IsSqliteLockException(SqliteException ex)
        {
            return ex.ErrorCode == SQLITE_BUSY_ERROR_CODE ||
                ex.ErrorCode == SQLITE_BUSY_RECOVERY_ERROR_CODE ||
                ex.ErrorCode == SQLITE_BUSY_SNAPSHOT_ERROR_CODE ||
                ex.ErrorCode == SQLITE_BUSY_TIMEOUT_ERROR_CODE ||
                ex.ErrorCode == SQLITE_LOCKED_ERROR_CODE;
        }

        /// <summary>
        /// Gets an IQueryable Set for a dynamic Type.
        /// https://stackoverflow.com/questions/21533506/find-a-specified-generic-dbset-in-a-dbcontext-dynamically-when-i-have-an-entity
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public virtual IQueryable Set(Type T)
        {
            MethodInfo method = typeof(DbContext).GetMethods().Where(p => p.Name == "Set" && p.ContainsGenericParameters).FirstOrDefault(); // Get the generic type definition 
            method = method.MakeGenericMethod(T); // Build a method with the specific type argument you're interested in
            return method.Invoke(this, null) as IQueryable;
        }

        #endregion //Utility Methods

        #region Crud Methods

        /// <summary>
        /// Executes raw SQL queries on the underlying DbContext.
        /// https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core
        /// </summary>
        /// <typeparam name="T">The entity type to convert the data to.</typeparam>
        /// <param name="query">The raw SQL query.</param>
        /// <param name="map">Mappings between the data received from the DbDataReader and the specified entity type.</param>
        /// <returns></returns>
        public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map)
        {
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                Database.OpenConnection();
                using (DbDataReader dbDataReader = command.ExecuteReader())
                {
                    var entities = new List<T>();
                    while (dbDataReader.Read())
                    {
                        entities.Add(map(dbDataReader));
                    }
                    return entities;
                }
            }
        }

        /// <summary>
        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
        /// </summary>
        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
        /// <param name="entity">The the entity to save.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
        public virtual List<LinqFunnelChangeResult> Save<E>(E entity, object entityIdentifier) where E : class
        {
            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(typeof(E), entity, entityIdentifier);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
        /// </summary>
        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
        /// <param name="entity">The the entity to save.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> SaveAsync<E>(E entity, object entityIdentifier) where E : class
        {
            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(typeof(E), entity, entityIdentifier);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
        /// </summary>
        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The the entity to save.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
        protected virtual List<LinqFunnelChangeResult> SaveWithoutSavingToDb(Type entityType, object entity, object entityIdentifier)
        {
            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
            bool containsIdentityColumn = IsIdentityColumn(surrogateKey);
            object original = null;
            object surrogateKeyValue = surrogateKey.GetValue(entity, null);
            original = GetEntityBySurrogateKey(entityType, surrogateKeyValue);
            List<LinqFunnelChangeResult> result = null;
            if (original == null)
            {
                Add(entity);
                result = new List<LinqFunnelChangeResult>();
                result.Add(new LinqFunnelChangeResult()
                {
                    SurrogateKey = surrogateKeyValue,
                    EntityIdentifier = entityIdentifier,
                    Function = "INSERT",
                    DateChanged = DateTime.Now,
                    EntityChanged = entityType.Name,
                    FieldChanged = surrogateKey.Name,
                    NewValue = surrogateKey.GetValue(entity, null)
                });
            }
            else
            {
                result = UpdateOriginalEntity(entityType, original, entity, surrogateKeyValue, entityIdentifier);
            }
            return result;
        }

        /// <summary>
        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
        /// </summary>
        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The the entity to save.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
        public virtual List<LinqFunnelChangeResult> Save(Type entityType, object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(entityType, entity, entityIdentifier);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Saves (updates/inserts) an entity to the table corrseponding to the entity type.
        /// If the entity's surrogate key is an identity entity will be inserted and not updated.
        /// </summary>
        /// <typeparam name="E">The type of the entity i.e. which table it will be saved to.</typeparam>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The the entity to save.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results i.e. what entities where updated</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> SaveAsync(Type entityType, object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = SaveWithoutSavingToDb(entityType, entity, entityIdentifier);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Inserts an entity to the database context without saving to the database.
        /// </summary>
        /// <param name="entity">The entity being inserted.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>A list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Insert<E>(E entity, object entityIdentifier) where E : class
        {
            return Insert(typeof(E), entity, entityIdentifier);
        }

        /// <summary>
        /// Inserts an entity to the database context without saving to the database.
        /// </summary>
        /// <param name="entity">The entity being inserted.</param>
        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
        /// <returns>A list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> InsertAsync<E>(E entity, object entityIdentifier) where E : class
        {
            Task<List<LinqFunnelChangeResult>> taskResult = InsertAsync(typeof(E), entity, entityIdentifier);
            await taskResult;
            return taskResult.Result;
        }

        /// <summary>
        /// Inserts an entity to the database context without saving to the database.
        /// </summary>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The entity being inserted.</param>
        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
        /// <returns>A list of change results.</returns>
        protected virtual List<LinqFunnelChangeResult> InsertWithoutSaveToDb(Type entityType, object entity, object entityIdentifier)
        {
            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
            bool containsIdentityColumn = IsIdentityColumn(surrogateKey);
            object surrogateKeyValue = surrogateKey.GetValue(entity, null);

            Add(entity);
            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            result.Add(new LinqFunnelChangeResult()
            {
                SurrogateKey = surrogateKeyValue,
                EntityIdentifier = entityIdentifier,
                Function = "INSERT",
                DateChanged = DateTime.Now,
                EntityChanged = entityType.Name,
                FieldChanged = surrogateKey.Name,
                NewValue = surrogateKey.GetValue(entity, null)
            });
            return result;
        }

        /// <summary>
        /// Inserts an entity to the database context and calls save on the database.
        /// </summary>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The entity being inserted.</param>
        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
        /// <returns>A list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Insert(Type entityType, object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = InsertWithoutSaveToDb(entityType, entity, entityIdentifier);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Inserts an entity to the database context and calls async save on the database.
        /// </summary>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="entity">The entity being inserted.</param>
        /// <param name="entityIdentifier">The value uniquely identifiying the entity</param>
        /// <returns>A list of change results.</returns>
        public async Task<List<LinqFunnelChangeResult>> InsertAsync(Type entityType, object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = InsertWithoutSaveToDb(entityType, entity, entityIdentifier);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
        public virtual PropertyInfo GetEntitySurrogateKey<E>()
        {
            return EntityReader.GetLinqToSqlEntitySurrogateKey<E>();
        }

        /// <summary>
        /// Determines the primary key of an entity type. The first primary key found on the entity type i.e.
        /// the assumption is made that the entity type only has one primary key, which is the surrogate key.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose surrogate key needs to be determined.</typeparam>
        /// <returns>Retruns the PropertyInfo corresponding to the column which is the surrogate key for the specified entity type.</returns>
        public virtual PropertyInfo GetEntitySurrogateKey(Type entityType)
        {
            return EntityReader.GetLinqToSqlEntitySurrogateKey(entityType);
        }

        /// <summary>
        /// Determines whether a property is an identity column.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Returns true if the property is an identity column.</returns>
        public virtual bool IsIdentityColumn(PropertyInfo p)
        {
            return EntityReader.IsLinqToSqlEntityPropertyIdentityColumn(p);
        }

        /// <summary>
        /// Updates the original entity with values of the latest entities. In other words, it copies the
        /// column values of the latest entity to the original entity.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose original record will be updated.</typeparam>
        /// <param name="original">The original entity retrieved from the database.</param>
        /// <param name="latest">The latest entity received from the client.</param>
        /// <param name="surrogateKeyValue">The value of the entity's surrogate key uniquely identifying the entity.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results containing all the fields that were changed and their original and new values.</returns>
        protected virtual List<LinqFunnelChangeResult> UpdateOriginalEntity<E>(E original, E latest, object surrogateKeyValue, object entityIdentifier) where E : class
        {
            return UpdateOriginalEntity(typeof(E), original, latest, surrogateKeyValue, entityIdentifier);
        }

        /// <summary>
        /// Updates the original entity with values of the latest entities. In other words, it copies the
        /// column values of the latest entity to the original entity.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table whose original record will be updated.</typeparam>
        /// <param name="entityType">The Type of entity being inserted.</param>
        /// <param name="original">The original entity retrieved from the database.</param>
        /// <param name="latest">The latest entity received from the client.</param>
        /// <param name="surrogateKeyValue">The value of the entity's surrogate key uniquely identifying the entity.</param>
        /// <param name="entityIdentifier">The value unique identifiying the entity</param>
        /// <returns>Returns a list of change results containing all the fields that were changed and their original and new values.</returns>
        protected virtual List<LinqFunnelChangeResult> UpdateOriginalEntity(
            Type entityType,
            object original,
            object latest,
            object surrogateKeyValue,
            object entityIdentifier)
        {
            if (entityType != original.GetType())
            {
                throw new ArgumentException(string.Format(
                    "Entity Type of {0} does not match the original entity's type of {1}.",
                    entityType.FullName,
                    original.GetType().FullName));
            }
            if (original.GetType() != latest.GetType())
            {
                throw new ArgumentException(string.Format(
                    "Cannot update original of type {0} because latest entity is of type {1}.",
                    original.GetType().FullName,
                    latest.GetType().FullName));
            }
            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            PropertyInfo[] properties = entityType.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                KeyAttribute keyAttribute = p.GetCustomAttribute<KeyAttribute>();
                ForeignKeyAttribute foreignKeyAttribute = p.GetCustomAttribute<ForeignKeyAttribute>();
                if (keyAttribute != null || foreignKeyAttribute != null)
                {
                    continue; //Property is either a key, or foreign key and therefore should not be saved/updated.
                }
                object originalValue = p.GetValue(original, null);
                object latestValue = p.GetValue(latest, null);
                if (object.Equals(originalValue, latestValue))
                {
                    continue;
                }
                result.Add(new LinqFunnelChangeResult()
                {
                    SurrogateKey = surrogateKeyValue,
                    EntityIdentifier = entityIdentifier,
                    Function = "UPDATE",
                    DateChanged = DateTime.Now,
                    EntityChanged = entityType.Name,
                    FieldChanged = p.Name,
                    OriginalValue = originalValue,
                    NewValue = latestValue,
                });
                p.SetValue(original, latestValue, null);
            }
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Delete<E>(E entity, object entityIdentifier) where E : class
        {
            List<LinqFunnelChangeResult> result = this.Delete<E, object>(entity, entityIdentifier, false);
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <returns>Returns a list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync<E>(E entity, object entityIdentifier) where E : class
        {
            Task<List<LinqFunnelChangeResult>> taskResult = this.DeleteAsync<E, object>(entity, entityIdentifier, false);
            await taskResult;
            return taskResult.Result;
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Delete(object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = this.DeleteWithoutSavingToDb(entity, entityIdentifier, false, null);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
        /// <param name="entity">The entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <returns>Returns a list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync(object entity, object entityIdentifier)
        {
            List<LinqFunnelChangeResult> result = this.DeleteWithoutSavingToDb(entity, entityIdentifier, false, null);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Delete<E, T>(E entity, object entityIdentifier, bool createTombstone)
            where E : class
            where T : class
        {
            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, typeof(T));
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <returns>Returns a list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync<E, T>(E entity, object entityIdentifier, bool createTombstone)
            where E : class
            where T : class
        {
            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, typeof(T));
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
        /// <returns>Returns a list of change results.</returns>
        protected virtual List<LinqFunnelChangeResult> DeleteWithoutSavingToDb(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
        {
            Type entityType = entity.GetType();
            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
            object surrogateKeyValue = surrogateKey.GetValue(entity, null);
            object original = GetEntityBySurrogateKey(entityType, surrogateKeyValue, false);
            if (original == null)
            {
                throw new Exception(
                    string.Format("Could not find entity with key {0} and value {1} to delete.",
                    GetEntitySurrogateKey(entityType).Name,
                    GetEntitySurrogateKey(entityType).GetValue(entity, null)));
            }
            if (createTombstone)
            {
                object tombstone = Activator.CreateInstance(tombstoneType);
                CopyToTombstoneEntity(entityType, tombstoneType, original, tombstone);
                object existingTombstone = GetEntityBySurrogateKey(tombstoneType, surrogateKey.GetValue(entity, null), false);
                if (existingTombstone != null)
                {
                    Remove(existingTombstone);
                }
                Add(tombstone);
            }
            Remove(original);

            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            result.Add(new LinqFunnelChangeResult()
            {
                SurrogateKey = surrogateKeyValue,
                EntityIdentifier = entityIdentifier,
                Function = "DELETE",
                DateChanged = DateTime.Now,
                EntityChanged = entityType.Name,
                FieldChanged = surrogateKey.Name,
            }); ;
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> Delete(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
        {
            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, tombstoneType);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="tombstoneType">The tombstone entity type i.e. the table where an tombstone will be created.</param>
        /// <returns>Returns a list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAsync(object entity, object entityIdentifier, bool createTombstone, Type tombstoneType)
        {
            List<LinqFunnelChangeResult> result = DeleteWithoutSavingToDb(entity, entityIdentifier, createTombstone, tombstoneType);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table it will be deleted from.</typeparam>
        /// <param name="surrogateKeyValue">The entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey<E>(object surrogateKeyValue, object entityIdentifier) where E : class
        {
            return DeleteBySurrogateKey<E, object>(surrogateKeyValue, entityIdentifier, false);
        }

        /// <summary>
        /// Deletes an entity from the table corresponding to the entity type.
        /// </summary>
        /// <param name="surrogateKeyValue">The surrogate key value uniquely identifying the entity to be deleted.</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="entityType">The type of the entity to be deleted.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey(object surrogateKeyValue, object entityIdentifier, Type entityType)
        {
            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, false, entityType, null);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey<E, T>(object surrogateKeyValue, object entityIdentifier, bool createTombstone)
            where E : class
            where T : class
        {
            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, typeof(E), typeof(T));
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="entityType">The type of the entity to be deleted.</param>
        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
        /// <returns>Returns a list of change results.</returns>
        protected virtual List<LinqFunnelChangeResult> DeleteBySurrogateKeyWithoutSavingToDb(
            object surrogateKeyValue,
            object entityIdentifier,
            bool createTombstone,
            Type entityType,
            Type tombstoneType)
        {
            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
            object original = GetEntityBySurrogateKey(entityType, surrogateKeyValue, false);
            if (original == null)
            {
                throw new Exception(
                    string.Format("Could not find entity with key {0} and value {1} to delete.",
                    GetEntitySurrogateKey(entityType).Name,
                    surrogateKeyValue));
            }
            if (createTombstone)
            {
                object tombstone = Activator.CreateInstance(tombstoneType);
                CopyToTombstoneEntity(original, tombstone);
                object existingTombstone = GetEntityBySurrogateKey(tombstoneType, surrogateKeyValue, false);
                if (existingTombstone != null)
                {
                    Remove(existingTombstone);
                }
                Add(tombstone);
            }
            Add(original);

            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            result.Add(new LinqFunnelChangeResult()
            {
                SurrogateKey = surrogateKeyValue,
                EntityIdentifier = entityIdentifier,
                Function = "DELETE",
                DateChanged = DateTime.Now,
                EntityChanged = entityType.Name,
                FieldChanged = surrogateKey.Name,
            });
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="entityType">The type of the entity to be deleted.</param>
        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
        /// <returns>Returns a list of change results.</returns>
        public virtual List<LinqFunnelChangeResult> DeleteBySurrogateKey(
            object surrogateKeyValue,
            object entityIdentifier,
            bool createTombstone,
            Type entityType,
            Type tombstoneType)
        {
            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, entityType, tombstoneType);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes an entity from the table with the specified surrogate key correspoding to the entity type (E) and creates
        /// a tombstone in the table correspoging to the tombstone entity type (T) if the 
        /// createTombstone flag is set to true.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <typeparam name="T">The tombstone entity type i.e. the table where an tombstone will be created.</typeparam>
        /// <param name="surrogateKeyValue">The surrogate key of the entity to be deleted</param>
        /// <param name="entityIdentifier">Unique value identifying the entity.</param>
        /// <param name="createTombstone">Indicates whether a tombstone should be created.</param>
        /// <param name="entityType">The type of the entity to be deleted.</param>
        /// <param name="tombstoneType">The type of the entity serving as the tombstone where the entity will be archited i.e. the tombstone table.</param>
        /// <returns>Returns a list of change results.</returns>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteBySurrogateKeyAsync(
            object surrogateKeyValue,
            object entityIdentifier,
            bool createTombstone,
            Type entityType,
            Type tombstoneType)
        {
            List<LinqFunnelChangeResult> result = DeleteBySurrogateKeyWithoutSavingToDb(surrogateKeyValue, entityIdentifier, createTombstone, entityType, tombstoneType);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        protected virtual List<LinqFunnelChangeResult> DeleteOlderThanWithoutSavingToDb<E>(string dateFieldName, TimeSpan time) where E : class
        {
            DateTime threshold = DateTime.Now.Subtract(time);
            DbSet<E> table = Set<E>();
            Type entityType = typeof(E);
            PropertyInfo dateField = entityType.GetProperty(dateFieldName);
            if ((dateField == null) || (dateField.PropertyType != typeof(DateTime)))
            {
                throw new Exception(
                    string.Format("Entity {0} does not contain the DateTime field with the name {1}",
                    entityType.Name,
                    dateFieldName));
            }
            List<object> toDelete = new List<object>();
            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            foreach (E entity in table)
            {
                if ((DateTime)dateField.GetValue(entity, null) < threshold)
                {
                    table.Remove(entity);
                    PropertyInfo surrogateKey = GetEntitySurrogateKey<E>();
                    object surrogateKeyValue = surrogateKey.GetValue(entity, null);
                    result.Add(new LinqFunnelChangeResult()
                    {
                        SurrogateKey = surrogateKeyValue,
                        EntityIdentifier = null,
                        Function = "DELETE",
                        DateChanged = DateTime.Now,
                        EntityChanged = entityType.Name,
                        FieldChanged = surrogateKey.Name,
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        public virtual List<LinqFunnelChangeResult> DeleteOlderThan<E>(string dateFieldName, TimeSpan time) where E : class
        {
            List<LinqFunnelChangeResult> result = this.DeleteOlderThan<E>(dateFieldName, time);
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <typeparam name="E">The entity type of the entity which will be deleted i.e. the table from where it will be deleted.</typeparam>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteOlderThanAsync<E>(string dateFieldName, TimeSpan time) where E : class
        {
            List<LinqFunnelChangeResult> result = this.DeleteOlderThanWithoutSavingToDb<E>(dateFieldName, time);
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <param name="entityType">The Type of the entity to be deleted.</param>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        protected virtual void DeleteOlderThanWithoutSavingToDb(Type entityType, string dateFieldName, TimeSpan time)
        {
            DateTime threshold = DateTime.Now.Subtract(time);
            IQueryable table = Set(entityType);
            PropertyInfo dateField = entityType.GetProperty(dateFieldName);
            if ((dateField == null) || (dateField.PropertyType != typeof(DateTime)))
            {
                throw new Exception(
                    string.Format("Entity {0} does not contain the DateTime field with the name {1}",
                    entityType.Name,
                    dateFieldName));
            }
            foreach (object e in table)
            {
                if ((DateTime)dateField.GetValue(e, null) < threshold)
                {
                    Remove(e);
                }
            }
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <param name="entityType">The Type of the entity to be deleted.</param>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        public virtual void DeleteOlderThan(Type entityType, string dateFieldName, TimeSpan time)
        {
            DeleteOlderThanWithoutSavingToDb(entityType, dateFieldName, time);
            SaveChanges();
        }

        /// <summary>
        /// Deletes all the entities in a given table older than the time specified.
        /// </summary>
        /// <param name="entityType">The Type of the entity to be deleted.</param>
        /// <param name="dateFieldName">The field name on the entity which must a date time field .</param>
        /// <param name="time">The time relative to the current time i.e. current time subracted by the this time sets the threshhold for entities deleted.</param>
        public async virtual void DeleteOlderThanAsync(Type entityType, string dateFieldName, TimeSpan time)
        {
            DeleteOlderThanWithoutSavingToDb(entityType, dateFieldName, time);
            await SaveChangesAsync();
        }

        /// <summary>
        /// Deletes all entities from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
        protected virtual List<LinqFunnelChangeResult> DeleteAllWithoutSavingToDb<E>() where E : class
        {
            DbSet<E> table = Set<E>();
            List<E> entities = new List<E>();
            foreach (E e in table)
            {
                entities.Add(e);
            }
            table.RemoveRange(entities);
            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            result.Add(new LinqFunnelChangeResult()
            {
                Function = "DELETE ALL",
                DateChanged = DateTime.Now,
                EntityChanged = typeof(E).Name,
            });
            return result;
        }

        /// <summary>
        /// Deletes all entities from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
        public virtual List<LinqFunnelChangeResult> DeleteAll<E>() where E : class
        {
            List<LinqFunnelChangeResult> result = DeleteAllWithoutSavingToDb<E>();
            SaveChanges();
            return result;
        }

        /// <summary>
        /// Deletes all entities from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
        public async virtual Task<List<LinqFunnelChangeResult>> DeleteAllAsync<E>() where E : class
        {
            List<LinqFunnelChangeResult> result = DeleteAllWithoutSavingToDb<E>();
            await SaveChangesAsync();
            return result;
        }

        /// <summary>
        /// Deletes all entities from the table corresponding to the entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. of the table whose records (entities) will be deleted.</typeparam>
        public virtual List<LinqFunnelChangeResult> DeleteAll(Type entityType)
        {
            IQueryable table = Set(entityType);
            List<object> entities = new List<object>();
            foreach (object e in table)
            {
                entities.Add(e);
            }
            RemoveRange(entities);
            SaveChanges();

            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            result.Add(new LinqFunnelChangeResult()
            {
                Function = "DELETE ALL",
                DateChanged = DateTime.Now,
                EntityChanged = entityType.Name,
            });
            return result;
        }

        /// <summary>
        /// Copies all the values from the original entity to a tombstone entity. If the fields/columns on the two entities
        /// do not match an exception will be thrown.
        /// </summary>
        /// <param name="original">The original entity retrieved from the database.</param>
        /// <param name="tombstone">The tombstone entity containing the same fields/columns as the original entity.</param>
        public virtual void CopyToTombstoneEntity<E, T>(E original, T tombstone)
            where E : class
            where T : class
        {
            CopyToTombstoneEntity(typeof(E), typeof(T), original, tombstone);
        }

        /// <summary>
        /// Copies all the values from the original entity to a tombstone entity. If the fields/columns on the two entities
        /// do not match an exception will be thrown.
        /// </summary>
        /// <param name="entityType">The Type of the original entity to be copied to the tombstone entity.</param>
        /// <param name="tombstoneType">The Type of the tombstone entity containing the same fields/columns as the original entity.</param>
        /// <param name="original">The original entity retrieved from the database.</param>
        /// <param name="tombstone">The tombstone entity containing the same fields/columns as the original entity.</param>
        public virtual void CopyToTombstoneEntity(Type entityType, Type tombstoneType, object original, object tombstone)
        {
            PropertyInfo[] properties = entityType.GetProperties();
            foreach (PropertyInfo p in properties)
            {
                object[] attributes = p.GetCustomAttributes(typeof(ColumnAttribute), false);
                ColumnAttribute columnAttribute = attributes.Length < 1 ? null : (ColumnAttribute)attributes[0];
                if (columnAttribute == null)
                {
                    continue;
                }
                PropertyInfo tombstoneProperty = tombstoneType.GetProperty(p.Name);
                if (tombstoneProperty == null)
                {
                    throw new NullReferenceException(
                        string.Format(
                        "Could not find property on tombstone entity with the name {0}.",
                        p.Name));
                }
                object originalValue = p.GetValue(original, null);
                tombstoneProperty.SetValue(tombstone, originalValue, null);
            }
        }

        /// <summary>
        /// Queries for and returns the first entity filtered by the specified expression.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
        /// <param name="expression">The expression to use to filter by.</param>
        /// <returns>Returns the first entity filtered by the specified expression.</returns>
        public virtual E GetFirstEntity<E>(Expression<Func<E, bool>> expression) where E : class
        {
            return GetFirstEntity<E>(expression, false);
        }

        /// <summary>
        /// Queries for and returns the first entity filtered by the specified expression.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
        /// <param name="expression">The expression to use to filter by.</param>
        /// <param name="throwExceptionOnNotFound">Whether or not to to throw an exception if the result is null.</param>
        /// <returns>Returns the first entity filtered by the specified expression.</returns>
        public virtual E GetFirstEntity<E>(Expression<Func<E, bool>> expression, bool throwExceptionOnNotFound) where E : class
        {
            E result = Set<E>().Where(expression.Compile()).FirstOrDefault();
            if (result == null && throwExceptionOnNotFound)
            {
                throw new Exception(string.Format("Could not find {0} for expression {1}'.",
                    typeof(E).Name,
                    expression.ToString()));
            }
            return result;
        }

        /// <summary>
        /// Queries for and returns a list of entities filtered by the specified expression.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
        /// <param name="expression">The expression to use to filter by.</param>
        /// <returns>Returns a list of entities filtered by the specified expression.</returns>
        public virtual List<E> GetEntities<E>(Expression<Func<E, bool>> expression) where E : class
        {
            return Set<E>().Where(expression.Compile()).ToList();
        }

        /// <summary>
        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
        /// on the surrogate key of the entity.
        /// </summary>
        /// <param name="entityType">The entity type i.e. which table the entity will be queried from.</param>
        /// <param name="keyValue">The value of the surrogate to search by.</param>
        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
        public virtual object GetEntityBySurrogateKey(Type entityType, object keyValue)
        {
            return GetEntityBySurrogateKey(entityType, keyValue, false);
        }

        /// <summary>
        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
        /// on the surrogate key of the entity.
        /// </summary>
        /// <param name="entityType">The entity type i.e. which table the entity will be queried from.</param>
        /// <param name="keyValue">The value of the surrogate to search by.</param>
        /// <param name="throwExceptionOnNotFound">Whether or not to throw an exception if the result is null.</param>
        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
        public virtual object GetEntityBySurrogateKey(Type entityType, object keyValue, bool throwExceptionOnNotFound)
        {
            MethodInfo methodDefinition = GetType().GetMethod("GetEntityBySurrogateKey", new Type[] { typeof(object), typeof(bool) }); //https://stackoverflow.com/questions/266115/pass-an-instantiated-system-type-as-a-type-parameter-for-a-generic-class
            MethodInfo method = methodDefinition.MakeGenericMethod(entityType);
            return method.Invoke(this, new object[] { keyValue, throwExceptionOnNotFound });
        }

        /// <summary>
        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
        /// on the surrogate key of the entity.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
        /// <param name="keyValue">The value of the surrogate to search by.</param>
        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
        public virtual E GetEntityBySurrogateKey<E>(object keyValue) where E : class
        {
            return GetEntityBySurrogateKey<E>(keyValue, false);
        }

        /// <summary>
        /// Queries for and returns an entity from the table corresponding to the entity type. The query is performed
        /// on the surrogate key of the entity.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. which table the entity will be queried from.</typeparam>
        /// <param name="keyValue">The value of the surrogate to search by.</param>
        /// <param name="throwExceptionOnNotFound">Whether or not to throw an exception if the result is null.</param>
        /// <returns>Returns an entity with the specified type and surrogate key. Returns null if one is not found.</returns>
        public virtual E GetEntityBySurrogateKey<E>(object keyValue, bool throwExceptionOnNotFound) where E : class
        {
            Type entityType = typeof(E);
            PropertyInfo surrogateKey = GetEntitySurrogateKey(entityType);
            object keyValueConverted = EntityReader.ConvertValueTypeTo(keyValue, surrogateKey.PropertyType);
            ParameterExpression e = Expression.Parameter(entityType, "e");
            MemberExpression memberExpression = Expression.MakeMemberAccess(e, surrogateKey); //Name of surrogate key : left hand side of the expression.
            ConstantExpression constantExpression = Expression.Constant(keyValueConverted, surrogateKey.PropertyType); //Value of the surrogate key : right hand side of the expression.
            BinaryExpression binaryExpression = Expression.Equal(memberExpression, constantExpression);
            Expression<Func<E, bool>> lambdaExpression = Expression.Lambda<Func<E, bool>>(binaryExpression, e);
            return Set<E>().SingleOrDefault(lambdaExpression);
        }

        /// <summary>
        /// Queries for entities in a table corresponding to entity type. The query is performed on the column/field
        /// specified with the specified field value.
        /// </summary>
        /// <param name="entityType">The entity type i.e. the table from which the entities will be returned.</param>
        /// <param name="fieldName">The name of the field/column on which the query will be performed.</param>
        /// <param name="fieldValue">The value of the field which will be used for the query.</param>
        /// <returns>Returns a list of entities of the specified type with the specified field/column and field value.</returns>
        public virtual List<object> GetEntitiesByField(Type entityType, string fieldName, object fieldValue)
        {
            MethodInfo methodDefinition = GetType().GetMethod("GetEntitiesByField", new Type[] { typeof(string), typeof(object), typeof(bool) }); //https://stackoverflow.com/questions/266115/pass-an-instantiated-system-type-as-a-type-parameter-for-a-generic-class
            MethodInfo method = methodDefinition.MakeGenericMethod(entityType);
            object queryResult = method.Invoke(this, new object[] { fieldName, fieldValue });
            System.Collections.IList genericList = (System.Collections.IList)queryResult;
            List<object> result = new List<object>();
            foreach (var e in genericList)
            {
                result.Add(e);
            }
            return result;
        }

        /// <summary>
        /// Queries for entities in a table corresponding to entity type. The query is performed on the column/field
        /// specified with the specified field value.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
        /// <param name="fieldName">The name of the field/column on which the query will be performed.</param>
        /// <param name="fieldValue">The value of the field which will be used for the query.</param>
        /// <returns>Returns a list of entities of the specified type with the specified field/column and field value.</returns>
        public virtual List<E> GetEntitiesByField<E>(string fieldName, object fieldValue) where E : class
        {
            Type entityType = typeof(E);
            PropertyInfo field = entityType.GetProperty(fieldName);
            object keyValueConverted = EntityReader.ConvertValueTypeTo(fieldValue, field.PropertyType);
            ParameterExpression e = Expression.Parameter(entityType, "e");
            MemberExpression memberExpression = Expression.MakeMemberAccess(e, field); //Name of surrogate key : left hand side of the expression.
            ConstantExpression constantExpression = Expression.Constant(keyValueConverted, field.PropertyType); //Value of the surrogate key : right hand side of the expression.
            BinaryExpression binaryExpression = Expression.Equal(memberExpression, constantExpression);
            Expression<Func<E, bool>> lambdaExpression = Expression.Lambda<Func<E, bool>>(binaryExpression, e);
            return Set<E>().Where(lambdaExpression).ToList();
        }

        /// <summary>
        /// Queries for all the entities in a table corresponging to the specfied entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
        /// <returns>Returns all the entities of the specified type.</returns>
        public virtual List<E> GetAllEntities<E>() where E : class
        {
            return Set<E>().ToList<E>();
        }

        /// <summary>
        /// Queries for all the entities in a table corresponging to the specfied entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be returned.</typeparam>
        /// <returns>Returns all the entities of the specified type.</returns>
        public virtual List<object> GetAllEntities(Type entityType)
        {
            List<object> result = new List<object>();
            IQueryable table = Set(entityType);
            foreach (object e in table)
            {
                result.Add(e);
            }
            return result;
        }

        /// <summary>
        /// Returns the total count of an entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
        /// <returns>Returns the total count of an entity type.</returns>
        public virtual int GetTotalCount<E>() where E : class
        {
            return Set<E>().Count();
        }

        /// <summary>
        /// Runs a raw SQL query to the database to count the number of records in a specified database table.
        /// </summary>
        /// <param name="entityType">The entity type i.e. the table from which the entities will be counted.</param>
        /// <returns>Returns the total count of an entity type.</returns>
        public virtual int GetTotalCount(Type entityType)
        {
            int result = 0;
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(*) FROM {entityType.Name}";
                command.CommandType = CommandType.Text;
                Database.OpenConnection();
                using (DbDataReader dbDataReader = command.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        result = Convert.ToInt32(dbDataReader[0]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the total count of an entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
        /// <returns>Returns the total count of an entity type.</returns>
        public virtual long GetTotalCountLong<E>() where E : class
        {
            return Set<E>().LongCount();
        }

        /// <summary>
        /// Runs a raw SQL query to the database to count the number of records in a specified database table.
        /// </summary>
        /// <param name="entityType">The entity type i.e. the table from which the entities will be counted.</param>
        /// <returns>Returns the total count of an entity type.</returns>
        public virtual long GetTotalCountLong(Type entityType)
        {
            long result = 0;
            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(*) FROM {entityType.Name}";
                command.CommandType = CommandType.Text;
                Database.OpenConnection();
                using (DbDataReader dbDataReader = command.ExecuteReader())
                {
                    while (dbDataReader.Read())
                    {
                        result = Convert.ToInt64(dbDataReader[0]);
                    }
                }
            }
            return result;
        }

        public virtual bool SqlTableExists(string tableName)
        {
            switch (_dbContextSettings.DatabaseProviderName)
            {
                case NKitDbProviderName.SqlServer:
                    return SqlServerTableExists(tableName);
                case NKitDbProviderName.Sqlite:
                    return SqliteTableExists(tableName);
                default:
                    throw new ArgumentException($"Unsupported {nameof(NKitDbProviderName)} of {_dbContextSettings.DatabaseProviderName} set in {nameof(_dbContextSettings.DatabaseProviderName)}.");
            }
        }

        public virtual bool SqlServerTableExists(string tableName)
        {
            string sqlQuery = $"SELECT COUNT(*) as Count FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
            DbConnection connection = Database.GetDbConnection();
            if (connection == null)
            {
                return false;
            }
            int count = connection.Query<int>(sqlQuery).FirstOrDefault(); // Query method extension provided by Dapper library.
            return (count > 0);
        }

        public virtual bool SqliteTableExists(string tableName)
        {
            string sqlQuery = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";
            DbConnection connection = Database.GetDbConnection();
            if (connection == null)
            {
                return false;
            }
            string queryResult = connection.Query<string>(sqlQuery).FirstOrDefault(); // Query method extension provided by Dapper library.
            return !string.IsNullOrEmpty(queryResult) && queryResult.Equals(tableName);
        }

        /// <summary>
        /// Returns the total count of an entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
        /// <returns>Returns the total count of an entity type.</returns>
        //public virtual int GetTotalCount(Type entityType)
        //{
        //    return GetAllEntities(entityType, false).Count;
        //}

        /// <summary>
        /// Returns the total count of an entity type.
        /// </summary>
        /// <typeparam name="E">The entity type i.e. the table from which the entities will be counted.</typeparam>
        /// <returns>Returns the total count of an entity type.</returns>
        //public virtual long GetTotalCountLong(Type entityType)
        //{
        //    return GetAllEntities(entityType, false).LongCount();
        //}

        /// <summary>
        /// Disposes the underlying Entity Framework DbContext.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion //Crud Methods

        #region Transactional Crud Methods

        #region Core Methods

        /// <summary>
        /// Saves a list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table).</typeparam>
        /// <param name="entities">The list of entities to be saved.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult SaveInTransaction<E>(List<E> entities) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (E e in entities)
                        {
                            Save<E>(e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Save)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Saves a list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table).</param>
        /// <param name="entities">The list of entities to be saved.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult SaveInTransaction(Type entityType, List<object> entities)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (object e in entities)
                        {
                            Save(entityType, e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Save)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Inserts list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table).</typeparam>
        /// <param name="entities">The list of entities to be inserted.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult InsertInTransaction<E>(List<E> entities) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (E e in entities)
                        {
                            Insert<E>(e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Insert)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Inserts list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table).</param>
        /// <param name="entities">The list of entities to be inserted.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult InsertInTransaction(Type entityType, List<object> entities)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (object e in entities)
                        {
                            Insert(entityType, e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Insert)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table).</typeparam>
        /// <param name="entities">The list of entities to be deleted.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteInTransaction<E>(List<E> entities) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (E e in entities)
                        {
                            Delete<E>(e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Delete)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes list of entities to the underlying DbContext in a single transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table).</param>
        /// <param name="entities">The list of entities to be deleted.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteInTransaction(Type entityType, List<object> entities)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (object e in entities)
                        {
                            Delete(e, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Delete)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes list of entities to the underlying DbContext in a single transaction based on the surrogate keys of the entities.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="surrogateKeys">The surrogate keys entities (database records).</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteBySurrogateKeyInTransaction<E>(List<object> surrogateKeys) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (object keyValue in surrogateKeys)
                        {
                            DeleteBySurrogateKey<E>(keyValue, null);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteBySurrogateKey)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes list of entities to the underlying DbContext in a single transaction based on the surrogate keys of the entities.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table).</param>
        /// <param name="surrogateKeys">The surrogate keys entities (database records).</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteBySurrogateKeyInTransaction(Type entityType, List<object> surrogateKeys)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        foreach (object key in surrogateKeys)
                        {
                            DeleteBySurrogateKey(key, null, entityType);
                        }
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteBySurrogateKey)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes all entities of the specified type from the underlying DbContext in a single transaction based on the surrogate keys of the entities.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table).</typeparam>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteAllInTransaction<E>() where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        DeleteAll<E>();
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Deletes all entities of the specified type from the underlying DbContext in a single transaction based on the surrogate keys of the entities. 
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table)</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceProcedureResult DeleteAllInTransaction(Type entityType)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        DeleteAll(entityType);
                        t.Complete();
                    }
                    return new ServiceProcedureResult();
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets an entity of the specified type based on it's surrogate key. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table)</typeparam>
        /// <param name="keyValue">The value of the surrogate key.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<E> GetEntityBySurrogateKeyInTransaction<E>(object keyValue) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    E result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetEntityBySurrogateKey<E>(keyValue);
                        t.Complete();
                    }
                    return new ServiceFunctionResult<E> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<E>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets an entity of the specified type based on it's surrogate key. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table)</param>
        /// <param name="keyValue">The value of the surrogate key.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<object> GetEntityBySurrogateKeyInTransaction(Type entityType, object keyValue)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    object result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetEntityBySurrogateKey(entityType, keyValue);
                        t.Complete();
                    }
                    return new ServiceFunctionResult<object> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<object>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets a list of entities of the specified type based by searching for a specific field and its value i.e. filtering based on a column. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table)</typeparam>
        /// <param name="fieldName">The name of the field (column) to filter by.</param>
        /// <param name="fieldValue">The value of the field (column) to filter by.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<List<E>> GetEntitiesByFieldInTransaction<E>(string fieldName, object fieldValue) where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    List<E> result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetEntitiesByField<E>(fieldName, fieldValue);
                        t.Complete();
                    }
                    return new ServiceFunctionResult<List<E>> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<List<E>>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets a list of entities of the specified type based by searching for a specific field and its value i.e. filtering based on a column. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table)</param>
        /// <param name="fieldName">The name of the field (column) to filter by.</param>
        /// <param name="fieldValue">The value of the field (column) to filter by.</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<List<object>> GetEntitiesByFieldInTransaction(Type entityType, string fieldName, object fieldValue)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    List<object> result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetEntitiesByField(entityType, fieldName, fieldValue);
                        t.Complete();
                    }
                    return new ServiceFunctionResult<List<object>> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<List<object>>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets all entities of the specified type i.e. all records in a database table. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table)</typeparam>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<List<E>> GetAllEntitiesInTransaction<E>() where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    List<E> result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetAllEntities<E>();
                        t.Complete();
                    }
                    return new ServiceFunctionResult<List<E>> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<List<E>>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets all entities of the specified type i.e. all records in a database table. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <param name="entityType">The type of entity (database table)</param>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<List<object>> GetAllEntitiesInTransaction(
            Type entityType)
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    List<object> result = null;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetAllEntities(entityType);
                        t.Complete();
                    }
                    return new ServiceFunctionResult<List<object>> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<List<object>>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets an integer (Int32) count of the specified type i.e. number of records records in a database table. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table)</typeparam>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<int> GetTotalCountInTransaction<E>() where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    int result = 0;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetTotalCount<E>();
                        t.Complete();
                    }
                    return new ServiceFunctionResult<int> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<int>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        /// <summary>
        /// Gets a long (Int64) count of the specified type i.e. number of records records in a database table. Query is performed in a transaction.
        /// The transaction is retried X times based on the TransactionDeadlockRetryAttempts setting configured in the NKitDbRepository section of the appsettings.xml file.
        /// No exception is thrown if all the attemps failed. Instead a result is returned with a result code of error.
        /// </summary>
        /// <typeparam name="E">The type of entity (database table)</typeparam>
        /// <returns>Returns a wrapper containing the result and ServiceResult code.</returns>
        public ServiceFunctionResult<long> GetTotalCountLongInTransaction<E>() where E : class
        {
            int attempts = 0;
            while (attempts < _transactionDeadlockRetryAttempts)
            {
                try
                {
                    long result = 0;
                    using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                    {
                        result = GetTotalCountLong<E>();
                        t.Complete();
                    }
                    return new ServiceFunctionResult<long> { Contents = result };
                }
                catch (SqlException sqlEx)
                {
                    attempts++;
                    if (!IsSqlServerDeadlockException(sqlEx) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
                catch (SqliteException sqliteException)
                {
                    attempts++;
                    if (!IsSqliteLockException(sqliteException) || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqliteException; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceFunctionResult<long>(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        #endregion //Core Methods

        #endregion //Transactional Crud Methods

        #region Utility Methods

        protected virtual string GetSearchFilterLowered(string searchFilter)
        {
            return searchFilter == null ? string.Empty : searchFilter.ToLower();
        }

        #endregion //Utility Methods

        #region Methods to be implemented

        public virtual List<EmailNotificationRecipient> GetInfoEmailNotificationRecipientsFailSafe()
        {
            try
            {
                return _emailClientServiceSettings != null ? _emailClientServiceSettings.DefaultEmailRecipients : new List<EmailNotificationRecipient>();
            }
            catch (Exception ex)
            {
                return new List<EmailNotificationRecipient>();
            }
        }

        public virtual List<EmailNotificationRecipient> GetErrorEmailNotificationRecipientsFailSafe()
        {
            try
            {
                return _emailClientServiceSettings != null ? _emailClientServiceSettings.DefaultEmailRecipients : new List<EmailNotificationRecipient>();
            }
            catch (Exception ex)
            {
                return new List<EmailNotificationRecipient>();
            }
        }

        #endregion //Methods to be implemented

        #region Logging Methods

        public virtual NKitLogEntry LogMessageToNKitLogEntry(string message, string source, string className, string functionName, string eventName)
        {
            if (!_loggingSettings.LogToNKitLogEntryDatabaseTable || !SqlTableExists(nameof(NKitLogEntry)))
            {
                return null;
            }
            NKitLogEntry logEntry = new NKitLogEntry()
            {
                NKitLogEntryId = Guid.NewGuid(),
                Message = message,
                Source = source,
                ClassName = className,
                FunctionName = functionName,
                StackTrace = null,
                EventId = 0,
                EventName = eventName,
                DateCreated = DateTime.Now
            };
            List<LinqFunnelChangeResult> changeResult = Insert<NKitLogEntry>(logEntry, logEntry.NKitLogEntryId);
            return logEntry;
        }

        /// <summary>
        /// Logs the Exception to the NKitLogEntry table if the table exists in the database.
        /// A sql query is run against the database first to check that the table exists in the database before trying to insert a log entry to it.
        /// To ensure that the table exists the NKitLogEntry model needs to registered by underlying DbContext as a DbSet in the application using the NKit.
        /// </summary>
        public virtual NKitLogEntry LogExceptionToNKitLogEntry(Exception ex, Nullable<EventId> eventId, bool includeExceptionDetailsInErrorMessage)
        {
            if (!_loggingSettings.LogToNKitLogEntryDatabaseTable || !SqlTableExists(nameof(NKitLogEntry)))
            {
                return null;
            }
            string message = ExceptionHandler.GetCompleteExceptionMessage(ex, includeExceptionDetailsInErrorMessage);
            NKitLogEntry logEntry = new NKitLogEntry()
            {
                NKitLogEntryId = Guid.NewGuid(),
                Message = message,
                Source = ex.Source,
                ClassName = ex.TargetSite != null ? ex.TargetSite.DeclaringType.FullName : null,
                FunctionName = ex.TargetSite != null ? ex.TargetSite.Name : null,
                StackTrace = ex.StackTrace != null ? ex.StackTrace : null,
                EventId = eventId.HasValue ? eventId.Value.Id : 0,
                EventName = eventId.HasValue ? eventId.Value.Name : null,
                DateCreated = DateTime.Now
            };
            List<LinqFunnelChangeResult> changeResult = Insert<NKitLogEntry>(logEntry, logEntry.NKitLogEntryId);
            return logEntry;
        }

        public virtual NKitLogEntry LogWebActionActivityToNKitLogEntry(string className, string actionName, string message, Nullable<EventId> eventId)
        {
            if (!_loggingSettings.LogToNKitLogEntryDatabaseTable || !SqlTableExists(nameof(NKitLogEntry)))
            {
                return null;
            }
            NKitLogEntry logEntry = new NKitLogEntry()
            {
                NKitLogEntryId = Guid.NewGuid(),
                Message = message,
                Source = _generalSettings.ApplicationName,
                ClassName = className,
                FunctionName = actionName ?? "Request",
                StackTrace = null,
                EventId = eventId.HasValue ? eventId.Value.Id : 0,
                EventName = eventId.HasValue ? eventId.Value.Name : null,
                DateCreated = DateTime.Now
            };
            List<LinqFunnelChangeResult> changeResult = Insert<NKitLogEntry>(logEntry, logEntry.NKitLogEntryId);
            return logEntry;
        }

        public void CreateNKitLogEntry(NKitLogEntry logEntry)
        {
            using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
            {
                Set<NKitLogEntry>().Add(logEntry);
                SaveChanges();
                t.Complete();
            }
        }

        public void SaveNKitLogEntry(NKitLogEntry e)
        {
            using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
            {
                NKitLogEntry original = GetFirstEntity<NKitLogEntry>(p => p.NKitLogEntryId == e.NKitLogEntryId);
                if (original != null)
                {
                    Set<NKitLogEntry>().Remove(original);
                    SaveChanges();
                }
                Set<NKitLogEntry>().Add(e);
                SaveChanges();
                t.Complete();
            }
        }

        public NKitLogEntry GetNKitLogEntry(Guid logEntryId, bool throwExceptionOnNotFound)
        {
            NKitLogEntry result = (from logEntry in Set<NKitLogEntry>()
                                   where logEntry.NKitLogEntryId == logEntryId
                                   select logEntry).FirstOrDefault();
            if (result == null && throwExceptionOnNotFound)
            {
                throw new Exception($"Could not find {nameof(Models.NKitLogEntry)} with {nameof(Models.NKitLogEntry.NKitLogEntryId)} of '{logEntryId}'.");
            }
            return result;
        }

        public NKitLogEntryViewModel GetNKitLogEntryViewModel(Guid logEntryId, bool throwExceptionOnNotFound)
        {
            NKitLogEntry e = GetNKitLogEntry(logEntryId, throwExceptionOnNotFound);
            if (e == null)
            {
                return null;
            }
            return new NKitLogEntryViewModel(e);
        }

        public int GetNKitLogEntriesDatasetRecordCount(
            string searchFilter,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange)
        {
            bool searchOnDateRange = filterByDateRange && startDate.HasValue && endDate.HasValue;
            int result = (from logEntry in Set<NKitLogEntry>()
                          where
                                (searchOnDateRange ? logEntry.DateCreated.Date >= startDate && logEntry.DateCreated.Date <= endDate : true) &&
                                (logEntry.Message.ToLower().Contains(searchFilter) ||
                                logEntry.Source.ToLower().Contains(searchFilter) ||
                                logEntry.ClassName.ToLower().Contains(searchFilter) ||
                                logEntry.FunctionName.ToLower().Contains(searchFilter) ||
                                logEntry.StackTrace.ToLower().Contains(searchFilter) ||
                                logEntry.EventId.ToString().ToLower().Contains(searchFilter) ||
                                logEntry.EventName.ToLower().Contains(searchFilter))
                          orderby logEntry.DateCreated descending
                          select logEntry).Count();
            return result;
        }

        public string FormatNKitLogEntryPageSortColumn(string sortColumn, SortDirectionTypeCore sortDirectionType)
        {
            string result = sortColumn;
            if (string.IsNullOrEmpty(sortColumn))
            {
                result = $"{nameof(Models.NKitLogEntry.DateCreated)} {SortDirectionCore.DESCENDING_LONG}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.Message), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.Message)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.MessageShortened), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.Message)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.Source), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.Source)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.ClassName), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.ClassName)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.FunctionName), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.FunctionName)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.StackTrace), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.StackTrace)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.StackTraceShortened), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.StackTrace)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.EventId), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.EventId)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.EventName), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.EventName)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else if (sortColumn.Equals(nameof(NKitLogEntryViewModel.DateCreated), StringComparison.OrdinalIgnoreCase))
            {
                result = $"{nameof(Models.NKitLogEntry.DateCreated)} {SortDirectionCore.GetSortDirectionLong(sortDirectionType)}";
            }
            else
            {
                result = $"{nameof(Models.NKitLogEntry.DateCreated)} {SortDirectionCore.DESCENDING_LONG}";
            }
            return result;
        }

        public List<NKitLogEntry> GetNKitLogEntriesByPage(
            string searchFilter,
            int pageSize,
            int pageIndex,
            int numberOfRecordsToSkip,
            string sortColumn,
            SortDirectionTypeCore sortDirectionType,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange,
            out int totalFullDatasetRecordCount)
        {
            searchFilter = GetSearchFilterLowered(searchFilter);
            sortColumn = FormatNKitLogEntryPageSortColumn(sortColumn, sortDirectionType);
            totalFullDatasetRecordCount = GetNKitLogEntriesDatasetRecordCount(searchFilter, startDate, endDate, filterByDateRange);
            bool searchOnDateRange = filterByDateRange & startDate.HasValue && endDate.HasValue;
            List<NKitLogEntry> result = (from logEntry in Set<NKitLogEntry>()
                                         where
                                               (searchOnDateRange ? logEntry.DateCreated.Date >= startDate && logEntry.DateCreated.Date <= endDate : true) &&
                                               (logEntry.Message.ToLower().Contains(searchFilter) ||
                                               logEntry.Source.ToLower().Contains(searchFilter) ||
                                               logEntry.ClassName.ToLower().Contains(searchFilter) ||
                                               logEntry.FunctionName.ToLower().Contains(searchFilter) ||
                                               logEntry.StackTrace.ToLower().Contains(searchFilter) ||
                                               logEntry.EventId.ToString().ToLower().Contains(searchFilter) ||
                                               logEntry.EventName.ToLower().Contains(searchFilter))
                                         orderby logEntry.DateCreated descending
                                         select logEntry).OrderBy(sortColumn).Skip(numberOfRecordsToSkip).Take(pageSize).ToList();
            return result;
        }

        public List<NKitLogEntryViewModel> GetNKitLogEntryViewModelsByPage(
            string searchFilter,
            int pageSize,
            int pageIndex,
            int numberOfRecordsToSkip,
            string sortColumn,
            SortDirectionTypeCore sortDirectionType,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange,
            out int totalFullDatasetRecordCount)
        {
            List<NKitLogEntry> logEntries = GetNKitLogEntriesByPage(searchFilter, pageSize, pageIndex, numberOfRecordsToSkip, sortColumn, sortDirectionType, startDate, endDate, filterByDateRange, out totalFullDatasetRecordCount);
            List<NKitLogEntryViewModel> result = new List<NKitLogEntryViewModel>();
            foreach (NKitLogEntry logEntry in logEntries)
            {
                NKitLogEntryViewModel model = new NKitLogEntryViewModel(logEntry);
                result.Add(model);
            }
            return result;
        }

        public List<NKitLogEntry> GetNKitLogEntriesByFilter(
            string searchFilter,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange)
        {
            searchFilter = GetSearchFilterLowered(searchFilter);
            bool searchOnDateRange = filterByDateRange & startDate.HasValue && endDate.HasValue;
            return (from logEntry in Set<NKitLogEntry>()
                    where
                        (searchOnDateRange ? logEntry.DateCreated.Date >= startDate && logEntry.DateCreated.Date <= endDate : true) &&
                        (logEntry.Message.ToLower().Contains(searchFilter) ||
                        logEntry.Source.ToLower().Contains(searchFilter) ||
                        logEntry.ClassName.ToLower().Contains(searchFilter) ||
                        logEntry.FunctionName.ToLower().Contains(searchFilter) ||
                        logEntry.StackTrace.ToLower().Contains(searchFilter) ||
                        logEntry.EventId.ToString().ToLower().Contains(searchFilter) ||
                        logEntry.EventName.ToLower().Contains(searchFilter))
                    orderby logEntry.DateCreated descending
                    select logEntry).ToList();
        }

        public List<NKitLogEntryViewModel> GetNKitLogEntryViewModelsByFilter(
            string searchFilter,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange)
        {
            List<NKitLogEntry> logEntries = GetNKitLogEntriesByFilter(searchFilter, startDate, endDate, filterByDateRange);
            List<NKitLogEntryViewModel> result = new List<NKitLogEntryViewModel>();
            foreach (NKitLogEntry logEntry in logEntries)
            {
                NKitLogEntryViewModel model = new NKitLogEntryViewModel(logEntry);
                result.Add(model);
            }
            return result;
        }

        public List<NKitLogEntryCsvModel> GetNKitLogEntryCsvModelsByFilter(
            string searchFilter,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange)
        {
            List<NKitLogEntry> logEntries = GetNKitLogEntriesByFilter(searchFilter, startDate, endDate, filterByDateRange);
            List<NKitLogEntryCsvModel> result = new List<NKitLogEntryCsvModel>();
            foreach (NKitLogEntry logEntry in logEntries)
            {
                NKitLogEntryCsvModel model = new NKitLogEntryCsvModel(logEntry);
                result.Add(model);
            }
            return result;
        }

        public List<LinqFunnelChangeResult> DeleteNKitLogEntriesByFilter(
            string searchFilter,
            Nullable<DateTime> startDate,
            Nullable<DateTime> endDate,
            bool filterByDateRange)
        {
            List<LinqFunnelChangeResult> result = new List<LinqFunnelChangeResult>();
            using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
            {
                List<NKitLogEntry> entities = GetNKitLogEntriesByFilter(searchFilter, startDate, endDate, filterByDateRange);
                foreach (NKitLogEntry e in entities)
                {
                    result.AddRange(Delete<NKitLogEntry>(e, e.DateCreated));
                }
                t.Complete();
            }
            return result;
        }

        #endregion //Logging Methods

        #endregion //Methods
    }
}
