namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public static class SqliteConnectionExtensionsWindows
    {
        #region Constants

        private const string TABLE_NAME_SQL_PARAMETER_NAME = "TableName";
        private const string FOREIGN_KEY_TABLE_COLUMN_NAME = "FK_Table";
        private const string FOREIGN_KEY_COLUMN_NAME = "FK_Column";
        private const string PRIMARY_KEY_TABLE_COLUMN_NAME = "PK_Table";
        private const string PRIMARY_KEY_COLUMN_NAME = "PK_Column";
        private const string CONSTRAINT_NAME_COLUMN_NAME = "Constraint_Name";

        #endregion //Constants

        #region Methods

        /// <summary>
        /// This method sometimes throws an exception in SQL 2008 or SQl 2008 R2:
        /// "There is insufficient system memory in resource pool 'internal' to run this query" error message when you run a full-text query that uses compound words in Microsoft SQL Server 2008 or in Microsoft SQL Server 2008 R2"
        /// Fix: http://support.microsoft.com/kb/982854
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static EntityCacheGeneric<string, ForeignKeyInfoWindows> GetTableForeignKeys(this SQLiteConnection connection, string tableName)
        {
            string commandText = GetTableForeignKeysCommandText(tableName);
            using (SQLiteCommand command = new SQLiteCommand(commandText, connection))
            {
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Add(new SQLiteParameter(string.Format("@{0}", TABLE_NAME_SQL_PARAMETER_NAME), tableName));
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    return GetForeignKeysFromSqlDataReader(reader);
                }
            }
        }

        private static string GetTableForeignKeysCommandText(string tableName)
        {
            return string.Format(@"SELECT {0} = FK.TABLE_NAME,
                                        {1} = CU.COLUMN_NAME,
                                        {2} = PK.TABLE_NAME,
                                        {3} = PT.COLUMN_NAME,
                                        {4} = C.CONSTRAINT_NAME
                                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                                INNER JOIN
                                (
                                    SELECT i1.TABLE_NAME, i2.COLUMN_NAME
                                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON
                                        i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                                    WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                                ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
                                WHERE FK.TABLE_NAME = @{5}
                                ORDER BY 1,2,3,4",
                                                 FOREIGN_KEY_TABLE_COLUMN_NAME,
                                                 FOREIGN_KEY_COLUMN_NAME,
                                                 PRIMARY_KEY_TABLE_COLUMN_NAME,
                                                 PRIMARY_KEY_COLUMN_NAME,
                                                 CONSTRAINT_NAME_COLUMN_NAME,
                                                 TABLE_NAME_SQL_PARAMETER_NAME);
        }

        private static EntityCacheGeneric<string, ForeignKeyInfoWindows> GetForeignKeysFromSqlDataReader(SQLiteDataReader reader)
        {
            EntityCacheGeneric<string, ForeignKeyInfoWindows> result = new EntityCacheGeneric<string, ForeignKeyInfoWindows>();
            while (reader.Read())
            {
                ForeignKeyInfoWindows f = new ForeignKeyInfoWindows()
                {
                    ChildTableName = reader[FOREIGN_KEY_TABLE_COLUMN_NAME].ToString(),
                    ChildTableForeignKeyName = reader[FOREIGN_KEY_COLUMN_NAME].ToString(),
                    ParentTableName = reader[PRIMARY_KEY_TABLE_COLUMN_NAME].ToString(),
                    ParentTablePrimaryKeyName = reader[PRIMARY_KEY_COLUMN_NAME].ToString(),
                    ConstraintName = reader[CONSTRAINT_NAME_COLUMN_NAME].ToString()
                };
                result.Add(f.ChildTableForeignKeyName, f);
            }
            return result;
        }

        #endregion //Methods
    }
}
