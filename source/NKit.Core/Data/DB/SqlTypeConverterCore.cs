namespace NKit.Data.DB
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Data;
    using System.Text;
    using NKit.Data;

    #endregion //Using Directives

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.data.sqltypes.aspx
    /// </summary>
    public class SqlTypeConverterCore : EntityCacheGeneric<string, SqlTypeConversionInfoCore>
    {
        #region Singleton Setup

        private static SqlTypeConverterCore _instance;

        public static SqlTypeConverterCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SqlTypeConverterCore();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private SqlTypeConverterCore()
        {
            Add(new SqlTypeConversionInfoCore("bigint", typeof(SqlInt64), SqlDbType.BigInt, typeof(long)));
            Add(new SqlTypeConversionInfoCore("binary", typeof(SqlBytes), SqlDbType.VarBinary, typeof(byte[])));
            Add(new SqlTypeConversionInfoCore("bit", typeof(SqlBoolean), SqlDbType.Bit, typeof(bool)));
            Add(new SqlTypeConversionInfoCore("char", typeof(SqlChars), SqlDbType.Char, typeof(char))); //this one may need work
            Add(new SqlTypeConversionInfoCore("cursor", null, SqlDbType.Variant, null));
            Add(new SqlTypeConversionInfoCore("date", typeof(SqlDateTime), SqlDbType.Date, typeof(DateTime)));
            Add(new SqlTypeConversionInfoCore("datetime", typeof(SqlDateTime), SqlDbType.DateTime, typeof(DateTime)));
            Add(new SqlTypeConversionInfoCore("datetime2", typeof(SqlDateTime), SqlDbType.DateTime2, typeof(DateTime)));
            Add(new SqlTypeConversionInfoCore("DATETIMEOFFSET", typeof(SqlDateTime), SqlDbType.DateTimeOffset, typeof(DateTimeOffset)));
            Add(new SqlTypeConversionInfoCore("decimal", typeof(SqlDecimal), SqlDbType.Decimal, typeof(decimal)));
            Add(new SqlTypeConversionInfoCore("float", typeof(SqlDouble), SqlDbType.Float, typeof(double)));
            //Add(new SqlTypeConversionInfoCore("geography", typeof(SqlGeography),typeosf(null)));
            //Add(new SqlTypeConversionInfoCore("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfoCore("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new SqlTypeConversionInfoCore("image", typeof(SqlBytes), SqlDbType.Image, typeof(byte[])));
            Add(new SqlTypeConversionInfoCore("int", typeof(SqlInt32), SqlDbType.Int, typeof(int)));
            Add(new SqlTypeConversionInfoCore("money", typeof(SqlMoney), SqlDbType.Money, typeof(decimal)));
            Add(new SqlTypeConversionInfoCore("nchar", typeof(SqlChars), SqlDbType.NChar, typeof(string)));
            Add(new SqlTypeConversionInfoCore("ntext", typeof(SqlChars), SqlDbType.NText, null));
            Add(new SqlTypeConversionInfoCore("numeric", typeof(SqlDecimal), SqlDbType.Decimal, typeof(decimal)));
            Add(new SqlTypeConversionInfoCore("nvarchar", typeof(SqlChars), SqlDbType.NVarChar, typeof(string)));
            Add(new SqlTypeConversionInfoCore("nvarchar(1)", typeof(SqlChars), SqlDbType.NVarChar, typeof(char)));
            Add(new SqlTypeConversionInfoCore("nchar(1)", typeof(SqlChars), SqlDbType.NVarChar, typeof(char)));
            Add(new SqlTypeConversionInfoCore("real", typeof(SqlSingle), SqlDbType.Real, typeof(float)));
            Add(new SqlTypeConversionInfoCore("rowversion", null, SqlDbType.Binary, typeof(byte[])));
            Add(new SqlTypeConversionInfoCore("smallint", typeof(SqlInt16), SqlDbType.SmallInt, typeof(short)));
            Add(new SqlTypeConversionInfoCore("smallmoney", typeof(SqlMoney), SqlDbType.SmallMoney, typeof(decimal)));
            Add(new SqlTypeConversionInfoCore("sql_variant", null, SqlDbType.Variant, typeof(object)));
            Add(new SqlTypeConversionInfoCore("table", null, SqlDbType.Structured, null));
            Add(new SqlTypeConversionInfoCore("text", typeof(SqlString), SqlDbType.VarChar, typeof(string))); //this one may need work
            Add(new SqlTypeConversionInfoCore("time", null, SqlDbType.Time, typeof(TimeSpan)));
            Add(new SqlTypeConversionInfoCore("timestamp", null, SqlDbType.Timestamp, null));
            Add(new SqlTypeConversionInfoCore("tinyint", typeof(SqlByte), SqlDbType.TinyInt, typeof(byte)));
            Add(new SqlTypeConversionInfoCore("uniqueidentifier", typeof(SqlGuid), SqlDbType.UniqueIdentifier, typeof(Guid)));
            Add(new SqlTypeConversionInfoCore("varbinary", typeof(SqlBytes), SqlDbType.VarBinary, typeof(byte[])));
            Add(new SqlTypeConversionInfoCore("varbinary(1)", typeof(SqlBytes), SqlDbType.VarBinary, typeof(byte)));
            Add(new SqlTypeConversionInfoCore("binary(1)", typeof(SqlBytes), SqlDbType.Binary, typeof(byte)));
            Add(new SqlTypeConversionInfoCore("varchar", typeof(SqlString), SqlDbType.VarChar, typeof(string))); //this one may need work
            Add(new SqlTypeConversionInfoCore("xml", typeof(SqlXml), SqlDbType.Xml, typeof(string)));
        }

        #endregion //Constructors

        #region Methods

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoCore).FullName,
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
                return DataHelper.GetNullableType(result);
            }
            return result;
        }

        public Type GetDotNetType(Type sqlType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
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
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
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
                typeof(SqlTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoCore).FullName,
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
                return DataHelper.GetNullableType(result);
            }
            return result;
        }

        public Type GetSqlType(Type dotNetType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
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
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
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
                typeof(SqlTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqlTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqlTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public SqlDbType GetSqlDbType(string sqlTypeName)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqlTypeConversionInfoCore).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].SqlDbType;
        }

        public SqlDbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
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
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
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
                typeof(SqlTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public SqlDbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (SqlTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlDbType;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlDbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqlTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        #endregion //Methods
    }
}
