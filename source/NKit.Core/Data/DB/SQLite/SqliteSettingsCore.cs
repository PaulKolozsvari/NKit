namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using Microsoft.EntityFrameworkCore.Sqlite.Internal;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    #endregion //Using Directives

    public class SqliteSettingsCore
    {
        #region Constructors


        public SqliteSettingsCore(string databaseFilePath) : this(databaseFilePath, null)
        {
        }

        public SqliteSettingsCore(string databaseFilePath, DateTimeFormatInfo dateTimeFormatInfo)
        {
            _databaseFilePath = databaseFilePath;
            string extension = Path.GetExtension(databaseFilePath);
            if (!extension.Contains(".sqlite") && !extension.Contains(".db") && !extension.Contains(".s3db") && !extension.Contains(".sl3"))
            {
                _databaseFilePath += ".sqlite";
            }
            _databaseFileName = Path.GetFileName(_databaseFilePath);
            _databaseDirectory = Path.GetDirectoryName(databaseFilePath);
            _connectionString = $"URI=file:{databaseFilePath}";
            _connectionString += ";DateTimeFormat=yyyy/MM/dd hh:mm:ss";
        }

        #endregion //Constructors

        #region Fields

        private string _databaseFilePath;
        private string _databaseFileName;
        private string _databaseDirectory;
        private string _connectionString;

        #endregion //Fields

        #region Properties

        public string DatabaseFilePath
        {
            get { return _databaseFilePath; }
            set
            {
                DataValidator.ValidateStringNotEmptyOrNull(value, nameof(DatabaseFilePath), nameof(SqliteSettingsCore));
                _databaseFilePath = value;
            }
        }

        public string DatabaseFileName
        {
            get { return _databaseFileName; }
        }

        public string DatabaseDirectory
        {
            get { return _databaseDirectory; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        #endregion //Properties        
    }
}
