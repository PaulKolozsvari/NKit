namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Microsoft.Data.Sqlite;

    #endregion //Using Directives

    public class MicrosoftSqliteDatabaseTableGenericCore<E> : MicrosoftSqliteDatabaseTableCore where E : class
    {
        #region Constructors

        public MicrosoftSqliteDatabaseTableGenericCore() : base()
        {
        }

        public MicrosoftSqliteDatabaseTableGenericCore(string tableName, string connectionString)
            : base(tableName, connectionString)
        {
        }

        public MicrosoftSqliteDatabaseTableGenericCore(DataRow schemaRow, string connectionString)
            : base(schemaRow, connectionString)
        {
        }

        #endregion //Constructors

        #region Methods

        public int Insert(E e, bool disposeConnectionAfterExecute, SqliteConnection connection, SqliteTransaction transaction)
        {
            return base.Insert(e, disposeConnectionAfterExecute, connection, transaction);
        }

        public void Insert(List<E> entities, bool useTransaction)
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            base.Insert(objects, useTransaction);
        }

        public int Delete(E e, string columnName, bool disposeConnectionAfterExecute, SqliteConnection connection, SqliteTransaction transaction)
        {
            return base.Delete(e, columnName, disposeConnectionAfterExecute, connection, transaction);
        }

        public void Delete(List<E> entities, string columnName, bool useTransaction)
        {
            List<object> objects = new List<object>();
            entities.ForEach(e => objects.Add(e));
            base.Delete(objects, columnName, useTransaction);
        }

        public int Update(E e, string columnName, bool disposeConnectionAfterExecute, SqliteConnection connection, SqliteTransaction transaction)
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
