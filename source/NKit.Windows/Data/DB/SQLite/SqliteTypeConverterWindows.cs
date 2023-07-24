namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlTypes;
    using System.Data;
    using System.Reflection;
    using System.Runtime.InteropServices;

    #endregion //Using Directives

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.data.sqltypes.aspx
    /// </summary>
    public class SqliteTypeConverterWindows : EntityCacheGeneric<string, SqliteTypeConversionInfoWindows>
    {
        #region Singleton Setup

        private static SqliteTypeConverterWindows _instance;

        public static SqliteTypeConverterWindows Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SqliteTypeConverterWindows();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private SqliteTypeConverterWindows()
        {
            Add(new SqliteTypeConversionInfoWindows("bigint", typeof(SqlInt64), DbType.Int64, typeof(Int64)));
            Add(new SqliteTypeConversionInfoWindows("binary", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoWindows("bit", typeof(SqlBoolean), DbType.String, typeof(Boolean)));
            Add(new SqliteTypeConversionInfoWindows("char", typeof(SqlChars), DbType.String, typeof(char))); //this one may need work
            Add(new SqliteTypeConversionInfoWindows("cursor", null, DbType.Object, null));
            //Add(new SqliteTypeConversionInfoWindows("date", typeof(SqlDateTime), DbType.Date, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoWindows("datetime", typeof(SqlDateTime), DbType.DateTime, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoWindows("datetime2", typeof(SqlDateTime), DbType.DateTime2, typeof(DateTime)));
            Add(new SqliteTypeConversionInfoWindows("DATETIMEOFFSET", typeof(SqlDateTime), DbType.DateTimeOffset,typeof(DateTimeOffset)));
            Add(new SqliteTypeConversionInfoWindows("decimal", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfoWindows("float", typeof(SqlDouble), DbType.Double, typeof(Double)));
            //Add(new SqlTypeConversionInfo("geography", typeof(SqlGeography),typeof(null)));
            //Add(new SqlTypeConversionInfo("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfo("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new SqliteTypeConversionInfoWindows("image", typeof(SqlBytes), DbType.Binary, typeof(byte[])));
            Add(new SqliteTypeConversionInfoWindows("int", typeof(SqlInt32), DbType.Int32, typeof(Int32)));
            Add(new SqliteTypeConversionInfoWindows("integer", typeof(SqlInt32), DbType.Int32, typeof(Int32)));
            Add(new SqliteTypeConversionInfoWindows("money", typeof(SqlMoney), DbType.Decimal, typeof(Decimal)));
			Add(new SqliteTypeConversionInfoWindows("varchar", typeof(SqlString), DbType.String, typeof(string))); //this one may need work
			Add(new SqliteTypeConversionInfoWindows("nchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfoWindows("ntext", typeof(SqlChars), DbType.String, null));
            Add(new SqliteTypeConversionInfoWindows("numeric", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfoWindows("nvarchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfoWindows("nvarchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfoWindows("nchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfoWindows("real", typeof(SqlSingle), DbType.Double, typeof(Single)));
            Add(new SqliteTypeConversionInfoWindows("rowversion", null, DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoWindows("smallint", typeof(SqlInt16), DbType.Int16, typeof(Int16)));
            Add(new SqliteTypeConversionInfoWindows("smallmoney", typeof(SqlMoney), DbType.Decimal,typeof(Decimal)));
            //Add(new SqliteTypeConversionInfo("sql_variant", null, SqlDbType.Variant, typeof(Object)));
            Add(new SqliteTypeConversionInfoWindows("table", null, DbType.Binary, null));
            Add(new SqliteTypeConversionInfoWindows("text", typeof(SqlString), DbType.String,typeof(string))); //this one may need work
            Add(new SqliteTypeConversionInfoWindows("time", null, DbType.Time, typeof(TimeSpan)));
            Add(new SqliteTypeConversionInfoWindows("timestamp", null, DbType.DateTime, null));
            Add(new SqliteTypeConversionInfoWindows("tinyint", typeof(SqlByte), DbType.Int16, typeof(Byte)));
            Add(new SqliteTypeConversionInfoWindows("uniqueidentifier", typeof(SqlGuid), DbType.String, typeof(Guid)));
            Add(new SqliteTypeConversionInfoWindows("varbinary", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfoWindows("varbinary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfoWindows("binary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfoWindows("xml", typeof(SqlXml), DbType.Xml, typeof(string)));
        }

        #endregion //Constructors

        #region Methods

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoWindows).FullName,
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
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                typeof(SqliteTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoWindows).FullName,
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
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                typeof(SqliteTypeConversionInfoWindows).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable, bool throwExceptionIfNotFound)
        {
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                    typeof(SqliteTypeConversionInfoWindows).FullName,
                    dotNetType.FullName));
            }
            return null;
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                typeof(SqliteTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        public DbType GetSqlDbType(string sqlTypeName)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfoWindows).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].DbType;
        }

        public DbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                typeof(SqliteTypeConversionInfoWindows).FullName,
                dotNetType.FullName));
        }

        public DbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (SqliteTypeConversionInfoWindows typeInfo in this)
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
                typeof(SqliteTypeConversionInfoWindows).FullName,
                sqlType.FullName));
        }

        #endregion //Methods
    }
}
