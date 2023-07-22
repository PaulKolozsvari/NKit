namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using NKit.Data;
    using System.Globalization;
    using System.IO;

    #endregion //Using Directives

    public class SqliteSettings
    {
        #region Constructors


        public SqliteSettings(string databaseFilePath) : this(databaseFilePath, null)
        {
        }

        public SqliteSettings(string databaseFilePath, DateTimeFormatInfo dateTimeFormatInfo)
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
                DataValidator.ValidateStringNotEmptyOrNull(value, nameof(DatabaseFilePath), nameof(SqliteSettings));
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
