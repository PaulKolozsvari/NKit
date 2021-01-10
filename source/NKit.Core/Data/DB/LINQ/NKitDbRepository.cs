namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Data;
    using NKit.Standard.Data.DB.LINQ;
    using NKit.Web.Service;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

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

        /// <summary>
        /// Error code on transaction deadlock exeptions.
        /// </summary>
        public const int SQL_TRANSACTION_DEADLOCK_ERROR_CODE = 1205;

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

        #region Core Methods

        public ServiceProcedureResult Save<E>(
            List<E> entities,
            string userName) where E : class
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Save)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult Save(
            Type entityType,
            List<object> entities,
            string userName)
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Save)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult Insert<E>(
            List<E> entities,
            string userName) where E : class
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Insert)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult Insert(
            Type entityType,
            List<object> entities,
            string userName)
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Insert)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult Delete<E>(
            List<E> entities,
            string userName) where E : class
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Delete)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult Delete(
            Type entityType,
            List<object> entities,
            string userName)
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(Delete)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult DeleteBySurrogateKey<E>(
            List<object> surrogateKeys,
            string userName) where E : class
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteBySurrogateKey)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult DeleteBySurrogateKey(
            Type entityType,
            List<object> surrogateKeys,
            string userName)
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteBySurrogateKey)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult DeleteAll<E>(
            string userName) where E : class
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        public ServiceProcedureResult DeleteAll(
            Type entityType,
            string userName)
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
                    if (sqlEx.Number != SQL_TRANSACTION_DEADLOCK_ERROR_CODE || attempts >= _transactionDeadlockRetryAttempts)
                    {
                        throw sqlEx; //If this was not caused by a deadlock, or if the retry attempts have been reached, then throw the exception.
                    }
                    Thread.Sleep(_transactionDeadlockRetryWaitPeriod);
                    continue;
                }
            }
            return new ServiceProcedureResult(new ServiceResult() { Code = ServiceResultCode.FatalError, Message = $"{nameof(DeleteAll)} operation could not be completed. See previous errors for results." });
        }

        public ServiceFunctionResult<E> GetEntityBySurrogateKey<E>(
            object keyValue,
            string userName) where E : class
        {
            return new ServiceFunctionResult<E> { Contents = base.GetEntityBySurrogateKey<E>(keyValue) };
        }

        public ServiceFunctionResult<object> GetEntityBySurrogateKey(
            Type entityType,
            object keyValue,
            string userName)
        {
            return new ServiceFunctionResult<object>() { Contents = base.GetEntityBySurrogateKey(entityType, keyValue) };
        }

        public ServiceFunctionResult<List<E>> GetEntitiesByField<E>(
            string fieldName,
            object fieldValue,
            string userName) where E : class
        {
            return new ServiceFunctionResult<List<E>>() { Contents = base.GetEntitiesByField<E>(fieldName, fieldValue) };
        }

        public ServiceFunctionResult<List<object>> GetEntitiesByField(
            Type entityType,
            string fieldName,
            object fieldValue,
            string userName)
        {
            return new ServiceFunctionResult<List<object>>() { Contents = base.GetEntitiesByField(entityType, fieldName, fieldValue) };
        }

        public ServiceFunctionResult<List<E>> GetAllEntities<E>(
            string userName) where E : class
        {
            return new ServiceFunctionResult<List<E>>() { Contents = base.GetAllEntities<E>() };
        }

        public ServiceFunctionResult<List<object>> GetAllEntities(
            Type entityType,
            string userName)
        {
            return new ServiceFunctionResult<List<object>>() { Contents = base.GetAllEntities(entityType) };
        }

        public ServiceFunctionResult<int> GetTotalCount<E>(
            string userName) where E : class
        {
            return new ServiceFunctionResult<int>() { Contents = base.GetTotalCount<E>() };
        }

        public ServiceFunctionResult<long> GetTotalCountLong<E>(
            string userName) where E : class
        {
            try
            {
                return new ServiceFunctionResult<long>() { Contents = base.GetTotalCountLong<E>() };
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
