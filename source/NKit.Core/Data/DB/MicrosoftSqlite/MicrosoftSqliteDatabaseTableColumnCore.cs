namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Microsoft.Data.Sqlite;

    #endregion //Using Directives

    public class MicrosoftSqliteDatabaseTableColumnCore : DatabaseTableColumnCore
    {
        #region Constructors

        public MicrosoftSqliteDatabaseTableColumnCore()
        {
        }

        public MicrosoftSqliteDatabaseTableColumnCore(DataRow schemaRow) : base(schemaRow)
        {
        }

        #endregion //Constructors

        #region Constants

        public const string COLUMN_NAME_SCHEMA_ATTRIBUTE = "column_name";
        public const string ORDINAL_POSITION_SCHEMA_ATTRIBUTE = "ordinal_position";
        public const string COLUMN_DEFAULT_SCHEMA_ATTRIBUTE = "column_default";
        public const string IS_NULLABLE_SCHEMA_ATTRIBUTE = "is_nullable";
        public const string DATA_TYPE_SCHEMA_ATTRIBUTE = "data_type";
        public const string PRIMARY_KEY_SCHEMA_ATTRIBUTE = "PRIMARY_KEY";
        public const string UNIQUE_SCHEMA_ATTRIBUTE = "UNIQUE";

        #endregion //Constants

        #region Fields

        protected DbType _sqlDbType;
        protected SqliteType _microsoftSqliteType;

        #endregion //Fields

        #region Properties

        public DbType SqlDbType
        {
            get { return _sqlDbType; }
            set { _sqlDbType = value; }
        }

        public SqliteType MicrosoftSqliteType
        {
            get { return _microsoftSqliteType; }
            set { _microsoftSqliteType = value; }
        }

        #endregion //Properties

        #region Methods

        public override void PopulateFromSchema(DataRow schemaRow)
        {
            _columnName = schemaRow[COLUMN_NAME_SCHEMA_ATTRIBUTE].ToString();
            _ordinalPosition = Convert.ToInt16(schemaRow[ORDINAL_POSITION_SCHEMA_ATTRIBUTE]);
            _columnDefault = schemaRow[COLUMN_DEFAULT_SCHEMA_ATTRIBUTE].ToString();
            _isNullable = schemaRow[IS_NULLABLE_SCHEMA_ATTRIBUTE].ToString();
            _dataType = schemaRow[DATA_TYPE_SCHEMA_ATTRIBUTE].ToString();
            _sqlDbType = MicrosoftSqliteTypeConverterCore.Instance.GetSqlDbType(_dataType);
            _isKey = Convert.ToBoolean(schemaRow[PRIMARY_KEY_SCHEMA_ATTRIBUTE]);
        }

        #endregion //Methods
    }
}
