namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using System.Reflection.Emit;
    using System.Text;
    using NKit.Data.ORM;
    using NKit.Data.DB.SQLQuery;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    [Serializable]
    public abstract class DatabaseCore
    {
        #region Inner Types

        public class OnDatabaseFeedbackEventArgs : EventArgs
        {
            #region Constructors

            public OnDatabaseFeedbackEventArgs(string feedbackInfo)
            {
                _feedbackInfo = feedbackInfo;
            }

            #region Fields

            private string _feedbackInfo;

            #endregion //Fields

            #endregion //Constructors

            #region Properties

            public string FeedbackInfo
            {
                get { return _feedbackInfo; }
            }

            #endregion //Properties
        }

        public delegate void OnDatabaseFeedbackHandler(object sender, OnDatabaseFeedbackEventArgs e);

        #endregion //Inner Types

        #region Constructors

        public DatabaseCore()
        {
            _tables = new EntityCacheGeneric<string, DatabaseTableCore>();
            _name = this.GetType().Name;
        }

        public DatabaseCore(
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            bool copyOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool overrideNameWithDatabaseNameFromSchema)
        {
            _name = this.GetType().Name;
            Initialize(
                connectionString,
                populateTablesFromSchema,
                createOrmAssembly,
                saveOrmAssembly,
                ormAssemblyOutputDirectory,
                overrideNameWithDatabaseNameFromSchema);
        }

        public DatabaseCore(
            string name,
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool overrideNameWithDatabaseNameFromSchema)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(string.Format(
                    "{0} not be null or empty when constructing {1}.",
                    EntityReaderGeneric<DatabaseCore>.GetPropertyName(p => p.Name, false),
                    this.GetType().FullName));
            }
            _name = name;
            Initialize(
                connectionString,
                populateTablesFromSchema,
                createOrmAssembly,
                saveOrmAssembly,
                ormAssemblyOutputDirectory,
                overrideNameWithDatabaseNameFromSchema);
        }

        #endregion //Constructors

        #region Events

        public event OnDatabaseFeedbackHandler OnDatabaseFeedback;

        #endregion //Events

        #region Fields

        protected string _name;
        protected string _connectionString;
        protected EntityCacheGeneric<string, DatabaseTableCore> _tables;
        protected OrmAssemblyCore _ormAssembly;

        #endregion //Fields

        #region Properties

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public EntityCacheGeneric<string, DatabaseTableCore> Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        #endregion //Properties

        #region Methods

        public OrmAssemblyCore GetOrmAssembly()
        {
            return _ormAssembly;
        }

        public override string ToString()
        {
            return _name;
        }

        public abstract void Initialize(
            string connectionString,
            bool populateTablesFromSchema,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory,
            bool overrideNameWithDatabaseNameFromSchema);

        protected void PublishFeedback(string feedback)
        {
            if (OnDatabaseFeedback != null)
            {
                OnDatabaseFeedback(this, new OnDatabaseFeedbackEventArgs(feedback));
            }
        }

        public abstract string GetDatabaseNameFromSchema(DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract DataTable GetRawTablesSchema(bool includeColumns, DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract void PopulateTablesFromSchema(bool includeColumns, DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract List<DatabaseTableKeyColumnsCore> GetTableKeyColumns();

        public abstract List<DatabaseTableForeignKeyColumnsCore> GetTableForeignKeyColumns();

        public abstract void Dispose();

        public virtual void ClearTables()
        {
            _tables.Clear();
        }

        public List<DatabaseTableCore> GetTablesMentionedInQuery(QueryCore query)
        {
            List<DatabaseTableCore> result = new List<DatabaseTableCore>();
            foreach (string t in query.TableNamesInQuery)
            {
                DatabaseTableCore table = _tables[t];
                if (table != null)
                {
                    result.Add(table);
                }
            }
            return result;
        }

        public virtual DatabaseTableCore GetDatabaseTable(Type entityType)
        {
            return GetDatabaseTable(entityType.Name);
        }

        public virtual DatabaseTableCore GetDatabaseTable(string tableName)
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            return (DatabaseTableCore)_tables[tableName];
        }

        public void CreateOrmAssembly(
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory)
        {
            string assemblyFileName = string.Format("{0}.dll", this.Name);
            _ormAssembly = new OrmAssemblyCore(
                this.Name,
                assemblyFileName,
                AssemblyBuilderAccess.Run);
            foreach (DatabaseTableCore table in _tables)
            {
                OrmTypeCore ormType = _ormAssembly.CreateOrmType(table.TableName, true);
                PublishFeedback(string.Format("Created type {0}.", ormType.TypeName));
                foreach (DatabaseTableColumnCore column in table.Columns)
                {
                    ormType.CreateOrmProperty(
                        column.ColumnName,
                        SqlTypeConverterCore.Instance.GetDotNetType(column.DataType, column.IsNullable));
                }
                table.MappedType = ormType.CreateType();
            }
            if (!saveOrmAssembly)
            {
                return;
            }
            _ormAssembly.Save(ormAssemblyOutputDirectory);
        }

        public virtual List<object> Query(
            string columnName,
            object columnValue,
            string propertyNameFilter,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            return Query(columnName, columnValue, entityType.Name, propertyNameFilter, entityType, disposeConnectionAfterExecute, connection, transaction);
        }

        public abstract List<object> Query(
            string sqlQueryString,
            OrmAssemblySqlCore ormCollectibleAssembly,
            string typeName,
            string propertyNameFilter,
            out OrmTypeCore ormCollecibleType);

        public abstract List<object> Query(
            QueryCore query,
            string propertyNameFilter,
            Type entityType);

        public abstract List<object> Query(
            QueryCore query,
            string propertyNameFilter,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction);

        public virtual List<object> Query(
            string columnName,
            object columnValue,
            string tableName,
            string propertyNameFilter,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            DatabaseTableCore table = GetDatabaseTable(tableName);
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableCore).FullName,
                    tableName));
            }
            return table.Query(columnName, columnValue, propertyNameFilter, entityType, disposeConnectionAfterExecute, connection, transaction);
        }

        public List<E> Query<E>(QueryCore query, string propertyNameFilter) where E : class
        {
            List<object> objects = Query(query, propertyNameFilter, typeof(E));
            List<E> result = new List<E>();
            objects.ForEach(o => result.Add((E)o));
            return result;
        }

        #endregion //Methods
    }
}
