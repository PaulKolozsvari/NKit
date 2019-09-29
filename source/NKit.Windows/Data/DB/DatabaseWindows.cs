namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection.Emit;
    using System.Data.Common;
    using NKit.Data.DB.SQLQuery;
    using NKit.Data.DB.SQLServer;
    using System.Data.SqlClient;
    using System.Data;
    using System.IO;
    using NKit.Toolkit.Data.ORM;

    #endregion //Using Directives

    [Serializable]
    public abstract class DatabaseWindows : IDisposable
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

        public DatabaseWindows()
        {
            _tables = new EntityCacheGeneric<string, DatabaseTableWindows>();
            _name = this.GetType().Name;
        }

        public DatabaseWindows(
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

        public DatabaseWindows(
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
                    EntityReaderGeneric<DatabaseWindows>.GetPropertyName(p => p.Name, false),
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
        protected EntityCacheGeneric<string, DatabaseTableWindows> _tables;
        protected OrmAssemblyWindows _ormAssembly;

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

        public EntityCacheGeneric<string, DatabaseTableWindows> Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        #endregion //Properties

        #region Methods

        public OrmAssemblyWindows GetOrmAssembly()
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

        public abstract List<DatabaseTableKeyColumnsWindows> GetTableKeyColumns();

        public abstract List<DatabaseTableForeignKeyColumnsWindows> GetTableForeignKeyColumns();

        public abstract void Dispose();

        public virtual void ClearTables()
        {
            _tables.Clear();
        } 

        public List<DatabaseTableWindows> GetTablesMentionedInQuery(QueryWindows query)
        {
            List<DatabaseTableWindows> result = new List<DatabaseTableWindows>();
            foreach (string t in query.TableNamesInQuery)
            {
                DatabaseTableWindows table = _tables[t];
                if (table == null)
                {
                    throw new NullReferenceException(string.Format(
                        "Could not find table {0} mentioned in {1} inside {2}.",
                        t,
                        query.GetType().FullName,
                        this.GetType().FullName));
                }
                result.Add(table);
            }
            return result;
        }

        public virtual DatabaseTableWindows GetDatabaseTable(Type entityType)
        {
            return GetDatabaseTable(entityType.Name);
        }

        public virtual DatabaseTableWindows GetDatabaseTable(string tableName)
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            return (DatabaseTableWindows)_tables[tableName];
        }

        public void CreateOrmAssembly(
            bool saveOrmAssembly, 
            string ormAssemblyOutputDirectory)
        {
            string assemblyFileName = string.Format("{0}.dll", this.Name);
            _ormAssembly = new OrmAssemblyWindows(
                this.Name,
                assemblyFileName,
                AssemblyBuilderAccess.RunAndSave);
            foreach (DatabaseTableWindows table in _tables)
            {
                OrmTypeWindows ormType = _ormAssembly.CreateOrmType(table.TableName, true);
                PublishFeedback(string.Format("Created type {0}.", ormType.TypeName));
                foreach (DatabaseTableColumnWindows column in table.Columns)
                {
                    ormType.CreateOrmProperty(
                        column.ColumnName,
                        SqlTypeConverterWindows.Instance.GetDotNetType(column.DataType, column.IsNullable));
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
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            return Query(columnName, columnValue, entityType.Name, entityType, disposeConnectionAfterExecute, connection, transaction);
        }

        public abstract List<object> Query(string sqlQueryString,
            OrmAssemblySqlWindows ormCollectibleAssembly,
            string typeName,
            out OrmTypeWindows ormCollecibleType);

        public abstract List<object> Query(
            QueryWindows query, 
            Type entityType);

        public abstract List<object> Query(
            QueryWindows query,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction);

        public virtual List<object> Query(
            string columnName,
            object columnValue,
            string tableName,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction)
        {
            DatabaseTableWindows table = GetDatabaseTable(tableName);
            if (table == null)
            {
                throw new NullReferenceException(string.Format(
                    "Could not find {0} with name {1}.",
                    typeof(DatabaseTableWindows).FullName,
                    tableName));
            }
            return table.Query(columnName, columnValue, entityType, disposeConnectionAfterExecute, connection, transaction);
        }

        public List<E> Query<E>(QueryWindows query) where E : class
        {
            List<object> objects = Query(query, typeof(E));
            List<E> result = new List<E>();
            objects.ForEach(o => result.Add((E)o));
            return result;
        }

        #endregion //Methods
    }
}