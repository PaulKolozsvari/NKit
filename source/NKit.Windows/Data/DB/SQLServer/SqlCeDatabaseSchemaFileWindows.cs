namespace NKit.Data.DB.SQLServer
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using NKit.Utilities.Serialization;
    using NKit.Utilities;

    #endregion //Using Directives

    public class SqlCeDatabaseSchemaFileWindows
    {
        #region Methods

        public static void ExportSchema(SqlDatabaseWindows database, string filePath)
        {
            GOCWindows.Instance.GetSerializer(SerializerType.XML).SerializeToFile(
                database,
                new Type[] 
                {
                    typeof(SqlDatabaseTableBaseWindows),
                    typeof(SqlDatabaseTableColumnWindows),
                    typeof(DatabaseCacheWindows), 
                    typeof(DatabaseWindows), 
                    typeof(DatabaseTableWindows), 
                    typeof(DatabaseTableColumnWindows)
                },
                filePath);
        }

        public static SqlDatabaseWindows ImportSchema(
            string filePath, 
            bool createOrmAssembly, 
            bool saveOrmAssembly, 
            string ormAssemblyOutputDirectory)
        {
            SqlDatabaseWindows result = (SqlDatabaseWindows)GOCWindows.Instance.GetSerializer(SerializerType.XML).DeserializeFromFile(
                typeof(SqlDatabaseWindows),
                new Type[] 
                {
                    typeof(SqlDatabaseTableBaseWindows), 
                    typeof(SqlDatabaseTableColumnWindows),
                    typeof(DatabaseCacheWindows), 
                    typeof(DatabaseWindows), 
                    typeof(DatabaseTableWindows), 
                    typeof(DatabaseTableColumnWindows)
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