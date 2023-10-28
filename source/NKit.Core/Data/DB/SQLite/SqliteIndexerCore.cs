namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class SqliteIndexerCore
    {
        #region Constructors

        public SqliteIndexerCore(string tableName, string columnName, bool isUnique)
        {
            TableName = tableName;
            ColumnName = columnName;
            IsUnique = isUnique;
        }

        #endregion //Constructors

        #region Properties

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public bool IsUnique { get; set; }

        #endregion //Properties

        #region Methods

        public string GetCreateSqlScript()
        {
            string indexName = this.TableName + this.ColumnName + "_index";
            string result = !this.IsUnique ?
                    "CREATE INDEX IF NOT EXISTS \"" + indexName + "\" ON \"" + this.TableName + "\" (\"" + this.ColumnName + "\")" :
                    "CREATE UNIQUE INDEX IF NOT EXISTS \"" + indexName + "\" ON \"" + this.TableName + "\" (\"" + this.ColumnName + "\")";
            return result;
        }

        #endregion //Methods
    }
}
