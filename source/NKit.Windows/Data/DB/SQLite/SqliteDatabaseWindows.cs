﻿namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Data;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Data.Common;
    using SQLite;
    using NKit.Data.DB.SQLQuery;
    using System.Data.SQLite;
    using NKit.Data.ORM;

    #endregion //Using Directives

    [Serializable]
    public class SqliteDatabaseWindows : DatabaseWindows
    {
        #region Constructors

        public SqliteDatabaseWindows()
        {
        }

        public SqliteDatabaseWindows(
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            bool copyOrmAssembly,
            string ormAssemblyCopyDirectory,
            bool overrideNameWithDatabaseNameFromSchema)
            : base(
            connectionString,
            populateTablesFromSchema,
            createOrmAssembly,
            saveOrmAssembly,
            copyOrmAssembly,
            ormAssemblyCopyDirectory,
            overrideNameWithDatabaseNameFromSchema)
        {
        }

        public SqliteDatabaseWindows(
            string name,
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool overrideNameWithDatabaseNameFromSchema)
            : base(
            name,
            connectionString,
            populateTablesFromSchema,
            createOrmAssembly,
            saveOrmAssembly,
            ormAssemblyOutputDirectory,
            overrideNameWithDatabaseNameFromSchema)
        {
        }

        #endregion //Constructors

        #region Constants

        public const string DATABASE_NAME_SCHEMA_ATTRIBUTE = "database_name";

        #endregion //Constants

        #region Methods

        public override void Initialize(
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool overrideNameWithDatabaseNameFromSchema)
        {
            _tables = new EntityCacheGeneric<string, DatabaseTableWindows>();
            _connectionString = connectionString;
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                PublishFeedback(string.Format("Opening connection to {0} ...", _connectionString));
                connection.Open();
                if (overrideNameWithDatabaseNameFromSchema)
                {
                    PublishFeedback("Getting DB name from schema ...");
                    _name = GetDatabaseNameFromSchema(connection, false);
                }
                if (populateTablesFromSchema)
                {
                    PublishFeedback("Populating DB tables from schema ...");
                    PopulateTablesFromSchema(true, connection, false);
                }
                if (createOrmAssembly)
                {
                    PublishFeedback("Generating ORM assembly ...");
                    CreateOrmAssembly(saveOrmAssembly, ormAssemblyOutputDirectory);
                }
            }
        }

        public SqliteDatabaseTableGenericWindows<E> GetSqlDatabaseTable<E>() where E : class
        {
            return GetSqlDatabaseTable<E>(typeof(E).Name);
        }

        public SqliteDatabaseTableGenericWindows<E> GetSqlDatabaseTable<E>(string tableName) where E : class
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            SqliteDatabaseTableGenericWindows<E> result = _tables[tableName] as SqliteDatabaseTableGenericWindows<E>;
            if (result == null)
            {
                throw new InvalidCastException(string.Format(
                    "Unexpected table type in {0}. Could not type cast {1} to a {2}.",
                    this.GetType().FullName,
                    typeof(DatabaseWindows).FullName,
                    typeof(SqliteDatabaseTableGenericWindows<E>).FullName));
            }
            return result;
        }

        public void AddTable(DatabaseTableWindows table)
        {
            _tables.Add(table);
        }

        public SqliteDatabaseTableGenericWindows<E> AddTable<E>() where E : class
        {
            return AddTable<E>(typeof(E).Name);
        }

        public SqliteDatabaseTableGenericWindows<E> AddTable<E>(string tableName) where E : class
        {
            if (_tables.Exists(tableName))
            {
                throw new Exception(string.Format(
                    "{0} with name {1} already added to {2}.",
                    typeof(SqliteDatabaseTableGenericWindows<E>).FullName,
                    tableName,
                    this.GetType().FullName));
            }
            SqliteDatabaseTableGenericWindows<E> table = new SqliteDatabaseTableGenericWindows<E>(tableName, _connectionString);
            _tables.Add(table);
            return table;
        }

        public override void Dispose()
        {
            if (!string.IsNullOrEmpty(_ormAssembly.AssemblyFilePath) &&
                File.Exists(_ormAssembly.AssemblyFilePath))
            {
                File.Delete(_ormAssembly.AssemblyFilePath); //Delete the ORM assembly from the output directory.
            }
        }

        public override List<object> Query(
            string sqlQueryString,
            OrmAssemblySqlWindows ormCollectibleAssembly,
            string typeName,
            string propertyNameFilter,
            out OrmTypeWindows ormCollecibleType)
        {
            if (ormCollectibleAssembly == null)
            {
                throw new NullReferenceException(string.Format("ormCollectibleAssembly may not be null."));
            }
            if (ormCollectibleAssembly.AssemblyBuilderAccess != AssemblyBuilderAccess.RunAndCollect)
            {
                throw new ArgumentException(string.Format(
                    "Querying the database with a raw SQL query string requires an {0} with the {1} property set to {2}.",
                    typeof(OrmAssemblyWindows).FullName,
                    EntityReaderGeneric<OrmAssemblyWindows>.GetPropertyName(p => p.AssemblyBuilderAccess, false),
                    AssemblyBuilderAccess.RunAndCollect));
            }
            List<object> result = null;
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(sqlQueryString, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        ormCollecibleType = ormCollectibleAssembly.CreateOrmTypeFromSqlDataReader(typeName, reader, true);
                        result = DataHelperWindows.ParseReaderToEntities(reader, ormCollecibleType.DotNetType, propertyNameFilter);
                    }
                }
            }
            return result;
        }

        public override List<object> Query(QueryWindows query, string propertyNameFilter, Type entityType)
        {
            List<DatabaseTableWindows> tablesMentioned = GetTablesMentionedInQuery(query);
            if (tablesMentioned.Count < 1)
            {
                return new List<object>();
            }
            List<object> result = null;
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(query.SqlQueryString, connection))
                {
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    command.CommandType = System.Data.CommandType.Text;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        result = DataHelperWindows.ParseReaderToEntities(reader, entityType, propertyNameFilter);
                    }
                }
            }
            return result;
        }

        public bool TableExistsInDatabase(
            string tableName,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            bool result = false;
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(_connectionString);
                }
                if (connection == null)
                {
                    throw new Exception($"Could not create connection to Sqlite: {_connectionString}");
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (connection.State != ConnectionState.Open)
                {
                    throw new Exception($"Could not open connection to Sqlite: {_connectionString}");
                }
                string sqlQuery = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
                using (SQLiteCommand command = new SQLiteCommand(sqlQuery, (SQLiteConnection)connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = (SQLiteTransaction)transaction;
                    }
                    command.CommandType = System.Data.CommandType.Text;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        result = reader != null ? reader.Read() : false;
                    }
                }
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &&
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
            return result;
        }

        public override List<object> Query(
            QueryWindows query,
            string propertyNameFilter,
            Type entityType, 
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            List<object> result = null;
            try
            {
                List<DatabaseTableWindows> tablesMentioned = GetTablesMentionedInQuery(query);
                if (tablesMentioned.Count < 1)
                {
                    return new List<object>();
                }
                if (connection == null)
                {
                    connection = new SQLiteConnection(_connectionString);
                }
                if (connection == null)
                {
                    throw new Exception($"Could not create connection to Sqlite: {_connectionString}");
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (connection.State != ConnectionState.Open)
                {
                    throw new Exception($"Could not open connection to Sqlite: {_connectionString}");
                }
                using (SQLiteCommand command = new SQLiteCommand(query.SqlQueryString, (SQLiteConnection)connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = (SQLiteTransaction)transaction;
                    }
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    command.CommandType = System.Data.CommandType.Text;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        result = DataHelperWindows.ParseReaderToEntities(reader, entityType, propertyNameFilter);
                    }
                }
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &&
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
            return result;
        }

        public List<E> Query<E>(
            string columnName,
            object columnValue,
            string propertyNameFilter,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction) where E : class
        {
            SqliteDatabaseTableGenericWindows<E> table = GetSqlDatabaseTable<E>();
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableWindows).FullName,
                    typeof(E).Name));
            }
            List<object> queryResults = Query(columnName, columnValue, propertyNameFilter, typeof(E), disposeConnectionAfterExecute, connection, transaction);
            List<E> results = new List<E>();
            queryResults.ForEach(p => results.Add((E)p));
            return results;
        }

        public List<E> Query<E>(
            string columnName,
            object columnValue,
            string tableName,
            string propertyNameFilter,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction) where E : class
        {
            SqliteDatabaseTableGenericWindows<E> table = GetSqlDatabaseTable<E>();
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableWindows).FullName,
                    typeof(E).Name));
            }
            List<object> queryResults = Query(columnName, columnValue, tableName, propertyNameFilter, typeof(E), disposeConnectionAfterExecute, connection, transaction);
            List<E> results = new List<E>();
            queryResults.ForEach(p => results.Add((E)p));
            return results;
        }

        public int ExecuteNonQuery(QueryWindows query)
        {
            int result = -1;
            List<DatabaseTableWindows> tablesMentioned = GetTablesMentionedInQuery(query);
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(query.SqlQueryString, connection))
                {
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        public DataTable GetSchema()
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Close();
                return connection.GetSchema();
            }
        }

        public override string GetDatabaseNameFromSchema(
            DbConnection connection,
            bool disposeConnectionAfterExecute)
        {
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(_connectionString);
                }
                if (connection == null)
                {
                    throw new Exception($"Could not create connection to Sqlite: {_connectionString}");
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                DataTable schema = connection.GetSchema("Databases");
                foreach (DataRow row in schema.Rows)
                {
                    string databaseName = row[DATABASE_NAME_SCHEMA_ATTRIBUTE].ToString();
                    if (_connectionString.Contains(databaseName))
                    {
                        return databaseName;
                    }
                }
                throw new Exception(string.Format(
                    "No database with exists on the server with a name as mentioned in the connection string {0}.",
                    _connectionString));
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &&
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
        }

        public override DataTable GetRawTablesSchema(
            bool includeColumns,
            DbConnection connection,
            bool disposeConnectionAfterExecute)
        {
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(_connectionString);
                }
                if (connection == null)
                {
                    throw new Exception($"Could not create connection to Sqlite: {_connectionString}");
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (connection.State != ConnectionState.Open)
                {
                    throw new Exception($"Could not open connection to Sqlite: {_connectionString}");
                }
                return connection.GetSchema("Tables");
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &&
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/library/ms254969.aspx
        /// http://www.devart.com/dotconnect/salesforce/docs/Metadata-GetSchema.html
        /// </summary>
        /// <param name="includeColumns"></param>
        public override void PopulateTablesFromSchema(
            bool includeColumns,
            DbConnection connection,
            bool disposeConnectionAfterExecute)
        {
            ///TODO Look at using SQL MSO (Server Management Objects) http://msdn.microsoft.com/en-us/magazine/cc163409.aspx
            try
            {
                if (connection == null)
                {
                    connection = new SQLiteConnection(_connectionString);
                }
                if (connection == null)
                {
                    throw new Exception($"Could not create connection to Sqlite: {_connectionString}");
                }
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                if (connection.State != ConnectionState.Open)
                {
                    throw new Exception($"Could not open connection to Sqlite: {_connectionString}");
                }
                DataTable schema = connection.GetSchema("Tables");
                _tables.Clear();
                foreach (DataRow row in schema.Rows)
                {
                    SqliteDatabaseTableGenericWindows<object> table = new SqliteDatabaseTableGenericWindows<object>(row, _connectionString);
                    if (table.IsSystemTable)
                    {
                        continue;
                    }
                    if (_tables.Exists(table.TableName))
                    {
                        throw new Exception(string.Format(
                            "{0} with name {1} already added to {2}.",
                            typeof(SqliteDatabaseTableGenericWindows<object>).FullName,
                            table.TableName,
                            this.GetType().FullName));
                    };
                    _tables.Add(table.TableName, table);
                }
                if (includeColumns)
                {
                    _tables.ToList().ForEach(t => t.PopulateColumnsFromSchema(connection, false));
                }
                PopulateChildrenTables();
            }
            finally
            {
                if (disposeConnectionAfterExecute &&
                    connection != null &&
                    connection.State != ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }
        }

        private void PopulateChildrenTables()
        {
            foreach (DatabaseTableWindows pkTable in _tables)
            {
                foreach (DatabaseTableWindows fkTable in _tables) //Find children tables i.e. tables that have foreign keys mapped this table's primary keys'.
                {
                    EntityCacheGeneric<string, ForeignKeyInfoWindows> mappedForeignKeys = new EntityCacheGeneric<string, ForeignKeyInfoWindows>();
                    fkTable.GetForeignKeyColumns().Where(c => c.ParentTableName == pkTable.TableName).ToList().ForEach(fk => mappedForeignKeys.Add(fk.ColumnName, new ForeignKeyInfoWindows()
                    {
                        ChildTableName = fkTable.TableName,
                        ChildTableForeignKeyName = fk.ColumnName,
                        ParentTableName = fk.ParentTableName,
                        ParentTablePrimaryKeyName = fk.ParentTablePrimaryKeyName,
                        ConstraintName = fk.ConstraintName

                    }));
                    if (mappedForeignKeys.Count > 0) //If there are any foreign keys mapped to parent table's name.
                    {
                        pkTable.ChildrenTables.Add(fkTable.TableName, mappedForeignKeys);
                    }
                }
            }
        }

        public override List<DatabaseTableKeyColumnsWindows> GetTableKeyColumns()
        {
            List<DatabaseTableKeyColumnsWindows> result = new List<DatabaseTableKeyColumnsWindows>();
            foreach (SqliteDatabaseTableWindows t in _tables)
            {
                DatabaseTableKeyColumnsWindows tableKeyColumns = new DatabaseTableKeyColumnsWindows(t.TableName);
                foreach (SqliteDatabaseTableColumnWindows c in t.Columns)
                {
                    if (c.IsKey)
                    {
                        tableKeyColumns.KeyNames.Add(c.ColumnName);
                    }
                }
                result.Add(tableKeyColumns);
            }
            return result;
        }

        public override List<DatabaseTableForeignKeyColumnsWindows> GetTableForeignKeyColumns()
        {
            List<DatabaseTableForeignKeyColumnsWindows> result = new List<DatabaseTableForeignKeyColumnsWindows>();
            foreach (SqliteDatabaseTableWindows t in _tables)
            {
                DatabaseTableForeignKeyColumnsWindows foreignKeyColumns = new DatabaseTableForeignKeyColumnsWindows(t.TableName);
                t.GetForeignKeyColumns().ToList().ForEach(c => foreignKeyColumns.ForeignKeys.Add(new ForeignKeyInfoWindows()
                {
                    ChildTableName = t.TableName,
                    ChildTableForeignKeyName = c.ColumnName,
                    ParentTableName = c.ParentTableName,
                    ParentTablePrimaryKeyName = c.ParentTablePrimaryKeyName,
                    ConstraintName = c.ConstraintName
                }));
                result.Add(foreignKeyColumns);
            }
            return result;
        }

        #endregion //Methods
    }
}