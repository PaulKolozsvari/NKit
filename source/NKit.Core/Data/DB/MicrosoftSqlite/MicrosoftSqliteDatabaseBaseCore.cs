namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Text;
    using NKit.Data.DB.SQLQuery;
    using NKit.Data.ORM;

    #endregion //Using Directives

    //Sqlite Locking: https://www.sqlite.org/lockingv3.html
    [Serializable]
    public abstract class MicrosoftSqliteDatabaseBaseCore : IDisposable
    {
        #region Constructors

        public MicrosoftSqliteDatabaseBaseCore(string connectionString)
        {
            _tables = new EntityCacheGeneric<string, MicrosoftSqliteDatabaseTableCore>();
            _name = this.GetType().Name;
            _connectionString = connectionString;
        }

        public MicrosoftSqliteDatabaseBaseCore(string name, string connectionString)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(string.Format(
                    "{0} not be null or empty when constructing {1}.",
                    EntityReaderGeneric<MicrosoftSqliteDatabaseBaseCore>.GetPropertyName(p => p.Name, false),
                    this.GetType().FullName));
            }
            _tables = new EntityCacheGeneric<string, MicrosoftSqliteDatabaseTableCore>();
            _name = name;
            _connectionString = connectionString;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;
        protected string _connectionString;
        protected EntityCacheGeneric<string, MicrosoftSqliteDatabaseTableCore> _tables;
        protected OrmAssemblyCore _ormAssembly;

        #endregion //Fields

        #region Properties

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public EntityCacheGeneric<string, MicrosoftSqliteDatabaseTableCore> Tables
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

        public abstract List<DatabaseTableKeyColumnsCore> GetTableKeyColumns();

        public abstract List<DatabaseTableForeignKeyColumnsCore> GetTableForeignKeyColumns();

        public abstract void Dispose();

        public virtual void ClearTables()
        {
            _tables.Clear();
        }

        public List<MicrosoftSqliteDatabaseTableCore> GetTablesMentionedInQuery(QueryCore query)
        {
            List<MicrosoftSqliteDatabaseTableCore> result = new List<MicrosoftSqliteDatabaseTableCore>();
            foreach (string t in query.TableNamesInQuery)
            {
                MicrosoftSqliteDatabaseTableCore table = _tables[t];
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

        public virtual MicrosoftSqliteDatabaseTableCore GetDatabaseTable(string tableName)
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            return (MicrosoftSqliteDatabaseTableCore)_tables[tableName];
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
            foreach (MicrosoftSqliteDatabaseTableCore table in _tables)
            {
                OrmTypeCore ormType = _ormAssembly.CreateOrmType(table.TableName, true);
                foreach (DatabaseTableColumnCore column in table.Columns)
                {
                    ormType.CreateOrmProperty(
                        column.ColumnName,
                        MicrosoftSqliteTypeConverterCore.Instance.GetDotNetType(column.DataType, column.IsNullable));
                }
                table.MappedType = ormType.CreateType();
            }
            if (!saveOrmAssembly)
            {
                return;
            }
            _ormAssembly.Save(ormAssemblyOutputDirectory);
        }

        #endregion //Methods
    }
}
