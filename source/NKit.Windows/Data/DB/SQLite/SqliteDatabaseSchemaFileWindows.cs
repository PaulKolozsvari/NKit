namespace NKit.Data.DB.SQLite
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

    public class SqliteDatabaseSchemaFileWindows
    {
        #region Methods

        public static void ExportSchema(SqliteDatabaseWindows database, string filePath)
        {
			GOC.Instance.GetSerializer(SerializerType.JSON).SerializeToFile(
                database,
                new Type[] 
                {
                    typeof(SqliteDatabaseTableWindows),
                    typeof(SqliteDatabaseTableColumnWindows),
                    typeof(DatabaseCacheWindows),
                    typeof(DatabaseWindows),
                    typeof(DatabaseTableWindowsWindows),
                    typeof(DatabaseTableColumnWindows),
					typeof(DatabaseTableForeignKeyColumnsWindows),
					typeof(DatabaseTableKeyColumnsWindows),
					typeof(ForeignKeyInfoWindows),
					typeof(DbmsWindows),
					typeof(SqliteTypeConverterWindows),
					typeof(SqliteTypeConversionInfoWindows)
                },
                filePath);
        }

        public static SqliteDatabaseWindows ImportSchema(
            string filePath, 
            bool createOrmAssembly, 
            bool saveOrmAssembly, 
            string ormAssemblyOutputDirectory)
        {
            SqliteDatabaseWindows result = (SqliteDatabaseWindows)GOCWindows.Instance.GetSerializer(SerializerType.JSON).DeserializeFromFile(
                typeof(SqliteDatabaseWindows),
                new Type[] 
                {
                    typeof(SqliteDatabaseTableWindows), 
                    typeof(SqliteDatabaseTableColumnWindows),
                    typeof(DatabaseCacheWindows),
                    typeof(DatabaseWindows), 
                    typeof(DatabaseTableWindowsWindows), 
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