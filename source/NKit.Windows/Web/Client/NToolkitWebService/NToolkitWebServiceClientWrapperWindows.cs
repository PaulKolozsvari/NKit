﻿namespace NKit.Web.Client.NKitWebService
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Web.Client;
    using NKit.Data;
    using System.Data;
    using NKit.Data.DB;
    using System.Reflection.Emit;
    using NKit.Utilities;
    using NKit.Utilities.Logging;
    using System.Net;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    public class NKitWebServiceClientWrapperWindows
    {
        #region Constructors

        public NKitWebServiceClientWrapperWindows(IMimeWebServiceClient webServiceClient, int timeout)
        {
            if (webServiceClient == null)
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be set to null when constructing {1}.",
                    EntityReaderGeneric<NKitWebServiceClientWrapperWindows>.GetPropertyName(p => p.WebServiceClient, false),
                    this.GetType().FullName));
            }
            _webServiceClient = webServiceClient;
            _timeout = timeout;
        }

        #endregion //Constructors

        #region Fields

        protected IMimeWebServiceClient _webServiceClient;
        protected int _timeout;

        #endregion //Fields

        #region Properties

        public IMimeWebServiceClient WebServiceClient
        {
            get { return _webServiceClient; }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException(string.Format(
                        "{0} may not be set to null on {1}.",
                        EntityReaderGeneric<NKitWebServiceClientWrapperWindows>.GetPropertyName(p => p.WebServiceClient, false),
                        this.GetType().FullName));
                }
                _webServiceClient = value;
            }
        }

        #endregion //Properties

        #region Methods

        public void ConnectionTest(bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            ConnectionTest(
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public void ConnectionTest(
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException,
            Dictionary<string, string> requestHeaders)
        {
            _webServiceClient.ConnectionTest(
                _timeout, 
                out statusCode, 
                out statusDescription,
                wrapWebException,
                requestHeaders);
        }

        public T GetSqlSchema<T>(
            DatabaseSchemaInfoTypeWindows schemaInfoType,
            string filters,
            out string rawOutput,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return GetSqlSchema<T>(
                schemaInfoType,
                filters,
                out rawOutput,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public T GetSqlSchema<T>(
            DatabaseSchemaInfoTypeWindows schemaInfoType, 
            string filters, 
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string queryString =
                string.IsNullOrEmpty(filters) ?
                string.Format("sqlschema/{0}", schemaInfoType.ToString()) :
                string.Format("sqlschema/{0}/{1}", schemaInfoType.ToString(), filters);
            return _webServiceClient.CallService<T>(
                queryString,
                null,
                HttpVerb.GET,
                out rawOutput,
                false,
                true,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public SqlDatabaseWindows GetSqlDatabase(
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool wrapWebException)
        {
            string rawOutput = string.Empty;
            SqlDatabaseWindows result = new SqlDatabaseWindows();
            HttpStatusCode statusCode;
            string statusDescription = null;

            result.Name = GetSqlSchema<string>(DatabaseSchemaInfoTypeWindows.DatabaseName, null, out rawOutput, out statusCode, out statusDescription, wrapWebException);
            DataTable tablesSchema = GetSqlSchema<DataTable>(DatabaseSchemaInfoTypeWindows.Tables, null, out rawOutput, out statusCode, out statusDescription, wrapWebException);
            Dictionary<string, DatabaseTableKeyColumnsWindows> tablesKeyColumns = new Dictionary<string, DatabaseTableKeyColumnsWindows>(); //Primary keys of all the tables.
            GetSqlSchema<List<DatabaseTableKeyColumnsWindows>>(DatabaseSchemaInfoTypeWindows.TableKeyColumns, null, out rawOutput, out statusCode, out statusDescription, wrapWebException).ForEach(p => tablesKeyColumns.Add(p.TableName, p));
            Dictionary<string, DatabaseTableForeignKeyColumnsWindows> tablesForeignKeyColumns = new Dictionary<string, DatabaseTableForeignKeyColumnsWindows>(); //Foreign keys of all tables.
            GetSqlSchema<List<DatabaseTableForeignKeyColumnsWindows>>(DatabaseSchemaInfoTypeWindows.TableForeignKeyColumns, null, out rawOutput, out statusCode, out statusDescription, wrapWebException).ForEach(p => tablesForeignKeyColumns.Add(p.TableName, p));
            foreach (DataRow t in tablesSchema.Rows)
            {
                SqlDatabaseTableWindows table = new SqlDatabaseTableWindows(t);
                if (table.IsSystemTable)
                {
                    continue;
                }
                if (!tablesKeyColumns.ContainsKey(table.TableName))
                {
                    throw new UserThrownException(string.Format("Could not find key columns for table {0}.", table.TableName), LoggingLevel.Minimum);
                }
                DatabaseTableKeyColumnsWindows tableKeys = tablesKeyColumns[table.TableName];
                DatabaseTableForeignKeyColumnsWindows tableForeignKeys = tablesForeignKeyColumns[table.TableName];
                if (result.Tables.Exists(table.TableName))
                {
                    throw new Exception(string.Format(
                        "{0} with name {1} already added.",
                        typeof(SqlDatabaseTableWindows).FullName,
                        table.TableName));
                }
                result.AddTable(table);
                DataTable columnsSchema = GetSqlSchema<DataTable>(DatabaseSchemaInfoTypeWindows.Columns, table.TableName, out rawOutput, out statusCode, out statusDescription, wrapWebException);
                table.PopulateColumnsFromSchema(columnsSchema);
                foreach (string keyColumn in tableKeys.KeyNames)
                {
                    if (!table.Columns.Exists(keyColumn))
                    {
                        throw new UserThrownException(string.Format("Could not find key column {0} on table {1}.", keyColumn, table.TableName), LoggingLevel.Minimum);
                    }
                    table.Columns[keyColumn].IsKey = true;
                }
                foreach (ForeignKeyInfoWindows foreignKeyColumn in tableForeignKeys.ForeignKeys)
                {
                    if (!table.Columns.Exists(foreignKeyColumn.ChildTableForeignKeyName))
                    {
                        throw new UserThrownException(string.Format("Could not find foreign key column {0} on table {1}.", foreignKeyColumn.ChildTableForeignKeyName, table.TableName), LoggingLevel.Minimum);
                    }
                    DatabaseTableColumnWindows c = table.Columns[foreignKeyColumn.ChildTableForeignKeyName];
                    c.IsForeignKey = true;
                    c.ParentTableName = foreignKeyColumn.ParentTableName;
                    c.ParentTablePrimaryKeyName = foreignKeyColumn.ParentTablePrimaryKeyName;
                    c.ConstraintName = foreignKeyColumn.ConstraintName;
                }
            }
            PopulateSqlDatabaseChildrenTables(result);
            if (createOrmAssembly)
            {
                result.CreateOrmAssembly(saveOrmAssembly, ormAssemblyOutputDirectory);
            }
            return result;
        }

        private void PopulateSqlDatabaseChildrenTables(SqlDatabaseWindows database)
        {
            foreach (SqlDatabaseTableWindows pkTable in database.Tables)
            {
                foreach (SqlDatabaseTableWindows fkTable in database.Tables) //Find children tables i.e. tables that have foreign keys mapped this table's primary keys'.
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

        public T Query<T>(
            NKitWebServiceFilterStringWindows filterString,
            out string rawOutput)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return Query<T>(
                filterString,
                out rawOutput);
        }

        public T Query<T>(
            NKitWebServiceFilterStringWindows filterString,
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            return _webServiceClient.CallService<T>(
                filterString.ToString(),
                null,
                HttpVerb.GET,
                out rawOutput,
                false,
                true,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public object Query(
            Type returnType,
            NKitWebServiceFilterStringWindows filterString,
            out string rawOutput,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription = null;
            return Query(
                returnType,
                filterString,
                out rawOutput,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public object Query(
            Type returnType,
            NKitWebServiceFilterStringWindows filterString,
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            return _webServiceClient.CallService(
                returnType,
                filterString.ToString(),
                null,
                HttpVerb.GET,
                out rawOutput,
                false,
                true,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public string ExecuteSqlScript(
            string sqlScript,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription = null;
            return ExecuteSqlScript(
                sqlScript,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public string ExecuteSqlScript(
            string sqlScript, 
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string rawOutput = null;
            string queryString = string.Format("sql");
            _webServiceClient.CallService<string>(
                queryString,
                sqlScript,
                HttpVerb.PUT,
                out rawOutput,
                false,
                false,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
            return rawOutput;
        }

        public T ExecuteSqlQuery<T>(
            string sqlQuery,
            string entityTypeName,
            out string rawOutput,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return ExecuteSqlQuery<T>(
                sqlQuery,
                entityTypeName,
                out rawOutput,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public T ExecuteSqlQuery<T>(
            string sqlQuery,
            string entityTypeName,
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string queryString = string.Format("sql/{0}", entityTypeName);
            return _webServiceClient.CallService<T>(
                queryString,
                sqlQuery,
                HttpVerb.PUT,
                out rawOutput,
                false,
                true,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public object ExecuteSqlQuery(
            Type returnType,
            string sqlQuery,
            string entityTypeName,
            out string rawOutput,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return ExecuteSqlQuery(
                returnType,
                sqlQuery,
                entityTypeName,
                out rawOutput,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public object ExecuteSqlQuery(
            Type returnType,
            string sqlQuery,
            string entityTypeName,
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string queryString = string.Format("sql/{0}", entityTypeName);
            return _webServiceClient.CallService(
                returnType,
                queryString,
                sqlQuery,
                HttpVerb.PUT,
                out rawOutput,
                false,
                true,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        public string Insert(
            NKitWebServiceFilterStringWindows filterString,
            object requestPostObject,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return Insert(
                filterString,
                requestPostObject,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public string Insert(
            NKitWebServiceFilterStringWindows filterString,
            object requestPostObject,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string rawOutput = null;
            _webServiceClient.CallService<string>(
                filterString.ToString(),
                requestPostObject,
                HttpVerb.POST,
                out rawOutput,
                true,
                false,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
            return rawOutput;
        }

        public string Update(
            NKitWebServiceFilterStringWindows filterString,
            string columnName,
            object requestPostObject,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return Update(
                filterString,
                columnName,
                requestPostObject,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public string Update(
            NKitWebServiceFilterStringWindows filterString,
            string columnName,
            object requestPostObject,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string rawOutput = null;
            string queryString = 
                string.IsNullOrEmpty(columnName) ?
                filterString.ToString() :
                string.Format("{0}/{1}", filterString, columnName);
            _webServiceClient.CallService<string>(
                queryString,
                requestPostObject,
                HttpVerb.PUT,
                out rawOutput,
                true,
                false,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
            return rawOutput;
        }

        public string Delete(
            NKitWebServiceFilterStringWindows filterString,
            string columnName,
            object requestPostObject,
            bool wrapWebException)
        {
            HttpStatusCode statusCode;
            string statusDescription;
            return Delete(
                filterString,
                columnName,
                requestPostObject,
                out statusCode,
                out statusDescription,
                wrapWebException);
        }

        public string Delete(
            NKitWebServiceFilterStringWindows filterString,
            string columnName,
            object requestPostObject,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string rawOutput = null;
            string queryString =
                string.IsNullOrEmpty(columnName) ?
                filterString.ToString() :
                string.Format("{0}/{1}", filterString, columnName);
            _webServiceClient.CallService<string>(
                queryString,
                requestPostObject,
                HttpVerb.DELETE,
                out rawOutput,
                true,
                false,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
            return rawOutput;
        }

        public T CallExtension<T>(
            string handlerName,
            string customParameters,
            object requestPostObject,
            bool serializePostObject,
            bool deserailizeToDotNetObject,
            out string rawOutput,
            out HttpStatusCode statusCode,
            out string statusDescription,
            bool wrapWebException)
        {
            string queryString =
                string.IsNullOrEmpty(customParameters) ?
                string.Format("extension/{0}", handlerName) :
                string.Format("extension/{0}/{1}", handlerName, customParameters);
            return _webServiceClient.CallService<T>(
                queryString,
                requestPostObject,
                HttpVerb.PUT,
                out rawOutput,
                serializePostObject,
                deserailizeToDotNetObject,
                _timeout,
                out statusCode,
                out statusDescription,
                wrapWebException,
                null);
        }

        #endregion //Methods
    }
}