﻿namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;

    #endregion //Using Directives

    /// <summary>
    /// A wrapper used to perform CRUD operations on a Sqlite database based on entities.
    /// </summary>
    public class SqliteEntityContext
    {
        #region Constructors

        public SqliteEntityContext(SqliteSettings settings)
        {
            DataValidator.ValidateObjectNotNull(settings, nameof(settings), nameof(SqliteEntityContext));
            _settings = settings;
            CreateDatabaseFile(settings.DatabaseFilePath);
        }

        #endregion //Constructors

        #region Fields

        private SqliteSettings _settings;

        #endregion //Fields

        #region Properties

        public SqliteSettings Settings
        {
            get { return _settings; }
        }

        #endregion //Properties

        #region Schema Methods

        private void CreateDatabaseFile(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath))
            {
                SQLiteConnection.CreateFile(databaseFilePath);
            }
        }

        public int CreateTable<T>() where T : class
        {
            Type type = typeof(T);
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(type.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
            //int resultCode = ExecuteNonQuery(table.GetSqlDropTableScript());
            int resultCode = ExecuteNonQuery(table.GetSqlCreateTableScript());
            return resultCode;
        }

        public int DropTable<T>() where T : class
        {
            Type type = typeof(T);
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(type.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
            int resultCode = ExecuteNonQuery(table.GetSqlDropTableScript());
            return resultCode;
        }

        public void DeleteDatabaseIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        protected void CreateTableIndexers(List<SqliteIndexer> indexers)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                foreach (SqliteIndexer index in indexers)
                {
                    string sqlScript = index.GetCreateSqlScript();
                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand command = new SQLiteCommand(sqlScript, connection))
                        {
                            int result = command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        #endregion //Schema Methods

        #region Execute Methods

        public int ExecuteNonQuery(string sqlQueryString)
        {
            int result = -1;
            using (SQLiteConnection connection = new SQLiteConnection(_settings.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sqlQueryString, connection))
                {
                    result = command.ExecuteNonQuery();
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
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(entityType.Name, _settings.ConnectionString);
            resultCode = table.Update(entity, columnName, disposeConnectionAfterExecute, connection, transaction);
            transaction.Commit();
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
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(entityType.Name, _settings.ConnectionString);
            resultCode = table.Delete(entity, columnName, disposeConnectionAfterExecute, connection, transaction);
            transaction.Commit();
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
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(entityType.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
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
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(entityType.Name, _settings.ConnectionString);
            table.AddColumnsByEntityType<T>();
            resultCode = table.Insert(entity, disposeConnectionAfterExecute, connection, transaction);
            transaction.Commit();
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
            Type entityType = typeof(T);
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
            SqliteDatabaseTableWindows table = new SqliteDatabaseTableWindows(entityType.Name, _settings.ConnectionString);
            entities = table.Query<T>(columName, columnValue, string.Empty, disposeConnectionAfterExecute: false, connection, transaction);
            return entities;
        }

        #endregion //Crud Methods
    }
}
