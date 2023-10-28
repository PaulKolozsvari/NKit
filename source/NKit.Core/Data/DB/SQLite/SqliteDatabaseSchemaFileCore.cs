namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Serialization;
    using NKit.Utilities;

    #endregion //Using Directives

    public class SqliteDatabaseSchemaFileCore
    {
        #region Methods

        public static void ExportSchema(SqliteDatabaseCore database, string filePath)
        {
            GOC.Instance.GetSerializer(SerializerType.JSON).SerializeToFile(
                database,
                new Type[]
                {
                    typeof(SqliteDatabaseTableCore),
                    typeof(SqliteDatabaseTableColumnCore),
                    typeof(DatabaseCacheCore),
                    typeof(DatabaseCore),
                    typeof(DatabaseTableCore),
                    typeof(DatabaseTableColumnCore),
                    typeof(DatabaseTableForeignKeyColumnsCore),
                    typeof(DatabaseTableKeyColumnsCore),
                    typeof(ForeignKeyInfoCore),
                    typeof(DbmsCore),
                    typeof(SqliteTypeConverterCore),
                    typeof(SqliteTypeConversionInfoCore)
                },
                filePath);
        }

        public static SqliteDatabaseCore ImportSchema(
            string filePath,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory)
        {
            SqliteDatabaseCore result = (SqliteDatabaseCore)GOC.Instance.GetSerializer(SerializerType.JSON).DeserializeFromFile(
                typeof(SqliteDatabaseCore),
                new Type[]
                {
                    typeof(SqliteDatabaseTableCore),
                    typeof(SqliteDatabaseTableColumnCore),
                    typeof(DatabaseCacheCore),
                    typeof(DatabaseCore),
                    typeof(DatabaseTableCore),
                    typeof(DatabaseTableColumnCore)
                },
                filePath);
            if (createOrmAssembly)
            {
                result.CreateOrmAssembly(saveOrmAssembly, ormAssemblyOutputDirectory);
            }
            return result;
        }

        #endregion //Methods
    }
}
