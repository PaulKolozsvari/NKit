namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    [Serializable]
    public class DatabaseTableForeignKeyColumnsWindows
    {
        #region Constructors

        public DatabaseTableForeignKeyColumnsWindows()
        {
            _foreignKeys = new List<ForeignKeyInfoWindows>();
        }

        public DatabaseTableForeignKeyColumnsWindows(string tableName)
        {
            _tableName = tableName;
            _foreignKeys = new List<ForeignKeyInfoWindows>();
        }

        public DatabaseTableForeignKeyColumnsWindows(string tableName, List<ForeignKeyInfoWindows> foreignKeys)
        {
            _tableName = tableName;
            _foreignKeys = foreignKeys;
        }

        #endregion //Constructors

        #region Fields

        protected string _tableName;
        protected List<ForeignKeyInfoWindows> _foreignKeys;

        #endregion //Fields

        #region Properties

        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        public List<ForeignKeyInfoWindows> ForeignKeys
        {
            get { return _foreignKeys; }
            set { _foreignKeys = value; }
        }

        #endregion //Properties
    }
}