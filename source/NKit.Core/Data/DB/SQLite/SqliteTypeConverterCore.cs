namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Data;
    using System.Text;

    #endregion //Using Directives

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.data.sqltypes.aspx
    /// </summary>
    public class SqliteTypeConverterCore : EntityCacheGeneric<string, SqliteTypeConversionInfoCore>
    {
        #region Singleton Setup

        private static SqliteTypeConverterCore _instance;

        public static SqliteTypeConverterCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SqliteTypeConverterCore();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private SqliteTypeConverterCore()
        {
            Add(new SqliteTypeConversionInfoCore("bigint", typeof(SqlInt64), DbType.Int64, typeof(Int64)));
            Add(new SqliteTypeConversionInfoCore("blob", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoCore("bit", typeof(SqlBoolean), DbType.String, typeof(Boolean)));
            Add(new SqliteTypeConversionInfoCore("char", typeof(SqlChars), DbType.String, typeof(char))); //this one may need work
            Add(new SqliteTypeConversionInfoCore("cursor", null, DbType.Object, null));
            //Add(new SqliteTypeConversionInfoCore("date", typeof(SqlDateTime), DbType.Date, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoCore("datetime", typeof(SqlDateTime), DbType.DateTime, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoCore("datetime2", typeof(SqlDateTime), DbType.DateTime2, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoCore("DATETIMEOFFSET", typeof(SqlDateTime), DbType.DateTimeOffset, typeof(DateTimeOffset)));
            Add(new SqliteTypeConversionInfoCore("decimal", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfoCore("float", typeof(SqlDouble), DbType.Double, typeof(Double)));
            //Add(new SqlTypeConversionInfo("geography", typeof(SqlGeography),typeof(null)));
            //Add(new SqlTypeConversionInfo("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfo("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new SqliteTypeConversionInfoCore("image", typeof(SqlBytes), DbType.Binary, typeof(byte[])));
            Add(new SqliteTypeConversionInfoCore("int", typeof(SqlInt32), DbType.Int32, typeof(Int32)));
            Add(new SqliteTypeConversionInfoCore("integer", typeof(SqlInt32), DbType.Int32, typeof(Int32)));
            Add(new SqliteTypeConversionInfoCore("money", typeof(SqlMoney), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfoCore("varchar", typeof(SqlString), DbType.String, typeof(string))); //this one may need work
            Add(new SqliteTypeConversionInfoCore("nchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfoCore("ntext", typeof(SqlChars), DbType.String, null));
            Add(new SqliteTypeConversionInfoCore("numeric", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfoCore("nvarchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfoCore("nvarchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfoCore("nchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfoCore("real", typeof(SqlSingle), DbType.Double, typeof(Single)));
            Add(new SqliteTypeConversionInfoCore("rowversion", null, DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoCore("smallint", typeof(SqlInt16), DbType.Int16, typeof(Int16)));
            Add(new SqliteTypeConversionInfoCore("smallmoney", typeof(SqlMoney), DbType.Decimal, typeof(Decimal)));
            //Add(new SqliteTypeConversionInfo("sql_variant", null, SqlDbType.Variant, typeof(Object)));
            Add(new SqliteTypeConversionInfoCore("table", null, DbType.Binary, null));
            Add(new SqliteTypeConversionInfoCore("text", typeof(SqlString), DbType.String, typeof(string))); //this one may need work
            Add(new SqliteTypeConversionInfoCore("time", null, DbType.Time, typeof(TimeSpan)));
            Add(new SqliteTypeConversionInfoCore("timestamp", null, DbType.DateTime, null));
            Add(new SqliteTypeConversionInfoCore("tinyint", typeof(SqlByte), DbType.Int16, typeof(Byte)));
            Add(new SqliteTypeConversionInfoCore("uniqueidentifier", typeof(SqlGuid), DbType.String, typeof(Guid)));
            Add(new SqliteTypeConversionInfoCore("varbinary", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoCore("varbinary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfoCore("binary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfoCore("xml", typeof(SqlXml), DbType.Xml, typeof(string)));
        }

        #endregion //Constructors

        #region Constants

        public const string DEFAULT_DATE_TIME_FORMAT = "yyyy/MM/dd HH:mm:ss";

        #endregion //Constants

        #region Methods

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoCore).FullName,
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
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(SqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoCore).FullName,
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
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(SqliteTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable, bool throwExceptionIfNotFound)
        {
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType) && typeInfo.DotNetType == dotNetType)
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType) &&
                    DataHelper.GetNullableType(typeInfo.DotNetType) == dotNetType)
                {
                    return typeInfo.SqlTypeName;
                }
            }
            if (throwExceptionIfNotFound)
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for .NET Type {1}.",
                    typeof(SqliteTypeConversionInfoCore).FullName,
                    dotNetType.FullName));
            }
            return null;
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(SqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public DbType GetSqlDbType(string sqlTypeName)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoCore).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].DbType;
        }

        public DbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
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
                    return typeInfo.DbType;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.DbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqliteTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public DbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (SqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.DbType;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.DbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public static string ConvertDateTimeToDefaultStringFormat(Nullable<DateTime> dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }
            return dateTime.Value.ToString(DEFAULT_DATE_TIME_FORMAT);
        }

        public static string ConvertDateTimeToDefaultStringFormat(DateTime dateTime)
        {
            return dateTime.ToString(DEFAULT_DATE_TIME_FORMAT);
        }

        #endregion //Methods
    }
}
