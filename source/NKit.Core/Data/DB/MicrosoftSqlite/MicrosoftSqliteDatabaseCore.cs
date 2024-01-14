namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using System.Linq;
    using System.Reflection.Emit;
    using NKit.Data.DB.SQLQuery;
    using NKit.Data.ORM;
    using Microsoft.Data.Sqlite;
    using System.IO;
    using System.Xml.Linq;
    using System.Data.SqlClient;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using System.ComponentModel.DataAnnotations;

    #endregion //Using Directives

    [Serializable]
    public class MicrosoftSqliteDatabaseCore : DatabaseCore
    {
        #region Constructors

        public MicrosoftSqliteDatabaseCore()
        {
        }

        public MicrosoftSqliteDatabaseCore(
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

        public MicrosoftSqliteDatabaseCore(
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
            _tables = new EntityCacheGeneric<string, DatabaseTableCore>();
            _connectionString = connectionString;
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
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

        public MicrosoftSqliteDatabaseTableGenericCore<E> GetSqlDatabaseTable<E>() where E : class
        {
            return GetSqlDatabaseTable<E>(typeof(E).Name);
        }

        public MicrosoftSqliteDatabaseTableGenericCore<E> GetSqlDatabaseTable<E>(string tableName) where E : class
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            MicrosoftSqliteDatabaseTableGenericCore<E> result = _tables[tableName] as MicrosoftSqliteDatabaseTableGenericCore<E>;
            if (result == null)
            {
                throw new InvalidCastException(string.Format(
                    "Unexpected table type in {0}. Could not type cast {1} to a {2}.",
                    this.GetType().FullName,
                    typeof(DatabaseCore).FullName,
                    typeof(MicrosoftSqliteDatabaseTableGenericCore<E>).FullName));
            }
            return result;
        }

        public void AddTable(DatabaseTableCore table)
        {
            _tables.Add(table);
        }

        public MicrosoftSqliteDatabaseTableGenericCore<E> AddTable<E>() where E : class
        {
            return AddTable<E>(typeof(E).Name);
        }

        public MicrosoftSqliteDatabaseTableGenericCore<E> AddTable<E>(string tableName) where E : class
        {
            if (_tables.Exists(tableName))
            {
                throw new Exception(string.Format(
                    "{0} with name {1} already added to {2}.",
                    typeof(MicrosoftSqliteDatabaseTableGenericCore<E>).FullName,
                    tableName,
                    this.GetType().FullName));
            }
            MicrosoftSqliteDatabaseTableGenericCore<E> table = new MicrosoftSqliteDatabaseTableGenericCore<E>(tableName, _connectionString);
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
            OrmAssemblySqlCore ormCollectibleAssembly,
            string typeName,
            string propertyNameFilter,
            out OrmTypeCore ormCollecibleType)
        {
            if (ormCollectibleAssembly == null)
            {
                throw new NullReferenceException(string.Format("ormCollectibleAssembly may not be null."));
            }
            if (ormCollectibleAssembly.AssemblyBuilderAccess != AssemblyBuilderAccess.RunAndCollect)
            {
                throw new ArgumentException(string.Format(
                    "Querying the database with a raw SQL query string requires an {0} with the {1} property set to {2}.",
                    typeof(OrmAssemblyCore).FullName,
                    EntityReaderGeneric<OrmAssemblyCore>.GetPropertyName(p => p.AssemblyBuilderAccess, false),
                    AssemblyBuilderAccess.RunAndCollect));
            }
            List<object> result = null;
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand(sqlQueryString, connection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        ormCollecibleType = ormCollectibleAssembly.CreateOrmTypeFromSqlDataReader(typeName, reader, true);
                        result = DataHelperCore.ParseReaderToEntities(reader, ormCollecibleType.DotNetType, propertyNameFilter);
                    }
                }
            }
            return result;
        }

        public override List<object> Query(QueryCore query, string propertyNameFilter, Type entityType)
        {
            List<DatabaseTableCore> tablesMentioned = GetTablesMentionedInQuery(query);
            if (tablesMentioned.Count < 1)
            {
                return new List<object>();
            }
            List<object> result = null;
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand(query.SqlQueryString, connection))
                {
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    command.CommandType = System.Data.CommandType.Text;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        result = DataHelperCore.ParseReaderToEntities(reader, entityType, propertyNameFilter);
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
                    connection = new SqliteConnection(_connectionString);
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
                using (SqliteCommand command = new SqliteCommand(sqlQuery, (SqliteConnection)connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = (SqliteTransaction)transaction;
                    }
                    command.CommandType = System.Data.CommandType.Text;
                    using (SqliteDataReader reader = command.ExecuteReader())
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
            QueryCore query,
            string propertyNameFilter,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            List<object> result = null;
            try
            {
                List<DatabaseTableCore> tablesMentioned = GetTablesMentionedInQuery(query);
                if (tablesMentioned.Count < 1)
                {
                    return new List<object>();
                }
                if (connection == null)
                {
                    connection = new SqliteConnection(_connectionString);
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
                using (SqliteCommand command = new SqliteCommand(query.SqlQueryString, (SqliteConnection)connection))
                {
                    if (transaction != null)
                    {
                        command.Transaction = (SqliteTransaction)transaction;
                    }
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    command.CommandType = System.Data.CommandType.Text;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        result = DataHelperCore.ParseReaderToEntities(reader, entityType, propertyNameFilter);
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
            MicrosoftSqliteDatabaseTableGenericCore<E> table = GetSqlDatabaseTable<E>();
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableCore).FullName,
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
            MicrosoftSqliteDatabaseTableGenericCore<E> table = GetSqlDatabaseTable<E>();
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableCore).FullName,
                    typeof(E).Name));
            }
            List<object> queryResults = Query(columnName, columnValue, tableName, propertyNameFilter, typeof(E), disposeConnectionAfterExecute, connection, transaction);
            List<E> results = new List<E>();
            queryResults.ForEach(p => results.Add((E)p));
            return results;
        }

        public int ExecuteNonQuery(QueryCore query)
        {
            int result = -1;
            List<DatabaseTableCore> tablesMentioned = GetTablesMentionedInQuery(query);
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (SqliteCommand command = new SqliteCommand(query.SqlQueryString, connection))
                {
                    query.SqlParameters.ForEach(p => command.Parameters.Add(p));
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        public DataTable GetSchema()
        {
            using (SqliteConnection connection = new SqliteConnection(_connectionString))
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
                    connection = new SqliteConnection(_connectionString);
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
                    connection = new SqliteConnection(_connectionString);
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
                    connection = new SqliteConnection(_connectionString);
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
                /*
                 * GetSchema: Not implemented on Microsoft.Data.Sqlite: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/adonet-limitations
                 * https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/metadata
                 */
                //DataTable schema = connection.GetSchema("Tables");
                Dictionary<string, List<MicrosoftSqliteDatabaseTableColumnCore>> databaseSchema = GetSqliteDatabaseSchema(connection);
                _tables.Clear();
                foreach (string tableName in databaseSchema.Keys)
                {
                    MicrosoftSqliteDatabaseTableGenericCore<object> table = new MicrosoftSqliteDatabaseTableGenericCore<object>(tableName, _connectionString);
                    if (table.IsSystemTable)
                    {
                        continue;
                    }
                    if (_tables.Exists(table.TableName))
                    {
                        throw new Exception(string.Format(
                            "{0} with name {1} already added to {2}.",
                            typeof(MicrosoftSqliteDatabaseTableGenericCore<object>).FullName,
                            table.TableName,
                            this.GetType().FullName));
                    };
                    if (includeColumns)
                    {
                        foreach (MicrosoftSqliteDatabaseTableColumnCore column in databaseSchema[tableName])
                        {
                            table.Columns.Add(column);
                        }
                    }
                    _tables.Add(table.TableName, table);
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

        private Dictionary<string, List<MicrosoftSqliteDatabaseTableColumnCore>> GetSqliteDatabaseSchema(DbConnection connection)
        {
            //This query will retrieve metadata about all the columns in the database: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/metadata
            string sqlQueryString = @"SELECT t.name AS tbl_name, c.name, c.cid, c.type, c.dflt_value, c.pk FROM sqlite_master AS t, pragma_table_info(t.name) AS c WHERE t.type = 'table';";
            Dictionary<string, List<MicrosoftSqliteDatabaseTableColumnCore>> result = new Dictionary<string, List<MicrosoftSqliteDatabaseTableColumnCore>>();
            connection.Open();
            using (SqliteCommand command = new SqliteCommand(sqlQueryString, (SqliteConnection)connection))
            {
                command.CommandType = System.Data.CommandType.Text;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);
                            string columnName = reader.GetString(1);
                            short ordinalPosition = reader.GetInt16(2);
                            string sqlTypeName = reader.GetString(3);
                            DbType sqlDbType = MicrosoftSqliteTypeConverterCore.Instance.GetSqlDbType(sqlTypeName);
                            SqliteType microsoftSqliteType = MicrosoftSqliteTypeConverterCore.Instance.GetMicrosoftSqliteType(sqlTypeName);
                            if (!result.ContainsKey(tableName))
                            {
                                result.Add(tableName, null);
                                result[tableName] = new List<MicrosoftSqliteDatabaseTableColumnCore>();
                            }
                            MicrosoftSqliteDatabaseTableColumnCore column = new MicrosoftSqliteDatabaseTableColumnCore()
                            {
                                ParentTableName = tableName,
                                OrdinalPosition = ordinalPosition,
                                ColumnName = columnName,
                                SqlDbType = sqlDbType,
                                MicrosoftSqliteType = microsoftSqliteType,
                                ColumnDefault = null
                            };
                            result[tableName].Add(column);
                        }
                    }
                }
            }
            return result;
        }

        private void PopulateChildrenTables()
        {
            foreach (DatabaseTableCore pkTable in _tables)
            {
                foreach (DatabaseTableCore fkTable in _tables) //Find children tables i.e. tables that have foreign keys mapped this table's primary keys'.
                {
                    EntityCacheGeneric<string, ForeignKeyInfoCore> mappedForeignKeys = new EntityCacheGeneric<string, ForeignKeyInfoCore>();
                    fkTable.GetForeignKeyColumns().Where(c => c.ParentTableName == pkTable.TableName).ToList().ForEach(fk => mappedForeignKeys.Add(fk.ColumnName, new ForeignKeyInfoCore()
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

        public override List<DatabaseTableKeyColumnsCore> GetTableKeyColumns()
        {
            List<DatabaseTableKeyColumnsCore> result = new List<DatabaseTableKeyColumnsCore>();
            foreach (MicrosoftSqliteDatabaseTableCore t in _tables)
            {
                DatabaseTableKeyColumnsCore tableKeyColumns = new DatabaseTableKeyColumnsCore(t.TableName);
                foreach (MicrosoftSqliteDatabaseTableColumnCore c in t.Columns)
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

        public override List<DatabaseTableForeignKeyColumnsCore> GetTableForeignKeyColumns()
        {
            List<DatabaseTableForeignKeyColumnsCore> result = new List<DatabaseTableForeignKeyColumnsCore>();
            foreach (MicrosoftSqliteDatabaseTableCore t in _tables)
            {
                DatabaseTableForeignKeyColumnsCore foreignKeyColumns = new DatabaseTableForeignKeyColumnsCore(t.TableName);
                t.GetForeignKeyColumns().ToList().ForEach(c => foreignKeyColumns.ForeignKeys.Add(new ForeignKeyInfoCore()
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
