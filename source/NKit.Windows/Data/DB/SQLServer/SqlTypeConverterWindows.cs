namespace NKit.Data.DB.SQLServer
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlTypes;
    using System.Data;

    #endregion //Using Directives

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.data.sqltypes.aspx
    /// </summary>
    public class SqlTypeConverterWindows : EntityCacheGeneric<string, SqlTypeConversionInfoWindows>
    {
        #region Singleton Setup

        private static SqlTypeConverterWindows _instance;

        public static SqlTypeConverterWindows Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SqlTypeConverterWindows();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private SqlTypeConverterWindows()
        {
            Add(new SqlTypeConversionInfoWindows("bigint", typeof(SqlInt64), SqlDbType.BigInt, typeof(Int64)));
            Add(new SqlTypeConversionInfoWindows("binary", typeof(SqlBytes), SqlDbType.VarBinary, typeof(Byte[])));
            Add(new SqlTypeConversionInfoWindows("bit", typeof(SqlBoolean), SqlDbType.Bit, typeof(Boolean)));
            Add(new SqlTypeConversionInfoWindows("char", typeof(SqlChars), SqlDbType.Char, typeof(char))); //this one may need work
            Add(new SqlTypeConversionInfoWindows("cursor", null, SqlDbType.Variant, null));
            Add(new SqlTypeConversionInfoWindows("date", typeof(SqlDateTime), SqlDbType.Date, typeof(DateTime)));
            Add(new SqlTypeConversionInfoWindows("datetime", typeof(SqlDateTime), SqlDbType.DateTime, typeof(DateTime)));
            Add(new SqlTypeConversionInfoWindows("datetime2", typeof(SqlDateTime), SqlDbType.DateTime2, typeof(DateTime)));
            Add(new SqlTypeConversionInfoWindows("DATETIMEOFFSET", typeof(SqlDateTime), SqlDbType.DateTimeOffset,typeof(DateTimeOffset)));
            Add(new SqlTypeConversionInfoWindows("decimal", typeof(SqlDecimal), SqlDbType.Decimal, typeof(Decimal)));
            Add(new SqlTypeConversionInfoWindows("float", typeof(SqlDouble), SqlDbType.Float, typeof(Double)));
            //Add(new SqlTypeConversionInfo("geography", typeof(SqlGeography),typeosf(null)));
            //Add(new SqlTypeConversionInfo("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfo("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new SqlTypeConversionInfoWindows("image", typeof(SqlBytes), SqlDbType.Image, typeof(byte[])));
            Add(new SqlTypeConversionInfoWindows("int", typeof(SqlInt32), SqlDbType.Int, typeof(Int32)));
            Add(new SqlTypeConversionInfoWindows("money", typeof(SqlMoney), SqlDbType.Money, typeof(Decimal)));
            Add(new SqlTypeConversionInfoWindows("nchar", typeof(SqlChars), SqlDbType.NChar, typeof(String)));
            Add(new SqlTypeConversionInfoWindows("ntext", typeof(SqlChars), SqlDbType.NText, null));
            Add(new SqlTypeConversionInfoWindows("numeric", typeof(SqlDecimal), SqlDbType.Decimal, typeof(Decimal)));
            Add(new SqlTypeConversionInfoWindows("nvarchar", typeof(SqlChars), SqlDbType.NVarChar, typeof(String)));
            Add(new SqlTypeConversionInfoWindows("nvarchar(1)", typeof(SqlChars), SqlDbType.NVarChar, typeof(Char)));
            Add(new SqlTypeConversionInfoWindows("nchar(1)", typeof(SqlChars), SqlDbType.NVarChar, typeof(Char)));
            Add(new SqlTypeConversionInfoWindows("real", typeof(SqlSingle), SqlDbType.Real, typeof(Single)));
            Add(new SqlTypeConversionInfoWindows("rowversion", null, SqlDbType.Binary, typeof(Byte[])));
            Add(new SqlTypeConversionInfoWindows("smallint", typeof(SqlInt16), SqlDbType.SmallInt, typeof(Int16)));
            Add(new SqlTypeConversionInfoWindows("smallmoney", typeof(SqlMoney), SqlDbType.SmallMoney,typeof(Decimal)));
            Add(new SqlTypeConversionInfoWindows("sql_variant", null, SqlDbType.Variant, typeof(Object)));
            Add(new SqlTypeConversionInfoWindows("table", null, SqlDbType.Structured, null));
            Add(new SqlTypeConversionInfoWindows("text", typeof(SqlString), SqlDbType.VarChar,typeof(string))); //this one may need work
            Add(new SqlTypeConversionInfoWindows("time", null, SqlDbType.Time, typeof(TimeSpan)));
            Add(new SqlTypeConversionInfoWindows("timestamp", null, SqlDbType.Timestamp, null));
            Add(new SqlTypeConversionInfoWindows("tinyint", typeof(SqlByte), SqlDbType.TinyInt, typeof(Byte)));
            Add(new SqlTypeConversionInfoWindows("uniqueidentifier", typeof(SqlGuid), SqlDbType.UniqueIdentifier, typeof(Guid)));
            Add(new SqlTypeConversionInfoWindows("varbinary", typeof(SqlBytes), SqlDbType.VarBinary, typeof(Byte[])));
            Add(new SqlTypeConversionInfoWindows("varbinary(1)", typeof(SqlBytes), SqlDbType.VarBinary, typeof(byte)));
            Add(new SqlTypeConversionInfoWindows("binary(1)", typeof(SqlBytes), SqlDbType.Binary, typeof(byte)));
            Add(new SqlTypeConversionInfoWindows("varchar", typeof(SqlString), SqlDbType.VarChar, typeof(string))); //this one may need work
            Add(new SqlTypeConversionInfoWindows("xml", typeof(SqlXml), SqlDbType.Xml, typeof(string)));
        }

        #endregion //Constructors

        #region Methods

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoWindows).FullName,
                    sqlTypeName));
            }
            Type result = this[sqlTypeName].DotNetType;
            if (result == null)
            {
                throw new NullReferenceException(string.Format(
                    "No matching .NET type for {0}.",
                    sqlTypeName));
            }
            if (isNullable)
            {
                return DataHelperWindows.GetNullableType(result);
            }
            return result;
        }

        public Type GetDotNetType(Type sqlType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    if (typeInfo.DotNetType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching .NET type for SQL type {0}.",
                            sqlType.FullName));
                    }
                    return typeInfo.DotNetType;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    if (typeInfo.DotNetType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching .NET type for SQL type {0}.",
                            sqlType.FullName));
                    }
                    return typeInfo.DotNetType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQL Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoWindows).FullName,
                    sqlTypeName));
            }
            Type result = this[sqlTypeName].SqlType;
            if (result == null)
            {
                throw new NullReferenceException(string.Format(
                    "No matching SQL type for {0}.",
                    sqlTypeName));
            }
            if (isNullable)
            {
                return DataHelperWindows.GetNullableType(result);
            }
            return result;
        }

        public Type GetSqlType(Type dotNetType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlType;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        public SqlDbType GetSqlDbType(string sqlTypeName)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoWindows).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].SqlDbType;
        }

        public SqlDbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlDbType;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlDbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                dotNetType.FullName));
        }

        public SqlDbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (SqlTypeConversionInfoWindows typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlDbType;
                }
                else if (DataHelperWindows.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlDbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqlTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        #endregion //Methods
    }
}
