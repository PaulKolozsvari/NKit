namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Microsoft.Data.Sqlite;

    #endregion //Using Directives

    public class MicrosoftSqliteTypeConversionInfoCore
    {
        #region Constructors

        public MicrosoftSqliteTypeConversionInfoCore()
        {
        }

        public MicrosoftSqliteTypeConversionInfoCore(
            string sqlTypeName,
            Type sqlType,
            DbType sqlDbType,
            Type dotNetType,
            SqliteType microsoftSqliteType)
        {
            _sqlTypeName = sqlTypeName;
            _sqlType = sqlType;
            _sqlDbType = sqlDbType;
            _dotNetType = dotNetType;
            _microsoftSqliteType = microsoftSqliteType;
        }

        #endregion //Constructors

        #region Fields

        protected string _sqlTypeName;
        protected Type _sqlType;
        protected DbType _sqlDbType;
        protected Type _dotNetType;
        protected SqliteType _microsoftSqliteType;
        protected bool _isNUllable;

        #endregion //Fields

        #region Properties

        public string SqlTypeName
        {
            get { return _sqlTypeName; }
        }

        public Type SqlType
        {
            get { return _sqlType; }
        }

        public Type DotNetType
        {
            get { return _dotNetType; }
        }

        public DbType DbType
        {
            get { return _sqlDbType; }
        }

        public bool IsNullable
        {
            get { return _isNUllable; }
        }

        public SqliteType MicrosoftSqliteType
        {
            get { return _microsoftSqliteType; }
        }

        #endregion //Properties
    }
}
