﻿namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    #endregion //Using Directives

    public class SqliteTypeConversionInfoCore
    {
        #region Constructors

        public SqliteTypeConversionInfoCore()
        {
        }

        public SqliteTypeConversionInfoCore(
            string sqlTypeName,
            Type sqlType,
            DbType sqlDbType,
            Type dotNetType)
        {
            _sqlTypeName = sqlTypeName;
            _sqlType = sqlType;
            _sqlDbType = sqlDbType;
            _dotNetType = dotNetType;
        }

        #endregion //Constructors

        #region Fields

        protected string _sqlTypeName;
        protected Type _sqlType;
        protected DbType _sqlDbType;
        protected Type _dotNetType;
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

        #endregion //Properties
    }
}
