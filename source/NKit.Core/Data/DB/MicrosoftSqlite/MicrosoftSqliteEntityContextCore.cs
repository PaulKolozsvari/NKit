namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using Microsoft.Data.Sqlite;
    using System.Linq;

    #endregion //Using Directives

    public class MicrosoftSqliteEntityContextCore
    {
        #region Constructors

        public MicrosoftSqliteEntityContextCore(MicrosoftSqliteSettingsCore settings)
        {
            DataValidator.ValidateObjectNotNull(settings, nameof(settings), nameof(MicrosoftSqliteEntityContextCore));
            _settings = settings;
            CreateDatabaseFile(settings.DatabaseFilePath);
            ResetSqliteDatabase();
        }

        #endregion //Constructors

        #region Fields

        private MicrosoftSqliteSettingsCore _settings;
        private MicrosoftSqliteDatabaseCore _sqliteDatabase;

        #endregion //Fields

        #region Properties

        public MicrosoftSqliteSettingsCore Settings
        {
            get { return _settings; }
        }

        protected MicrosoftSqliteDatabaseCore Database
        {
            get { return _sqliteDatabase; }
        }

        #endregion //Properties

        #region Schema Methods

        protected void ResetSqliteDatabase()
        {
            _sqliteDatabase = new MicrosoftSqliteDatabaseCore(_settings.ConnectionString, populateTablesFromSchema: true, createOrmAssembly: false, saveOrmAssembly: false, copyOrmAssembly: false, ormAssemblyCopyDirectory: null, overrideNameWithDatabaseNameFromSchema: false);
        }

        private void CreateDatabaseFile(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                //SqliteConnection.CreateFile(databaseFilePath); //No need to do this with Microsoft.Data.Sqlite because it will create the database when opening the connection if the database doesn't exist.
            }
        }

        public int CreateTable<T>(SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            return CreateTable<T>(connection, transaction, disposeConnectionAfterExecute, null);
        }

        public int CreateTable<T>(SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute, Dictionary<string, Type> extraColumns) where T : class
        {
            Type type = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = new MicrosoftSqliteDatabaseTableCore(type.Name, _settings.ConnectionString);
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

        public int DropTable<T>(SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            Type type = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = new MicrosoftSqliteDatabaseTableCore(type.Name, _settings.ConnectionString);
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

        protected void CreateTableIndexers(List<MicrosoftSqliteIndexerCore> indexers, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute)
        {
            foreach (MicrosoftSqliteIndexerCore index in indexers)
            {
                string sqlScript = index.GetCreateSqlScript();
                ExecuteNonQuery(sqlScript, connection, transaction, disposeConnectionAfterExecute);
            }
        }

        #endregion //Schema Methods

        #region Execute Methods

        public int ExecuteNonQuery(string sqlQueryString, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute)
        {
            int result = -1;
            try
            {
                if (connection == null)
                {
                    connection = new SqliteConnection(_settings.ConnectionString);
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqliteCommand command = new SqliteCommand(sqlQueryString, connection))
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
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Update(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Update<T>(T entity, string columnName, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
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
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Delete(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Delete<T>(T entity, string columnName, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we cannot delete records from it.
            }
            resultCode = table.Delete(entity, columnName, disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }


        public int DeleteAll<T>(SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute)
        {
            int resultCode;
            Type entityType = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
            resultCode = table.DeleteAll(disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }

        public int Save<T>(T entity, string columnName) where T : class
        {
            int resultCode;
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Save(entity, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Save<T>(T e, string columnName, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
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
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    Save(entities, columnName, connection, transaction, false);
                    transaction.Commit();
                }
            }
        }

        public void Save<T>(List<T> entities, string columnName, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            foreach (T e in entities)
            {
                Save<T>(e, columnName, connection, transaction, disposeConnectionAfterExecute);
            }
        }

        public int Insert<T>(T entity) where T : class
        {
            int resultCode;
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    resultCode = Insert(entity, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return resultCode;
        }

        public int Insert<T>(T entity, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            int resultCode;
            Type entityType = typeof(T);
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
            if (table == null)
            {
                return 0; //If the table doesn't exist we cannot insert records to it.
            }
            resultCode = table.Insert(entity, disposeConnectionAfterExecute, connection, transaction);
            return resultCode;
        }

        public void Insert<T>(List<T> entities) where T : class
        {
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    Insert(entities, connection, transaction, false);
                    transaction.Commit();
                }
            }
        }

        public void Insert<T>(List<T> entities, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
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
            using (SqliteConnection connection = new SqliteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    entities = Read<T>(columName, columnValue, connection, transaction, false);
                    transaction.Commit();
                }
            }
            return entities;
        }

        public List<T> Read<T>(string columName, object columnValue, SqliteConnection connection, SqliteTransaction transaction, bool disposeConnectionAfterExecute) where T : class
        {
            Type entityType = typeof(T);
            List<T> entities;
            MicrosoftSqliteDatabaseTableCore table = _sqliteDatabase.Tables[entityType.Name] as MicrosoftSqliteDatabaseTableCore;
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
