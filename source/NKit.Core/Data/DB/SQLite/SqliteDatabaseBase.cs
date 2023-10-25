//namespace NKit.Data.DB.SQLite
//{
//    #region Using Directives

//    using System;
//    using System.Collections.Generic;
//    using System.Reflection.Emit;
//    using System.Text;
//    using System.Xml.Linq;

//    #endregion //Using Directives

//    [Serializable]
//    public abstract class SqliteDatabaseBase
//    {
//        #region Constructors

//        public SqliteDatabaseBase(string connectionString)
//        {
//            _tables = new EntityCacheGeneric<string, SqliteDatabaseTable>();
//            _name = this.GetType().Name;
//            _connectionString = connectionString;
//        }

//        public SqliteDatabaseBase(string name, string connectionString)
//        {
//            if (string.IsNullOrEmpty(name))
//            {
//                throw new ArgumentNullException(string.Format(
//                    "{0} not be null or empty when constructing {1}.",
//                    EntityReaderGeneric<SqliteDatabaseBase>.GetPropertyName(p => p.Name, false),
//                    this.GetType().FullName));
//            }
//            _tables = new EntityCacheGeneric<string, SqliteDatabaseTable>();
//            _name = name;
//            _connectionString = connectionString;
//        }

//        #endregion //Constructors

//        #region Fields

//        protected string _name;
//        protected string _connectionString;
//        protected EntityCacheGeneric<string, SqliteDatabaseTable> _tables;
//        protected OrmAssembly _ormAssembly;

//        #endregion //Fields

//        #region Properties

//        public virtual string Name
//        {
//            get { return _name; }
//            set { _name = value; }
//        }

//        public EntityCacheGeneric<string, SqliteDatabaseTable> Tables
//        {
//            get { return _tables; }
//            set { _tables = value; }
//        }

//        #endregion //Properties

//        #region Methods

//        public OrmAssembly GetOrmAssembly()
//        {
//            return _ormAssembly;
//        }

//        public override string ToString()
//        {
//            return _name;
//        }

//        public abstract List<DatabaseTableKeyColumns> GetTableKeyColumns();

//        public abstract List<DatabaseTableForeignKeyColumns> GetTableForeignKeyColumns();

//        public abstract void Dispose();

//        public virtual void ClearTables()
//        {
//            _tables.Clear();
//        }

//        public List<SqliteDatabaseTable> GetTablesMentionedInQuery(Query query)
//        {
//            List<SqliteDatabaseTable> result = new List<SqliteDatabaseTable>();
//            foreach (string t in query.TableNamesInQuery)
//            {
//                SqliteDatabaseTable table = _tables[t];
//                if (table == null)
//                {
//                    throw new NullReferenceException(string.Format(
//                        "Could not find table {0} mentioned in {1} inside {2}.",
//                        t,
//                        query.GetType().FullName,
//                        this.GetType().FullName));
//                }
//                result.Add(table);
//            }
//            return result;
//        }

//        public virtual SqliteDatabaseTable GetDatabaseTable(string tableName)
//        {
//            if (!_tables.Exists(tableName))
//            {
//                return null;
//            }
//            return (SqliteDatabaseTable)_tables[tableName];
//        }

//        public void CreateOrmAssembly(
//            bool saveOrmAssembly,
//            string ormAssemblyOutputDirectory)
//        {
//            string assemblyFileName = string.Format("{0}.dll", this.Name);
//            _ormAssembly = new OrmAssembly(
//                this.Name,
//                assemblyFileName,
//                AssemblyBuilderAccess.Run);
//            foreach (SqliteDatabaseTable table in _tables)
//            {
//                OrmType ormType = _ormAssembly.CreateOrmType(table.TableName, true);
//                foreach (DatabaseTableColumn column in table.Columns)
//                {
//                    ormType.CreateOrmProperty(
//                        column.ColumnName,
//                        SqliteTypeConverter.Instance.GetDotNetType(column.DataType, column.IsNullable));
//                }
//                table.MappedType = ormType.CreateType();
//            }
//            if (!saveOrmAssembly)
//            {
//                return;
//            }
//            _ormAssembly.Save(ormAssemblyOutputDirectory);
//        }

//        #endregion //Methods
//    }
//}
