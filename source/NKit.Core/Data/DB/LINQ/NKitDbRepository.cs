namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.EntityFrameworkCore;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Data;
    using NKit.Standard.Data.DB.LINQ;
    using NKit.Web.Service;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Data.Sqlite;
    using Microsoft.Data.SqlClient;

    #endregion //Using Directives

    /// <summary>
    /// A facade wrapper (repository) around an Entity Framework DbContext used to manage it to provide CRUD operations.
    /// Extends the functionality of the NKitDbContextRepositoryBase that allows management of multiple entities at a time.
    /// Methods in this class are wrapped in Transactions.
    /// Managing DbContext the right way with Entity Framework 6: an in-depth guide: https://mehdi.me/ambient-dbcontext-in-ef6/
    /// Database.BeginTransaction vs Transactions.TransactionScope: https://stackoverflow.com/questions/22382892/database-begintransaction-vs-transactions-transactionscope
    /// </summary>
    /// <typeparam name="D"></typeparam>
    public class NKitDbRepository : NKitDbRespositoryBase
    {
        #region Constructors

        /// <summary>
        /// Creates an entity context with the service provider from which it will get the entity framework DbContext.
        /// </summary>
        /// <param name="serviceProvider">ServiceProvider to be used to get the DbContext of type D</param>
        /// <param name="dbContextType">The type of the DbContext that will be be created when using this constructor.</param>
        /// <param name="generalOptions">General settings.</param>
        /// <param name="dbContextOptions">DbContext related settings.</param>
        /// <param name="loggingOptions">Logging related settings.</param>
        public NKitDbRepository(
            IServiceProvider serviceProvider,
            Type dbContextType,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitDbRepositorySettings> dbContextOptions,
            IOptions<NKitLoggingSettings> loggingOptions) : base(serviceProvider, dbContextType, generalOptions, dbContextOptions, loggingOptions)
        {
            Initialize(dbContextOptions.Value);
        }

        /// <summary>
        /// Creates an entity context using the specified entity framework DbContext.
        /// </summary>
        /// <param name="db">The DbContext to use for running operations for the underlying database.</param>
        /// <param name="generalOptions">General settings.</param>
        /// <param name="dbContextOptions">DbContext related settings.</param>
        /// <param name="loggingOptions">Logging related settings.</param>
        public NKitDbRepository(
            DbContext db,
            IOptions<NKitGeneralSettings> generalOptions,
            IOptions<NKitDbRepositorySettings> dbContextOptions, 
            IOptions<NKitLoggingSettings> loggingOptions) : base(db, generalOptions, dbContextOptions, loggingOptions)
        {
            Initialize(dbContextOptions.Value);
        }

        private void Initialize(NKitDbRepositorySettings databaseSettings)
        {
            DataValidator.ValidateObjectNotNull(databaseSettings, nameof(databaseSettings), nameof(NKitDbRepository));
            DataValidator.ValidateIntegerNotNegative(databaseSettings.DatabaseTransactionDeadlockRetryAttempts, nameof(databaseSettings.DatabaseTransactionDeadlockRetryAttempts), nameof(NKitDbRepository));
            DataValidator.ValidateIntegerNotNegative(databaseSettings.DatabaseTransactionDeadlockRetryWaitPeriod, nameof(databaseSettings.DatabaseTransactionDeadlockRetryWaitPeriod), nameof(NKitDbRepository));

            _transactionScopeOption = databaseSettings.DatabaseTransactionScopeOption;
            _transactionOptions = new TransactionOptions() 
            { 
                IsolationLevel = databaseSettings.DatabaseTransactionIsolationLevel, 
                Timeout = new TimeSpan(0, 0, databaseSettings.DatabaseTransactionTimeoutSeconds) 
            };
            _transactionDeadlockRetryAttempts = databaseSettings.DatabaseTransactionDeadlockRetryAttempts;
            _transactionDeadlockRetryWaitPeriod = databaseSettings.DatabaseTransactionDeadlockRetryWaitPeriod;
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

        protected TransactionScopeOption _transactionScopeOption;
        protected TransactionOptions _transactionOptions;
        protected int _transactionDeadlockRetryAttempts;
        protected int _transactionDeadlockRetryWaitPeriod;

        #endregion //Fields

        #region Properties

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

        #endregion //Properties

        #region Methods

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

        #endregion //Utility Methods

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
                            base.Save<E>(e, null);
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
                            base.Save(entityType, e, null);
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
                            base.Insert<E>(e, null);
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
                            base.Insert(entityType, e, null);
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
                            base.Delete<E>(e, null);
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
                            base.Delete(e, null);
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
                            base.DeleteBySurrogateKey<E>(keyValue, null);
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
                            base.DeleteBySurrogateKey(key, null, entityType);
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
                        base.DeleteAll<E>();
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
                        base.DeleteAll(entityType);
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
                        result = base.GetEntityBySurrogateKey<E>(keyValue);
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
                        result = base.GetEntityBySurrogateKey(entityType, keyValue);
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
                        result = base.GetEntitiesByField<E>(fieldName, fieldValue);
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
                        result = base.GetEntitiesByField(entityType, fieldName, fieldValue);
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
                        result = base.GetAllEntities<E>();
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
                        result = base.GetAllEntities(entityType);
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
                        result = base.GetTotalCount<E>();
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
                        result = base.GetTotalCountLong<E>();
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

        #region Schema Query Methods

        //public List<SqlDatabaseTableColumn> GetSqlDatabaseTableColumnNames(string tableName)
        //{
        //    List<SqlDatabaseTableColumn> result = new List<SqlDatabaseTableColumn>();
        //    using (DB.Connection)
        //    {
        //        if (DB.Connection.State != ConnectionState.Open)
        //        {
        //            DB.Connection.Open();
        //        }
        //        DataTable schemaTable = DB.Connection.GetSchema("Columns", new string[] { null, null, tableName, null });
        //        DB.Connection.Close();
        //        foreach (DataRow schemaRow in schemaTable.Rows)
        //        {
        //            SqlDatabaseTableColumn column = new SqlDatabaseTableColumn(schemaRow);
        //            result.Add(column);
        //        }
        //    }
        //    return result;
        //}

        //public List<SqlDatabaseTable> GetSqlDatabaseTableNames()
        //{
        //    List<SqlDatabaseTable> result = new List<SqlDatabaseTable>();
        //    using (DB.Connection)
        //    {
        //        if (DB.Connection.State != ConnectionState.Open)
        //        {
        //            DB.Connection.Open();
        //        }
        //        DataTable schemaTable = DB.Connection.GetSchema("Tables");
        //        DB.Connection.Close();
        //        foreach (DataRow schemaRow in schemaTable.Rows)
        //        {
        //            SqlDatabaseTable table = new SqlDatabaseTable(schemaRow);
        //            result.Add(table);
        //        }
        //    }
        //    return result;
        //}

        #endregion //Schema Query Methods

        #endregion //Methods
    }
}
