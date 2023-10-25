namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    [Serializable]
    public class DatabaseTableForeignKeyColumnsCore
    {
        #region Constructors

        public DatabaseTableForeignKeyColumnsCore()
        {
            _foreignKeys = new List<ForeignKeyInfoCore>();
        }

        public DatabaseTableForeignKeyColumnsCore(string tableName)
        {
            _tableName = tableName;
            _foreignKeys = new List<ForeignKeyInfoCore>();
        }

        public DatabaseTableForeignKeyColumnsCore(string tableName, List<ForeignKeyInfoCore> foreignKeys)
        {
            _tableName = tableName;
            _foreignKeys = foreignKeys;
        }

        #endregion //Constructors

        #region Fields

        protected string _tableName;
        protected List<ForeignKeyInfoCore> _foreignKeys;

        #endregion //Fields

        #region Properties

        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public List<ForeignKeyInfoCore> ForeignKeys
        {
            get { return _foreignKeys; }
            set { _foreignKeys = value; }
        }

        #endregion //Properties
    }
}
