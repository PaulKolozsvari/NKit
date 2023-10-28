namespace NKit.Data.ORM
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using System.Reflection.Emit;
    using System.Text;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    public class OrmAssemblySqlCore : OrmAssemblyCore
    {
        #region Constructors

        public OrmAssemblySqlCore(string assemblyName, AssemblyBuilderAccess assemblyBuilderAccess) :
            base(assemblyName, assemblyBuilderAccess)
        {
        }

        public OrmAssemblySqlCore(string assemblyName, string assemblyFileName, AssemblyBuilderAccess assemblyBuilderAccess) :
            base(assemblyName, assemblyFileName, assemblyBuilderAccess)
        {
        }

        #endregion //Constructors

        #region Constants

        public const string COLUMN_NAME_SCHEMA_ATTRIBUTE = "ColumnName";
        public const string ORDINAL_POSITION_SCHEMA_ATTRIBUTE = "ColumnOrdinal";
        public const string IS_NULLABLE_SCHEMA_ATTRIBUTE = "AllowDBNull";
        public const string DATA_TYPE_NAME_SCHEMA_ATTRIBUTE = "DataTypeName";

        #endregion //Constants

        #region Methods

        public OrmTypeCore CreateOrmTypeFromSqlDataReader(
            string typeName,
            DbDataReader reader,
            bool prefixWithAssemblyNamespace)
        {
            DataTable schemaTable = reader.GetSchemaTable();
            OrmTypeCore result = CreateOrmType(typeName, prefixWithAssemblyNamespace);
            foreach (DataRow r in schemaTable.Rows)
            {
                string columnName = r[COLUMN_NAME_SCHEMA_ATTRIBUTE].ToString();
                short ordinalPosition = Convert.ToInt16(r[ORDINAL_POSITION_SCHEMA_ATTRIBUTE]);
                bool isNullable = bool.Parse(r[IS_NULLABLE_SCHEMA_ATTRIBUTE].ToString());
                string dataTypeName = r[DATA_TYPE_NAME_SCHEMA_ATTRIBUTE].ToString();
                result.CreateOrmProperty(
                    columnName,
                    SqlTypeConverterCore.Instance.GetDotNetType(dataTypeName, isNullable));
            }
            result.CreateType();
            return result;
        }

        #region Destructors

        //~OrmAssemblySql()
        //{
        //    int test = 0;
        //}

        #endregion //Destructors

        #endregion //Methods
    }
}
