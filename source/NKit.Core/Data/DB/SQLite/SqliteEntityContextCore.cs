namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Data;
    using System.Text;
    using System.IO;
    using Microsoft.EntityFrameworkCore.Sqlite.Internal;
    using System.Collections;
    using System.Linq;

    #endregion //Using Directives

    public class SqliteEntityContextCore
    {
        #region Constructors

        public SqliteEntityContextCore(SqliteSettingsCore settings)
        {
            DataValidator.ValidateObjectNotNull(settings, nameof(settings), nameof(SqliteEntityContextCore));
            _settings = settings;
            CreateDatabaseFile(settings.DatabaseFilePath);
            ResetSqliteDatabase();
        }

        #endregion //Constructors

        #region Fields

        private SqliteSettingsCore _settings;
        private SqliteDatabaseCore _sqliteDatabase;

        #endregion //Fields

        #region Properties

        public SqliteSettingsCore Settings
        {
            get { return _settings; }
        }

        protected SqliteDatabaseCore Database
        {
            get { return _sqliteDatabase; }
        }

        #endregion //Properties

        #region Schema Methods

        protected void ResetSqliteDatabase()
        {
            _sqliteDatabase = new SqliteDatabaseCore(_settings.ConnectionString, populateTablesFromSchema: true, createOrmAssembly: false, saveOrmAssembly: false, copyOrmAssembly: false, ormAssemblyCopyDirectory: null, overrideNameWithDatabaseNameFromSchema: false);
        }

        private void CreateDatabaseFile(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                SQLiteConnection.CreateFile(databaseFilePath);
            }
        }

        public int CreateTable<T>(SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            return CreateTable<T>(connection, transaction, disposeConnectionAfterExecute, null);
        }

        public int CreateTable<T>(SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute, Dictionary<string, Type> extraColumns) where T : class
        {
            Type type = typeof(T);
            SqliteDatabaseTableCore table = new SqliteDatabaseTableCore(type.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
            if (extraColumns != null)
            {
                foreach (KeyValuePair<string, Type> column in extraColumns)
                {
                    table.AddColumn(column.Key, column.Value);
                }
            }
            //int resultCode = ExecuteNonQuery(table.GetSqlDropTableScript());
            int resultCode = ExecuteNonQuery(table.GetSqlCreateTableScript(), connection, transaction, disposeConnectionAfterExecute);
            return resultCode;
        }

        public int DropTable<T>(SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            Type type = typeof(T);
            SqliteDatabaseTableCore table = new SqliteDatabaseTableCore(type.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
            int resultCode = ExecuteNonQuery(table.GetSqlDropTableScript(), connection, transaction, disposeConnectionAfterExecute);
            return resultCode;
        }

        public void DeleteDatabaseIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        protected void CreateTableIndexers(List<SqliteIndexerCore> indexers, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute)
        {
            foreach (SqliteIndexerCore index in indexers)
            {
                string sqlScript = index.GetCreateSqlScript();
                ExecuteNonQuery(sqlScript, connection, transaction, disposeConnectionAfterExecute);
            }
        }

        #endregion //Schema Methods

        #region Execute Methods

        public int ExecuteNonQuery(string sqlQueryString, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute)
        {
            int result = -1;
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(_settings.ConnectionString);
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (SQLiteCommand command = new SQLiteCommand(sqlQueryString, connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = transaction;
                    }
                    result = command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
            return result;
        }

        #endregion //Execute Methods

        #region Crud Methods

        public int Update<T>(T entity, string columnName) where T : class
        {
            int resultCode;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Update(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Update<T>(T entity, string columnName, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            SqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as SqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we cannot update records on it.
            }
            resultCode = table.Update(entity, columnName, disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }

        public int Delete<T>(T entity, string columnName) where T : class
        {
            int resultCode;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Delete(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Delete<T>(T entity, string columnName, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            SqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as SqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we cannot delete records from it.
            }
            resultCode = table.Delete(entity, columnName, disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }

        public int Save<T>(T entity, string columnName) where T : class
        {
            int resultCode;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Save(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Save<T>(T e, string columnName, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            SqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as SqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we save records to it.
            }
            object columnValue = EntityReaderGeneric<object>.GetPropertyValue(columnName, e, true);
            T original = table.Query<T>(columnName, columnValue, null, false, connection, transaction).FirstOrDefault();
            if (original == null)
            {
                resultCode = table.Insert(e, disposeConnectionAfterExecute, connection, transaction);
            }
            else
            {
                resultCode = table.Update(e, columnName, disposeConnectionAfterExecute, connection, transaction);
            }
            return resultCode;
        }

        public void Save<T>(List<T> entities, string columnName) where T : class
        {
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    Save(entities, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
        }

        public void Save<T>(List<T> entities, string columnName, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            foreach (T e in entities)
            {
                Save<T>(e, columnName, connection, transaction, disposeConnectionAfterExecute);
            }
        }

        public int Insert<T>(T entity) where T : class
        {
            int resultCode;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Insert(entity, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Insert<T>(T entity, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            SqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as SqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we cannot insert records to it.
            }
            resultCode = table.Insert(entity, disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }

        public void Insert<T>(List<T> entities) where T : class
        {
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    Insert(entities, connection, transaction, false);
                    transaction.Commit();
                }
            }
        }

        public void Insert<T>(List<T> entities, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            foreach (T entity in entities)
            {
                Insert(entity, connection, transaction, disposeConnectionAfterExecute);
            }
        }

        public List<T> Read<T>() where T : class
        {
            return Read<T>(null, null);
        }

        public List<T> Read<T>(string columName, object columnValue) where T : class
        {
            List<T> entities;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    entities = Read<T>(columName, columnValue, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return entities;
        }

        public List<T> Read<T>(string columName, object columnValue, SQLiteConnection connection, SQLiteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            Type entityType = typeof(T);
            List<T> entities;
            SqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as SqliteDatabaseTableCore;
            if (table == null)
            {
                return new List<T>(); //If the table doesn't exist we cannot query from it.
            }
            entities = table.Query<T>(columName, columnValue, string.Empty, disposeConnectionAfterExecute: false, connection, transaction);
            return entities;
        }

        #endregion //Crud Methods
    }
}
