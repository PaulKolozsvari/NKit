namespace NKit.Core.Data.DB.LINQ
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
    using NKit.Data;
    using NKit.Standard.Data.DB.LINQ;
    using NKit.Web.Service;

    #endregion //Using Directives

    /// <summary>
    /// A wrapper around the LinqFunnelContext that allows management of multiple entities at a time.
    /// </summary>
    /// <typeparam name="D"></typeparam>
    public class LinqEntityContextCore<D> : LinqFunnelContextCore<D> where D : DbContext
    {
        #region Constructors

        /// <summary>
        /// Creates an entity context with the service provider from which it will get the entity framework DbContext.
        /// </summary>
        /// <param name="serviceProvider">Service provider to be used for finding the DbContext.</param>
        /// <param name="settings">Database related settings.</param>
        /// <param name="transactionScopeOption">TransactionScopeOption to used for constructing a TransactionScope.</param>
        /// <param name="transactionOptions">TransactionOptions to used for constructing a TransactionScope.</param>
        /// <param name="transactionDeadlockRetryAttempts">Number of times to retry an operation in case of a transaction deadlock.</param>
        /// <param name="transactionDeadlockRetryWaitPeriod">Number of millisends to wait before each retry an operation causes a transaction deadlock.</param>
        public LinqEntityContextCore(
            IServiceProvider serviceProvider,
            LinqFunnelSettings settings,
            TransactionScopeOption transactionScopeOption,
            TransactionOptions transactionOptions,
            int transactionDeadlockRetryAttempts,
            int transactionDeadlockRetryWaitPeriod) : base(serviceProvider, settings)
        {
            Initialize(transactionScopeOption, transactionOptions, transactionDeadlockRetryAttempts, transactionDeadlockRetryWaitPeriod);
        }

        /// <summary>
        /// Creates an entity context using the specified entity framework DbContext.
        /// </summary>
        /// <param name="db">The DbContext to use for running operations against the database.</param>
        /// <param name="settings">Database related settings.</param>
        /// <param name="transactionScopeOption">TransactionScopeOption to used for constructing a TransactionScope.</param>
        /// <param name="transactionOptions">TransactionOptions to used for constructing a TransactionScope.</param>
        /// <param name="transactionDeadlockRetryAttempts">Number of times to retry an operation in case of a transaction deadlock.</param>
        /// <param name="transactionDeadlockRetryWaitPeriod">Number of millisends to wait before each retry an operation causes a transaction deadlock.</param>
        public LinqEntityContextCore(
            D db,
            LinqFunnelSettings settings,
            TransactionScopeOption transactionScopeOption,
            TransactionOptions transactionOptions,
            int transactionDeadlockRetryAttempts,
            int transactionDeadlockRetryWaitPeriod) : base(db, settings)
        {
            Initialize(transactionScopeOption, transactionOptions, transactionDeadlockRetryAttempts, transactionDeadlockRetryWaitPeriod);
        }

        private void Initialize(
            TransactionScopeOption transactionScopeOption,
            TransactionOptions transactionOptions,
            int transactionDeadlockRetryAttempts,
            int transactionDeadlockRetryWaitPeriod)
        {
            DataValidator.ValidateIntegerNotNegative(transactionDeadlockRetryAttempts, nameof(transactionDeadlockRetryAttempts), nameof(LinqEntityContextCore<D>));
            DataValidator.ValidateIntegerNotNegative(transactionDeadlockRetryWaitPeriod, nameof(transactionDeadlockRetryWaitPeriod), nameof(LinqEntityContextCore<D>));

            _transactionScopeOption = transactionScopeOption;
            _transactionOptions = transactionOptions;
            _transactionDeadlockRetryAttempts = transactionDeadlockRetryAttempts;
            _transactionDeadlockRetryWaitPeriod = transactionDeadlockRetryWaitPeriod;
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
            Nullable<Guid> userId,
            string userName,
            bool saveChildren) where E : class
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
                            base.Save<E>(e, null, saveChildren);
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
            Nullable<Guid> userId,
            string userName,
            bool saveChildren)
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
                            base.Save(entityType, e, null, false);
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
            Nullable<Guid> userId,
            string userName,
            bool saveChildren) where E : class
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
                            base.Insert<E>(e, null, false);
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
            Nullable<Guid> userId,
            string userName,
            bool saveChildren)
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
                            base.Insert(entityType, e, null, false);
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
            Nullable<Guid> userId,
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
            Nullable<Guid> userId,
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
            Nullable<Guid> userId,
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
            Nullable<Guid> userId,
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
            Nullable<Guid> userId,
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
            Nullable<Guid> userId,
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
            bool loadChildren,
            Nullable<Guid> userId,
            string userName) where E : class
        {
            return new ServiceFunctionResult<E> { Contents = base.GetEntityBySurrogateKey<E>(keyValue, loadChildren) };
        }

        public ServiceFunctionResult<object> GetEntityBySurrogateKey(
            Type entityType,
            object keyValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            return new ServiceFunctionResult<object>() { Contents = base.GetEntityBySurrogateKey(entityType, keyValue, loadChildren) };
        }

        public ServiceFunctionResult<List<E>> GetEntitiesByField<E>(
            string fieldName,
            object fieldValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName) where E : class
        {
            return new ServiceFunctionResult<List<E>>() { Contents = base.GetEntitiesByField<E>(fieldName, fieldValue, loadChildren) };
        }

        public ServiceFunctionResult<List<object>> GetEntitiesByField(
            Type entityType,
            string fieldName,
            object fieldValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            return new ServiceFunctionResult<List<object>>() { Contents = base.GetEntitiesByField(entityType, fieldName, fieldValue, loadChildren) };
        }

        public ServiceFunctionResult<List<E>> GetAllEntities<E>(
            bool loadChildren,
            Nullable<Guid> userId,
            string userName) where E : class
        {
            return new ServiceFunctionResult<List<E>>() { Contents = base.GetAllEntities<E>(loadChildren) };
        }

        public ServiceFunctionResult<List<object>> GetAllEntities(
            Type entityType,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            return new ServiceFunctionResult<List<object>>() { Contents = base.GetAllEntities(entityType, loadChildren) };
        }

        public ServiceFunctionResult<int> GetTotalCount<E>(
            Nullable<Guid> userId,
            string userName) where E : class
        {
            return new ServiceFunctionResult<int>() { Contents = base.GetTotalCount<E>() };
        }

        public ServiceFunctionResult<long> GetTotalCountLong<E>(
            Nullable<Guid> userId,
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
