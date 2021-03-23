namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Reflection.Emit;
    using System.Data.Common;
    using System.Data;
    using System.IO;
	using System.Xml.Serialization;
    using NKit.Data.DB;
    using NKit.Data.ORM;
    using NKit.Data.DB.SQLQuery;

    #endregion //Using Directives

    //Sqlite Locking: https://www.sqlite.org/lockingv3.html

    [Serializable]
    public abstract class SqliteDatabaseBaseWindows : IDisposable
    {
        #region Constructors

		public SqliteDatabaseBaseWindows(string connectionString)
        {
			_tables = new EntityCacheGeneric<string, SqliteDatabaseTableWindows> ();
            _name = this.GetType().Name;
            _connectionString = connectionString;
        }

		public SqliteDatabaseBaseWindows(string name, string connectionString)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(string.Format(
                    "{0} not be null or empty when constructing {1}.",
                    EntityReaderGeneric<SqliteDatabaseBaseWindows>.GetPropertyName(p => p.Name, false),
                    this.GetType().FullName));
            }
			_tables = new EntityCacheGeneric<string, SqliteDatabaseTableWindows> ();
            _name = name;
            _connectionString = connectionString;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;
		protected string _connectionString;
        protected EntityCacheGeneric<string, SqliteDatabaseTableWindows> _tables;
        protected OrmAssemblyWindows _ormAssembly;

        #endregion //Fields

        #region Properties

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public EntityCacheGeneric<string, SqliteDatabaseTableWindows> Tables
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

        public abstract List<DatabaseTableKeyColumnsWindows> GetTableKeyColumns();

        public abstract List<DatabaseTableForeignKeyColumnsWindows> GetTableForeignKeyColumns();

        public abstract void Dispose();

        public virtual void ClearTables()
        {
            _tables.Clear();
        }

		public List<SqliteDatabaseTableWindows> GetTablesMentionedInQuery(QueryWindows query)
		{
			List<SqliteDatabaseTableWindows> result = new List<SqliteDatabaseTableWindows>();
			foreach (string t in query.TableNamesInQuery)
			{
				SqliteDatabaseTableWindows table = _tables[t];
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

        public virtual SqliteDatabaseTableWindows GetDatabaseTable(string tableName)
        {
            if (!_tables.Exists(tableName))
            {
                return null;
            }
            return (SqliteDatabaseTableWindows)_tables[tableName];
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
            foreach (SqliteDatabaseTableWindows table in _tables)
            {
                OrmTypeWindows ormType = _ormAssembly.CreateOrmType(table.TableName, true);
                foreach (DatabaseTableColumnWindows column in table.Columns)
                {
                    ormType.CreateOrmProperty(
                        column.ColumnName,
                        SqliteTypeConverterWindows.Instance.GetDotNetType(column.DataType, column.IsNullable));
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