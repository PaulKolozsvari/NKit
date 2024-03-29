﻿namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Common;
    using System.Xml.Serialization;

    #endregion //Using Directives

    [Serializable]
    public abstract class DatabaseTableWindows
    {
        #region Constructors

        public DatabaseTableWindows()
        {
            _columns = new EntityCacheGeneric<string, DatabaseTableColumnWindows>();
            _tableName = this.GetType().Name;
            _childrenTables = new EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>>();
        }

        public DatabaseTableWindows(string tableName, string connectionString)
        {
            _columns = new EntityCacheGeneric<string, DatabaseTableColumnWindows>();
            _tableName = tableName;
            _connectionString = connectionString;
            _childrenTables = new EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>>();
        }

        public DatabaseTableWindows(DataRow schemaRow)
        {
            _columns = new EntityCacheGeneric<string, DatabaseTableColumnWindows>();
            PopulateFromSchema(schemaRow);
            _childrenTables = new EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schemaRow">The DataRow retrieved from a database schema containing information about this column.</param>
        public DatabaseTableWindows(DataRow schemaRow, string connectionString)
        {
            _columns = new EntityCacheGeneric<string, DatabaseTableColumnWindows>();
            PopulateFromSchema(schemaRow);
            _connectionString = connectionString;
            _childrenTables = new EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>>();
        }

        #endregion //Constructors

        #region Fields

        protected string _tableName;
        protected bool _isSystemTable;
        protected EntityCacheGeneric<string, DatabaseTableColumnWindows> _columns;
        protected Type _mappedType;
        protected string _mappedTypeName;
        protected string _connectionString;
        protected EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>> _childrenTables;

        #endregion //Fields

        #region Properties

        public virtual string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public virtual bool IsSystemTable
        {
            get { return _isSystemTable; }
            set { _isSystemTable = value; }
        }

        public virtual EntityCacheGeneric<string, DatabaseTableColumnWindows> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        [XmlIgnore]
        public virtual Type MappedType
        {
            get { return _mappedType; }
            set
            {
                _mappedType = value;
                _mappedTypeName = _mappedType.AssemblyQualifiedName;
            }
        }

        //public virtual string MappedTypeName
        //{
        //    get { return _mappedType.AssemblyQualifiedName; }
        //    set { _mappedType = Type.GetType(value); }
        //}

        /// <summary>
        /// Contains the children table names (keys) and a collection of foreign keys (values) that are mapped to this table's primary key.
        /// </summary>
        public virtual EntityCacheGeneric<string, EntityCacheGeneric<string, ForeignKeyInfoWindows>> ChildrenTables
        {
            get { return _childrenTables; }
            set { _childrenTables = value; }
        }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            return _tableName;
        }

        public virtual EntityCacheGeneric<string, DatabaseTableColumnWindows> GetForeignKeyColumns()
        {
            EntityCacheGeneric<string, DatabaseTableColumnWindows> result = new EntityCacheGeneric<string, DatabaseTableColumnWindows>();
            _columns.Where(c => c.IsForeignKey).ToList().ForEach(c => result.Add(c.ColumnName, c));
            return result;
        }

        public virtual List<E> Query<E>(
            string columnName,
            object columnValue,
            string propertyNameFilter,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction) where E : class
        {
            List<object> entities = Query(columnName, columnValue, propertyNameFilter, typeof(E), disposeConnectionAfterExecute, connection, transaction);
            List<E> result = new List<E>();
            foreach (object o in entities)
            {
                result.Add((E)o);
            }
            return result;
        }

        public abstract List<object> Query(
            string columnName,
            object columnValue,
            string propertyNameFilter,
            Type entityType,
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction);

        public abstract int Insert(object e, bool disposeConnectionAfterExecute);

        public abstract void Insert(List<object> entities, bool useTransaction);

        public abstract long CountAll(
            bool disposeConnectionAfterExecute,
            DbConnection connection,
            DbTransaction transaction);

        public abstract int DeleteAll(bool disposeConnectionAfterExecute, DbConnection connection, DbTransaction transaction);

        public abstract int Delete(object e, string columnName, bool disposeConnectionAfterExecute);

        public abstract void Delete(List<object> entities, string columnName, bool useTransaction);

        public abstract int Update(object e, string columnName, bool disposeConnectionAfterExecute);

        public abstract void Update(List<object> entities, string columnName, bool useTransaction);

        public abstract void Update(List<object> entities, bool useTransaction);

        public abstract void PopulateFromSchema(DataRow schemaRow);

        public abstract void PopulateColumnsFromSchema(DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract List<string> GetKeyColummnNames(DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract void PopulateColumnsFromSchema(DataTable columnsSchema);

        public abstract DataTable GetRawColumnsSchema(DbConnection connection, bool disposeConnectionAfterExecute);

        public abstract void AddColumnsByEntityType<E>() where E : class;

        public abstract void AddColumnsByEntityType(Type entityType);

        public abstract void AddColumn(string columnName, Type dotNetType);

        public abstract string GetSqlCreateTableScript();

        public abstract string GetSqlDropTableScript();

        public abstract string GetSqlCreateCompositeIndexonAllColumns(string indexName);

        public abstract List<string> GetSqlCreateSeparateIndecesOnAllColumns();

        #endregion //Methods
    }
}