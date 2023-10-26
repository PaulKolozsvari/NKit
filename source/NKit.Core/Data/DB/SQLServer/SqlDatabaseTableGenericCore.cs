namespace NKit.Data.DB.SQLServer
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;

    #endregion //Using Directives

    [Serializable]
    public class SqlDatabaseTableGenericCore<E> : SqlDatabaseTableCore where E : class
    {
        #region Constructors

        public SqlDatabaseTableGenericCore() : base()
        {
        }

        public SqlDatabaseTableGenericCore(string tableName, string connectionString)
            : base(tableName, connectionString)
        {
        }

        public SqlDatabaseTableGenericCore(DataRow schemaRow, string connectionString)
            : base(schemaRow, connectionString)
        {
        }

        #endregion //Constructors

        #region Methods

        public int Insert(E e, bool disposeConnectionAfterExecute, SqlConnection connection, SqlTransaction transaction)
        {
            return base.Insert(e, disposeConnectionAfterExecute, connection, transaction);
        }

        public void Insert(List<E> entities, bool useTransaction)
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            base.Insert(objects, useTransaction);
        }

        public int Delete(E e, string columnName, bool disposeConnectionAfterExecute, SqlConnection connection, SqlTransaction transaction)
        {
            return base.Delete(e, columnName, disposeConnectionAfterExecute, connection, transaction);
        }

        public void Delete(List<E> entities, string columnName, bool useTransaction)
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            base.Delete(objects, columnName, useTransaction);
        }

        public int Update(E e, string columnName, bool disposeConnectionAfterExecute, SqlConnection connection, SqlTransaction transaction)
        {
            return base.Update(e, columnName, disposeConnectionAfterExecute, connection, transaction);
        }

        public void Update(List<E> entities, string columnName, bool useTransaction)
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            base.Update(objects, columnName, useTransaction);
        }

        #endregion //Methods
    }
}
