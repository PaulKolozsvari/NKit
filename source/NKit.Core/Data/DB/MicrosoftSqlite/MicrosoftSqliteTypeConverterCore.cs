namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Data;
    using System.Text;
    using Microsoft.Data.Sqlite;

    #endregion //Using Directives

    public class MicrosoftSqliteTypeConverterCore : EntityCacheGeneric<string, MicrosoftSqliteTypeConversionInfoCore>
    {
        #region Singleton Setup

        private static MicrosoftSqliteTypeConverterCore _instance;

        public static MicrosoftSqliteTypeConverterCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MicrosoftSqliteTypeConverterCore();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private MicrosoftSqliteTypeConverterCore()
        {
            Add(new MicrosoftSqliteTypeConversionInfoCore(sqlTypeName: "bigint", sqlType: typeof(SqlInt64), sqlDbType: DbType.Int64, dotNetType: typeof(Int64), SqliteType.Integer));
            Add(new MicrosoftSqliteTypeConversionInfoCore("blob", typeof(SqlBytes), DbType.Binary, typeof(Byte[]), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("bit", typeof(SqlBoolean), DbType.String, typeof(Boolean), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("char", typeof(SqlChars), DbType.String, typeof(char), SqliteType.Text)); //this one may need work
            Add(new MicrosoftSqliteTypeConversionInfoCore("cursor", null, DbType.Object, null, SqliteType.Integer));
            //Add(new MicrosoftSqliteTypeConversionInfoCore("date", typeof(SqlDateTime), DbType.Date, typeof(DateTime)));
            Add(new MicrosoftSqliteTypeConversionInfoCore("datetime", typeof(SqlDateTime), DbType.DateTime, typeof(DateTime), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("datetime2", typeof(SqlDateTime), DbType.DateTime2, typeof(DateTime), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("datetimeoffset", typeof(SqlDateTime), DbType.DateTimeOffset, typeof(DateTimeOffset), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("decimal", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal), SqliteType.Real));
            Add(new MicrosoftSqliteTypeConversionInfoCore("float", typeof(SqlDouble), DbType.Double, typeof(Double), SqliteType.Real));
            //Add(new SqlTypeConversionInfo("geography", typeof(SqlGeography),typeof(null)));
            //Add(new SqlTypeConversionInfo("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfo("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new MicrosoftSqliteTypeConversionInfoCore("image", typeof(SqlBytes), DbType.Binary, typeof(byte[]), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("int", typeof(SqlInt32), DbType.Int32, typeof(Int32), SqliteType.Integer));
            Add(new MicrosoftSqliteTypeConversionInfoCore("integer", typeof(SqlInt32), DbType.Int32, typeof(Int32), SqliteType.Integer));
            Add(new MicrosoftSqliteTypeConversionInfoCore("money", typeof(SqlMoney), DbType.Decimal, typeof(Decimal), SqliteType.Real));
            Add(new MicrosoftSqliteTypeConversionInfoCore("varchar", typeof(SqlString), DbType.String, typeof(string), SqliteType.Text)); //this one may need work
            Add(new MicrosoftSqliteTypeConversionInfoCore("nchar", typeof(SqlChars), DbType.String, typeof(String), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("ntext", typeof(SqlChars), DbType.String, typeof(String), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("numeric", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal), SqliteType.Real));
            Add(new MicrosoftSqliteTypeConversionInfoCore("nvarchar", typeof(SqlChars), DbType.String, typeof(String), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("nvarchar(1)", typeof(SqlChars), DbType.String, typeof(Char), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("nchar(1)", typeof(SqlChars), DbType.String, typeof(Char), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("real", typeof(SqlSingle), DbType.Double, typeof(Single), SqliteType.Real));
            Add(new MicrosoftSqliteTypeConversionInfoCore("rowversion", null, DbType.Binary, typeof(Byte[]), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("smallint", typeof(SqlInt16), DbType.Int16, typeof(Int16), SqliteType.Integer));
            Add(new MicrosoftSqliteTypeConversionInfoCore("smallmoney", typeof(SqlMoney), DbType.Decimal, typeof(Decimal), SqliteType.Real));
            //Add(new SqliteTypeConversionInfo("sql_variant", null, SqlDbType.Variant, typeof(Object)));
            Add(new MicrosoftSqliteTypeConversionInfoCore("table", null, DbType.Binary, null, SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("text", typeof(SqlString), DbType.String, typeof(string), SqliteType.Text)); //this one may need work
            Add(new MicrosoftSqliteTypeConversionInfoCore("time", null, DbType.Time, typeof(TimeSpan), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("timestamp", null, DbType.DateTime, null, SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("tinyint", typeof(SqlByte), DbType.Int16, typeof(Byte), SqliteType.Integer));
            Add(new MicrosoftSqliteTypeConversionInfoCore("uniqueidentifier", typeof(SqlGuid), DbType.String, typeof(Guid), SqliteType.Text));
            Add(new MicrosoftSqliteTypeConversionInfoCore("varbinary", typeof(SqlBytes), DbType.Binary, typeof(Byte[]), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("varbinary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("binary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte), SqliteType.Blob));
            Add(new MicrosoftSqliteTypeConversionInfoCore("xml", typeof(SqlXml), DbType.Xml, typeof(string), SqliteType.Text));
        }

        #endregion //Constructors

        #region Constants

        public const string DEFAULT_DATE_TIME_FORMAT = "yyyy/MM/dd HH:mm:ss";

        #endregion //Constants

        #region Methods

        #region Get .NET Type

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            sqlTypeName = sqlTypeName != null ? sqlTypeName.ToLower() : string.Empty;
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
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
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public Type GetDotNetType(SqliteType microsoftSqliteType, bool isNullable)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.MicrosoftSqliteType == microsoftSqliteType)
                {
                    return typeInfo.DotNetType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for Microsoft SQLite Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                microsoftSqliteType.ToString()));
        }

        #endregion //Get .NET Type

        #region Get Sql Type

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            sqlTypeName = sqlTypeName != null ? sqlTypeName.ToLower() : string.Empty;
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
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
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public Type GetSqlType(SqliteType microsoftSqliteType, bool isNullable)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.MicrosoftSqliteType == microsoftSqliteType)
                {
                    return typeInfo.SqlType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for Microsoft SQLite Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                microsoftSqliteType.ToString()));
        }

        #endregion //Get Sql Type

        #region Get Sql Type Name

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable, bool throwExceptionIfNotFound)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                    typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                    dotNetType.FullName));
            }
            return null;
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public string GetSqlTypeNameFromMicrosoftSqliteType(SqliteType microsoftSqliteType, bool isNullable)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.MicrosoftSqliteType == microsoftSqliteType)
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for Microsoft SQLite Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                microsoftSqliteType.ToString()));
        }

        #endregion //Get Sql Type Name

        #region Get Sql Db Type

        public DbType GetSqlDbType(string sqlTypeName)
        {
            sqlTypeName = sqlTypeName != null ? sqlTypeName.ToLower() : string.Empty;
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].DbType;
        }

        public DbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public DbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
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
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        public DbType GetSqlDbTypeFromMicrosoftSqliteType(SqliteType microsoftSqliteType)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.MicrosoftSqliteType == microsoftSqliteType)
                {
                    return typeInfo.DbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                microsoftSqliteType.ToString()));
        }

        #endregion //Get Sql Db Type

        #region Get Microsoft Sqlite Type

        public SqliteType GetMicrosoftSqliteType(string sqlTypeName)
        {
            sqlTypeName = sqlTypeName != null ? sqlTypeName.ToLower() : string.Empty;
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].MicrosoftSqliteType;
        }

        public SqliteType GetMicrosoftSqliteTypeFromDotNetType(Type dotNetType)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    return typeInfo.MicrosoftSqliteType;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    return typeInfo.MicrosoftSqliteType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                dotNetType.FullName));
        }

        public SqliteType GetMicrosoftSqliteTypeFromSqlType(Type sqlType)
        {
            foreach (MicrosoftSqliteTypeConversionInfoCore typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.MicrosoftSqliteType;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.MicrosoftSqliteType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQL Type {1}.",
                typeof(MicrosoftSqliteTypeConversionInfoCore).FullName,
                sqlType.FullName));
        }

        #endregion //Get Microsoft Sqlite Type

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
