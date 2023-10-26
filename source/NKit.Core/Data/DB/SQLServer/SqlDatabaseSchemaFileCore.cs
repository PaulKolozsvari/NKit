namespace NKit.Data.DB.SQLServer
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Utilities.Serialization;
    using NKit.Utilities;

    #endregion //Using Directives

    public class SqlDatabaseSchemaFileCore
    {
        #region Methods

        public static void ExportSchema(SqlDatabaseCore database, string filePath)
        {
            GOC.Instance.GetSerializer(SerializerType.XML).SerializeToFile(
                database,
                new Type[]
                {
                    typeof(SqlDatabaseTableCore),
                    typeof(SqlDatabaseTableColumnCore),
                    typeof(DatabaseCacheCore),
                    typeof(DatabaseCore),
                    typeof(DatabaseTableCore),
                    typeof(DatabaseTableColumnCore)
                },
                filePath);
        }

        public static SqlDatabaseCore ImportSchema(
            string filePath,
            bool createOrmAssembly,
            bool saveOrmAssembly,
            string ormAssemblyOutputDirectory)
        {
            SqlDatabaseCore result = (SqlDatabaseCore)GOC.Instance.GetSerializer(SerializerType.XML).DeserializeFromFile(
                typeof(SqlDatabaseCore),
                new Type[]
                {
                    typeof(SqlDatabaseTableCore),
                    typeof(SqlDatabaseTableColumnCore),
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
