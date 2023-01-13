namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Reflection;
    using System.Transactions;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using NKit.Data.DB.LINQ;
    using NKit.Web.Service;
    using NKit.Data.DB.SQLServer;
    using NKit.Data.DB.LINQ.Logging;
    using System.Data.Common;
    using Dapper;
    using System.Linq;

    #endregion //Using Directives

    //6 ways of doing locking in .NET (Pessimistic and optimistic): https://www.codeproject.com/Articles/114262/ways-of-doing-locking-in-NET-Pessimistic-and-opt
    //Deadlocks: https://blog.codinghorror.com/deadlocked/
    //Overview of concurrency in LINQ to SQL: https://www.c-sharpcorner.com/article/overview-of-concurrency-in-linq-to-sql/
    //Optimistic Concurrency Overview: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/optimistic-concurrency-overview
    //RefreshMode : https://docs.microsoft.com/en-us/dotnet/api/system.data.linq.refreshmode?view=netframework-4.7.2
    //How to: Specify When Concurrency Exceptions are Thrown: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/how-to-specify-when-concurrency-exceptions-are-thrown
    //How to handle concurrency in LINQ to SQL: https://www.codeproject.com/Articles/38299/How-To-Handle-Concurrency-in-LINQ-to-SQL
    //Pessimistic Concurrency in LINQ to SQL by using Transactions: https://blogs.msdn.microsoft.com/wriju/2007/08/06/linq-to-sql-using-transaction/
    //Manage Change Conflicts: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/how-to-manage-change-conflicts
    //Detect and Resolve Conflicting Submissions: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/how-to-detect-and-resolve-conflicting-submissions
    public class LinqEntityContextWindows : LinqFunnelContextWindows
    {
        #region Constructors

        public LinqEntityContextWindows(
            DataContext db,
            LinqFunnelSettings settings,
            bool handleExceptions,
            Type userLinqToSqlType,
            Type serverActionLinqToSqlType,
            Type serverErrorLinqToSqlType) : this(
                db,
                settings,
                handleExceptions,
                userLinqToSqlType,
                serverActionLinqToSqlType,
                serverErrorLinqToSqlType,
                false,
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = TransactionManager.DefaultTimeout
                },
                1,
                1000)
        {
        }

        public LinqEntityContextWindows(
            DataContext db,
            LinqFunnelSettings settings,
            bool handleExceptions,
            Type userLinqToSqlType,
            Type serverActionLinqToSqlType,
            Type serverErrorLinqToSqlType,
            bool useTransactionsOnCrudOperations,
            TransactionScopeOption transactionScopeOption,
            TransactionOptions transactionOptions,
            int transactionDeadlockRetryAttempts,
            int transactionDeadlockRetryWaitPeriod) :
            base(db, settings)
        {
            DataValidator.ValidateIntegerNotNegative(transactionDeadlockRetryAttempts, nameof(transactionDeadlockRetryAttempts), nameof(LinqEntityContextWindows));
            DataValidator.ValidateIntegerNotNegative(transactionDeadlockRetryWaitPeriod, nameof(transactionDeadlockRetryWaitPeriod), nameof(LinqEntityContextWindows));

            _handleExceptions = handleExceptions;
            _userLinqToSqlType = userLinqToSqlType;
            _serverActionLinqToSqlType = serverActionLinqToSqlType;
            _serverErrorLinqtoSqlType = serverErrorLinqToSqlType;

            _useTransactionsOnCrudOperations = useTransactionsOnCrudOperations;
            _transactionScopeOption = transactionScopeOption;
            _transactionOptions = transactionOptions;
            _transactionDeadlockRetryAttempts = transactionDeadlockRetryAttempts;
            _transactionDeadlockRetryWaitPeriod = transactionDeadlockRetryWaitPeriod;
        }

        #endregion //Constructors

        #region Constants

        public const int SQL_TRANSACTION_DEADLOCK_ERROR_CODE = 1205;

        #endregion //Constants

        #region Fields

        protected bool _handleExceptions;
        protected Type _userLinqToSqlType;
        protected Type _serverActionLinqToSqlType;
        protected Type _serverErrorLinqtoSqlType;

        protected bool _useTransactionsOnCrudOperations;
        protected TransactionScopeOption _transactionScopeOption;
        protected TransactionOptions _transactionOptions;
        protected int _transactionDeadlockRetryAttempts;
        protected int _transactionDeadlockRetryWaitPeriod;

        #endregion //Fields

        #region Properties

        public Type UserLinqToSqlType
        {
            get { return _userLinqToSqlType; }
        }

        public Type ServerActionLinqToSqlType
        {
            get { return _serverActionLinqToSqlType; }
        }

        public Type ServerErrorLinqToSqlType
        {
            get { return _serverErrorLinqtoSqlType; }
        }

        public TransactionScopeOption TransactionScopeOption
        {
            get { return _transactionScopeOption; }
        }

        public TransactionOptions TransactionOptions
        {
            get { return _transactionOptions; }
        }

        public int TransactionDeadlockRetryAttempts
        {
            get { return _transactionDeadlockRetryAttempts; }
        }

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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (E e in entities)
                            {
                                base.Save<E>(e, null, saveChildren).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (E e in entities)
                        {
                            base.Save<E>(e, null, saveChildren).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (object e in entities)
                            {
                                base.Save(entityType, e, null, false).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (object e in entities)
                        {
                            base.Save(entityType, e, null, false).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (E e in entities)
                            {
                                base.Insert<E>(e, null, false).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (E e in entities)
                        {
                            base.Insert<E>(e, null, false).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (object e in entities)
                            {
                                base.Insert(entityType, e, null, false).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (object e in entities)
                        {
                            base.Insert(entityType, e, null, false).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (E e in entities)
                            {
                                base.Delete<E>(e, null).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (E e in entities)
                        {
                            base.Delete<E>(e, null).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (object e in entities)
                            {
                                base.Delete(e, null).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (object e in entities)
                        {
                            base.Delete(e, null).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (object keyValue in surrogateKeys)
                            {
                                base.DeleteBySurrogateKey<E>(keyValue, null).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (object keyValue in surrogateKeys)
                        {
                            base.DeleteBySurrogateKey<E>(keyValue, null).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            foreach (object key in surrogateKeys)
                            {
                                base.DeleteBySurrogateKey(key, null, entityType).ForEach(c => HandleChange(c, userId, userName));
                            }
                            t.Complete();
                        }
                    }
                    else
                    {
                        foreach (object key in surrogateKeys)
                        {
                            base.DeleteBySurrogateKey(key, null, entityType).ForEach(c => HandleChange(c, userId, userName));
                        }
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            base.DeleteAll<E>().ForEach(c => HandleChange(c, userId, userName));
                            t.Complete();
                        }
                    }
                    else
                    {
                        base.DeleteAll<E>().ForEach(c => HandleChange(c, userId, userName));
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
                    if (_useTransactionsOnCrudOperations)
                    {
                        using (TransactionScope t = new TransactionScope(_transactionScopeOption, _transactionOptions))
                        {
                            base.DeleteAll(entityType).ForEach(c => HandleChange(c, userId, userName));
                            t.Complete();
                        }
                    }
                    else
                    {
                        base.DeleteAll(entityType).ForEach(c => HandleChange(c, userId, userName));
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
                catch (Exception ex)
                {
                    if (_handleExceptions)
                    {
                        HandleException(ex, userId, userName);
                    }
                    throw;
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
            try
            {
                return new ServiceFunctionResult<E> { Contents = base.GetEntityBySurrogateKey<E>(keyValue, loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<object> GetEntityBySurrogateKey(
            Type entityType,
            object keyValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            try
            {
                return new ServiceFunctionResult<object>() { Contents = base.GetEntityBySurrogateKey(entityType, keyValue, loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<List<E>> GetEntitiesByField<E>(
            string fieldName,
            object fieldValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName) where E : class
        {
            try
            {
                return new ServiceFunctionResult<List<E>>() { Contents = base.GetEntitiesByField<E>(fieldName, fieldValue, loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<List<object>> GetEntitiesByField(
            Type entityType,
            string fieldName,
            object fieldValue,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            try
            {
                return new ServiceFunctionResult<List<object>>() { Contents = base.GetEntitiesByField(entityType, fieldName, fieldValue, loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<List<E>> GetAllEntities<E>(
            bool loadChildren,
            Nullable<Guid> userId,
            string userName) where E : class
        {
            try
            {
                return new ServiceFunctionResult<List<E>>() { Contents = base.GetAllEntities<E>(loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<List<object>> GetAllEntities(
            Type entityType,
            bool loadChildren,
            Nullable<Guid> userId,
            string userName)
        {
            try
            {
                return new ServiceFunctionResult<List<object>>() { Contents = base.GetAllEntities(entityType, loadChildren) };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
        }

        public ServiceFunctionResult<int> GetTotalCount<E>(
            Nullable<Guid> userId,
            string userName) where E : class
        {
            try
            {
                return new ServiceFunctionResult<int>() { Contents = base.GetTotalCount<E>() };
            }
            catch (Exception ex)
            {
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw;
            }
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
                if (_handleExceptions)
                {
                    HandleException(ex, userId, userName);
                }
                throw ex;
            }
        }

        public Guid GetUserId(string userName)
        {
            List<object> query = GetEntitiesByField(
                _userLinqToSqlType,
                EntityReaderGeneric<LinqUserWindows>.GetPropertyName(p => p.UserName, false),
                userName,
                false);
            if (query.Count > 1)
            {
                throw new Exception(string.Format("More than one user with the user name of {0}.", userName));
            }
            if (query.Count < 1)
            {
                throw new Exception(string.Format("No user exists with the user name of {0}.", userName));
            }
            object user = query[0];
            PropertyInfo surrogateKey = GetEntitySurrogateKey(_userLinqToSqlType);
            Guid result = (Guid)EntityReader.GetPropertyValue(surrogateKey.Name, user, false);
            return result;
        }

        #endregion //Core Methods

        #region Utility Methods

        protected void HandleChange(LinqFunnelChangeResult change, Nullable<Guid> userId, string userName)
        {
            LinqServerActionWindows serverAction = new LinqServerActionWindows()
            {
                Function = change.Function,
                DateCreated = change.DateChanged,
                EntityChanged = change.EntityChanged,
                FieldChanged = change.FieldChanged
            };
            if (userId.HasValue && !string.IsNullOrEmpty(userName))
            {
                serverAction.UserId = userId;
                serverAction.UserName = userName;
            }
            if (change.OriginalValue != null)
            {
                serverAction.OriginalValue = change.OriginalValue.ToString();
            }
            if (change.NewValue != null)
            {
                serverAction.NewValue = change.NewValue.ToString();
            }
            if (_serverActionLinqToSqlType != null)
            {
                object serverActionLinqToSql = EntityReaderGeneric<LinqServerActionWindows>.ConvertTo(serverAction, _serverActionLinqToSqlType);
                base.Save(_serverActionLinqToSqlType, serverActionLinqToSql, false);
            }
        }

        protected ServiceResult HandleException(Exception ex)
        {
            return HandleException(ex, null, null);
        }

        //protected ServiceResult HandleException(Exception ex, User user)
        //{
        //    ServerError error = new ServerError()
        //    {
        //        ErrorMessage = ex.Message,
        //        DateCreated = DateTime.Now,
        //    };
        //    if (user != null)
        //    {
        //        error.UserId = user.UserId;
        //        error.UserName = user.UserName;
        //    }
        //    //HACK The saving of errors needs to be refactored. The reason for the below code is because we need to
        //    //create a new context, as the DB one has thrown an exception, therefore we are not able to use it any longer to save.
        //    //That's why the error is being saved in the first place i.e. because an exception has been thrown.
        //    DataContext context = ((DataContext)Activator.CreateInstance(DB.GetType()));
        //    context.Connection.ConnectionString = base.LinqFunnelSettings.ConnectionString;
        //    using (context)
        //    {
        //        context.GetTable<ServerError>().InsertOnSubmit(error);
        //        context.SubmitChanges();
        //    }
        //    ServiceException serviceException = ex as ServiceException;
        //    if (serviceException == null)
        //    {
        //        return new ServiceResult { Code = ServiceResultCode.FatalError, Message = ex.Message }; //Not a user thrown exception.
        //    }
        //    else
        //    {
        //        return serviceException.Result;
        //    }
        //}

        protected ServiceResult HandleException(Exception ex, Nullable<Guid> userId, string userName)
        {
            LinqServerErrorWindows serverError = new LinqServerErrorWindows()
            {
                ErrorMessage = ex.Message,
                DateCreated = DateTime.Now,
            };
            if (userId.HasValue && !string.IsNullOrEmpty(userName))
            {
                serverError.UserId = userId;
                serverError.UserName = userName;
            }
            //HACK The saving of errors needs to be refactored. The reason for the below code is because we need to
            //create a new context, as the DB one has thrown an exception, therefore we are not able to use it any longer to save.
            //That's why the error is being saved in the first place i.e. because an exception has been thrown.
            DataContext context = ((DataContext)Activator.CreateInstance(DB.GetType()));
            context.Connection.ConnectionString = base.LinqFunnelSettings.ConnectionString;
            using (context)
            {
                object serverErrorLinqToSql = EntityReaderGeneric<LinqServerErrorWindows>.ConvertTo(serverError, _serverErrorLinqtoSqlType);
                context.GetTable(_serverErrorLinqtoSqlType).InsertOnSubmit(serverErrorLinqToSql);
                context.SubmitChanges();
            }
            ServiceException serviceException = ex as ServiceException;
            if (serviceException == null)
            {
                return new ServiceResult { Code = ServiceResultCode.FatalError, Message = ex.Message }; //Not a user thrown exception.
            }
            else
            {
                return serviceException.Result;
            }
        }

        #endregion //Utility Methods

        #region Schema Query Methods

        public List<SqlDatabaseTableColumnWindows> GetSqlDatabaseTableColumnNames(string tableName)
        {
            List<SqlDatabaseTableColumnWindows> result = new List<SqlDatabaseTableColumnWindows>();
            using (DB.Connection)
            {
                if (DB.Connection.State != ConnectionState.Open)
                {
                    DB.Connection.Open();
                }
                DataTable schemaTable = DB.Connection.GetSchema("Columns", new string[] { null, null, tableName, null });
                DB.Connection.Close();
                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    SqlDatabaseTableColumnWindows column = new SqlDatabaseTableColumnWindows(schemaRow);
                    result.Add(column);
                }
            }
            return result;
        }

        public List<SqlDatabaseTableWindows> GetSqlDatabaseTableNames()
        {
            List<SqlDatabaseTableWindows> result = new List<SqlDatabaseTableWindows>();
            using (DB.Connection)
            {
                if (DB.Connection.State != ConnectionState.Open)
                {
                    DB.Connection.Open();
                }
                DataTable schemaTable = DB.Connection.GetSchema("Tables");
                DB.Connection.Close();
                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    SqlDatabaseTableWindows table = new SqlDatabaseTableWindows(schemaRow);
                    result.Add(table);
                }
            }
            return result;
        }

        public virtual bool SqlServerTableExists(string tableName)
        {
            string sqlQuery = $"SELECT COUNT(*) as Count FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
            DbConnection connection = DB.Connection;
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
            DbConnection connection = DB.Connection;
            if (connection == null)
            {
                return false;
            }
            string queryResult = connection.Query<string>(sqlQuery).FirstOrDefault(); // Query method extension provided by Dapper library.
            return !string.IsNullOrEmpty(queryResult) && queryResult.Equals(tableName);
        }

        #endregion //Schema Query Methods

        public virtual bool LogMessageToNKitLogEntry(string message, string source, string className, string functionName, string eventName)
        {
            return false;
        }

        public virtual bool LogExceptionToNKitLogEntry(Exception ex, bool includeExceptionDetailsInErrorMessage)
        {
            return false;
        }

        public virtual bool LogWebActionActivityToNKitLogEntry(string source, string className, string actionName, string message)
        {
            return false;
        }

        #endregion //Methods
    }
}