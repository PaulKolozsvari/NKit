﻿namespace NKit.Data.ORM
{
    using NKit.Data.DB.SQLServer;
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class OrmCodeGeneratorWindows
    {
        #region Constants

        private const string COLUMN_NAME_SCHEMA_ATTRIBUTE = "ColumnName";
        private const string ORDINAL_POSITION_SCHEMA_ATTRIBUTE = "ColumnOrdinal";
        private const string IS_NULLABLE_SCHEMA_ATTRIBUTE = "AllowDBNull";
        private const string DATA_TYPE_NAME_SCHEMA_ATTRIBUTE = "DataTypeName";

        #endregion //Constants

        #region Methods

        public static Type GenerateType(
            string assemblyName,
            string typeName,
            SqlDataReader reader,
            bool prefixWithAssemblyNamespace)
        {
            return GenerateType(assemblyName, typeName, reader.GetSchemaTable(), prefixWithAssemblyNamespace);
        }

        public static Type GenerateType(
            string assemblyName,
            string typeName,
            DataTable schemaTable,
            bool prefixWithAssemblyNamespace)
        {
            OrmAssemblyWindows assembly = new OrmAssemblyWindows(assemblyName, AssemblyBuilderAccess.Run);
            OrmTypeWindows type = assembly.CreateOrmType(typeName, prefixWithAssemblyNamespace);
            foreach (DataRow row in schemaTable.Rows)
            {
                string columnName = row[COLUMN_NAME_SCHEMA_ATTRIBUTE].ToString();
                short ordinalPosition = Convert.ToInt16(row[ORDINAL_POSITION_SCHEMA_ATTRIBUTE]);
                bool isNullable = bool.Parse(row[IS_NULLABLE_SCHEMA_ATTRIBUTE].ToString());
                string dataTypeName = row[DATA_TYPE_NAME_SCHEMA_ATTRIBUTE].ToString();
                type.CreateOrmProperty(
                    columnName, 
                    SqlTypeConverterWindows.Instance.GetDotNetType(dataTypeName, isNullable), 
                    System.Reflection.PropertyAttributes.HasDefault);
            }
            Type result = type.CreateType();
            return result;
        }

        #endregion //Methods
    }
}