namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Data;
    using System.Drawing;
    using NKit.Web;
    using System.Reflection;
    using NKit.Data.DB;
    using NKit.Web.Client;
    using System.Diagnostics;
    using System.ServiceModel;
    using NKit.Web.Service;
    using NKit.Utilities.Logging;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.SettingsFile;
    using NKit.Web.Client.NKitWebService;
    using NKit.Utilities.Email;
    using System.Transactions;
    using System.Windows.Forms;
    using System.Data.Linq;

    #endregion //Using Directives

    public class GOCWindows : GOC
    {
        #region Singleton Setup

        private static GOCWindows _instance;

        public static GOCWindows Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GOCWindows();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        public GOCWindows()
        {
            _settingsCache = new SettingsCache();
            _webServiceCache = new WebServiceClientCache();
            _databaseCache = new DatabaseCacheWindows();
            _logger = new LoggerWindows();
            _xSerializer = new XSerializer();
            _jSerializer = new JSerializer();
            _csvSerializer = new CsvSerializer();
            _encoding = Encoding.Default;
        }

        #endregion //Constructors

        #region Fields

        protected DatabaseCacheWindows _databaseCache;
        protected LoggerWindows _logger;
        protected Image _logo;
        protected NotifyIcon _notifyIcon;
        protected NKitWebServiceClientWrapperWindows _NKitWebServiceClient;
        protected NKitWebServiceClientWrapperWindows _NKitWebServiceClientSchema;

        #endregion //Fields

        #region Properties

        public LoggerWindows Logger
        {
            get { return _logger; }
            set{_logger = value;}
        }

        public Image Logo
        {
            get { return _logo; }
            set { _logo = value; }
        }

        public NotifyIcon NotifyIcon
        {
            get { return _notifyIcon; }
            set { _notifyIcon = value; }
        }

        public NKitWebServiceClientWrapperWindows NKitWebServiceClient
        {
            get { return _NKitWebServiceClient; }
            set { _NKitWebServiceClient = value; }
        }

        public NKitWebServiceClientWrapperWindows NKitWebServiceClientSchema
        {
            get { return _NKitWebServiceClientSchema; }
            set { _NKitWebServiceClientSchema = value; }
        }

        #endregion //Properties

        #region Methods

        public void AddDatabase<D>(D database) where D : DatabaseWindows
        {
            if (database == null)
            {
                throw new NullReferenceException(string.Format(
                    "Database to be added to {0} may not be null.",
                    this.GetType().FullName));
            }
            string databaseId = typeof(D).Name;
            if (_databaseCache.Exists(databaseId))
            {
                throw new ArgumentException(string.Format(
                    "Database with name {0} cannot be added to {1} as it has already been added.",
                    database.Name,
                    this.GetType().FullName));
            }
            else
            {
                _databaseCache.Add(databaseId, database);
            }
        }

        public void DeleteDatabase(string databaseId)
        {
            _databaseCache.Delete(databaseId);
        }

        public DatabaseWindows GetDatabase(string databaseId)
        {
            return _databaseCache[databaseId];
        }

        public D GetDatabase<D>() where D : DatabaseWindows
        {
            string databaseId = typeof(D).Name;
            D result;
            if (_databaseCache.Exists(databaseId))
            {
                result = (D)_databaseCache[databaseId];
            }
            else
            {
                result = Activator.CreateInstance<D>();
                _databaseCache.Add(result);
            }
            return result;
        }

        public void ClearDatabases()
        {
            _databaseCache.Clear();
        }

        public void SetLinqToSqlDataContextType<T>() where T : DataContext
        {
            _linqToSqlDataContextType = typeof(T);
        }

        public DataContext GetNewLinqToSqlDataContext()
        {
            return (DataContext)Activator.CreateInstance(_linqToSqlDataContextType);
        }

        #endregion //Methods
    }
}