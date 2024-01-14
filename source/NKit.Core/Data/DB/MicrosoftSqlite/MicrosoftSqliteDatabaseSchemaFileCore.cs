namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Serialization;
    using NKit.Utilities;

    #endregion //Using Directives

    public class MicrosoftSqliteDatabaseSchemaFileCore
    {
        #region Methods

        public static void ExportSchema(MicrosoftSqliteDatabaseCore database, string filePath)
        {
            GOC.Instance.GetSerializer(SerializerType.JSON).SerializeToFile(
                database,
                new Type[]
                {
                    typeof(MicrosoftSqliteDatabaseTableCore),
                    typeof(MicrosoftSqliteDatabaseTableColumnCore),
                    typeof(DatabaseCacheCore),
                    typeof(DatabaseCore),
                    typeof(DatabaseTableCore),
                    typeof(DatabaseTableColumnCore),
                    typeof(DatabaseTableForeignKeyColumnsCore),
                    typeof(DatabaseTableKeyColumnsCore),
                    typeof(ForeignKeyInfoCore),
                    typeof(DbmsCore),
                    typeof(MicrosoftSqliteTypeConverterCore),
                    typeof(MicrosoftSqliteTypeConversionInfoCore)
                },
                filePath);
        }

        public static MicrosoftSqliteDatabaseCore ImportSchema(
            string filePath,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory)
        {
            MicrosoftSqliteDatabaseCore result = (MicrosoftSqliteDatabaseCore)GOC.Instance.GetSerializer(SerializerType.JSON).DeserializeFromFile(
                typeof(MicrosoftSqliteDatabaseCore),
                new Type[]
                {
                    typeof(MicrosoftSqliteDatabaseTableCore),
                    typeof(MicrosoftSqliteDatabaseTableColumnCore),
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
