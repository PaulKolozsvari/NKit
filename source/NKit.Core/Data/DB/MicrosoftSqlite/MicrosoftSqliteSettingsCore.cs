namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security.Policy;
    using System.Text;
    using Microsoft.Data.Sqlite;

    #endregion //Using Directives

    public class MicrosoftSqliteSettingsCore
    {
        #region Constructors


        public MicrosoftSqliteSettingsCore(string databaseFilePath) : this(databaseFilePath, null)
        {
        }

        public MicrosoftSqliteSettingsCore(string databaseFilePath, DateTimeFormatInfo dateTimeFormatInfo)
        {
            _databaseFilePath = databaseFilePath;
            string extension = Path.GetExtension(databaseFilePath);
            if (!extension.Contains(".sqlite") && !extension.Contains(".db") && !extension.Contains(".s3db") && !extension.Contains(".sl3"))
            {
                _databaseFilePath += ".sqlite";
            }
            _databaseFileName = Path.GetFileName(_databaseFilePath);
            _databaseDirectory = Path.GetDirectoryName(databaseFilePath);

            /*
             * Connection string when using System.Data.SQLite
            _connectionString = $"URI=file:{databaseFilePath}";
            _connectionString += ";DateTimeFormat=yyyy/MM/dd hh:mm:ss";
            */

            //Microsoft.Data.Sqlite connection strings: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings
            //In-memory database: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/in-memory-databases

            var connectionStringBuilder = new SqliteConnectionStringBuilder()
            {
                DataSource = databaseFilePath,
                Mode = SqliteOpenMode.ReadWriteCreate, //Will create the database if it doesn't already exist.
                //Password = null,
                Cache = SqliteCacheMode.Private,
                Pooling = true, //Setting this to true keeps the connection alive and even after connection is closed the process will have a handle: https://github.com/dotnet/efcore/issues/27139
            };
            _connectionString = connectionStringBuilder.ToString();
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
                DataValidator.ValidateStringNotEmptyOrNull(value, nameof(DatabaseFilePath), nameof(MicrosoftSqliteSettingsCore));
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
