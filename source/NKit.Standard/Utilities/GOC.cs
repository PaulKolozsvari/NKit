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
    using NKit.Web.Client;
    using System.Diagnostics;
    using NKit.Utilities.Logging;
    using NKit.Utilities.Serialization;
    using NKit.Utilities.Email;
    using System.Transactions;

    #endregion //Using Directives

    public partial class GOC : EntityCacheGeneric<string, object>
    {
        #region Singleton Setup

        private static GOC _instance;

        public static GOC Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GOC();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        public GOC()
        {
            _webServiceCache = new WebServiceClientCache();
            _loggerStandard = new Logger();
            _xSerializer = new XSerializer();
            _jSerializer = new JSerializer();
            _csvSerializer = new CsvSerializer();
            _encoding = Encoding.Default;
        }

        #endregion //Constructors

        #region Fields

        protected string _applicationName;
        protected string _executableName;
        protected string _version;
        protected string _userAgent;
        protected WebServiceClientCache _webServiceCache;
        protected Logger _loggerStandard;
        protected XSerializer _xSerializer;
        protected JSerializer _jSerializer;
        protected CsvSerializer _csvSerializer;
        protected bool _showWindowsMessageBoxOnException;
        protected bool _showWindowsFormsNotificationOnException;
        protected Encoding _encoding;

        protected Assembly _linqToClassesAssembly;
        protected string _linqToSQLClassesNamespace;
        protected Type _linqToSqlDataContextType;
        protected bool _useTransactionsOnCrudOperations;
        protected TransactionScopeOption _databaseTransactionScopeOption;
        protected IsolationLevel _databaseTransactionIsolationLevel;
        protected int _databaseTransactionTimeoutSeconds;
        protected int _databaseTransactionDeadlockRetryAttempts;
        protected int _transactionDeadlockRetryWaitPeriod;
        protected Type _userLinqToSqlType;
        protected Type _serverActionLinqToSqlType;
        protected Type _serverErrorLinqtoSqlType;

        #region Email

        protected bool _emailNotificationsEnabled;
        protected bool _throwEmailFailExceptions;
        protected EmailProvider _emailProvider;
        protected string _gMailSMTPServer;
        protected string _gMailSmtpUserName;
        protected string _gMailSmtpPassword;
        protected int _gMailSmtpPort;
        protected string _senderEmailAddress;
        protected string _senderDisplayName;
        protected string _exceptionEmailSubject;
        protected bool _emailExceptions;
        protected bool _appendHostNameToEmailBody;
        protected bool _logEmails;
        protected bool _includeDefaultEmailRecipients;
        protected List<EmailNotificationRecipient> _defaultEmailRecipients;
        protected EmailClient _emailClient;

        #endregion //Email

        #endregion //Fields

        #region Properties

        public string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public string ExecutableName
        {
            get { return _executableName; }
            set { _executableName = value; }
        }

        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        public Logger Logger
        {
            get { return _loggerStandard; }
            set{_loggerStandard = value;}
        }

        public XSerializer XmlSerializer
        {
            get { return _xSerializer; }
            set { _xSerializer = value; }
        }

        public JSerializer JsonSerializer
        {
            get { return _jSerializer; }
            set { _jSerializer = value; }
        }

        public CsvSerializer CSVSerializer
        {
            get { return _csvSerializer; }
            set { _csvSerializer = value; }
        }

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public bool ShowMessageBoxOnException
        {
            get { return _showWindowsMessageBoxOnException; }
            set { _showWindowsMessageBoxOnException = value; }
        }

        public bool ShowWindowsFormsNotificationOnException
        {
            get { return _showWindowsFormsNotificationOnException; }
            set { _showWindowsFormsNotificationOnException = value; }
        }

        public Assembly LinqToClassesAssembly
        {
            get { return _linqToClassesAssembly; }
            set { _linqToClassesAssembly = value; }
        }

        public string LinqToSQLClassesNamespace
        {
            get { return _linqToSQLClassesNamespace; }
            set { _linqToSQLClassesNamespace = value; }
        }

        public bool UseTransactionsOnCrudOperations
        {
            get{return _useTransactionsOnCrudOperations; }
            set { _useTransactionsOnCrudOperations = value; }
        }

        public TransactionScopeOption DatabaseTransactionScopeOption
        {
            get { return _databaseTransactionScopeOption; }
            set { _databaseTransactionScopeOption = value; }
        }

        public IsolationLevel DatabaseTransactionIsolationLevel
        {
            get { return _databaseTransactionIsolationLevel; }
            set { _databaseTransactionIsolationLevel = value; }
        }

        public int DatabaseTransactionTimeoutSeconds
        {
            get { return _databaseTransactionTimeoutSeconds; }
            set { _databaseTransactionTimeoutSeconds = value; }
        }

        public int DatabaseTransactionDeadlockRetryAttempts
        {
            get { return _databaseTransactionDeadlockRetryAttempts; }
            set { _databaseTransactionDeadlockRetryAttempts = value; }
        }

        public int DatabaseTransactionDeadlockRetryWaitPeriod
        {
            get { return _transactionDeadlockRetryWaitPeriod; }
            set { _transactionDeadlockRetryWaitPeriod = value; }
        }

        public Type UserLinqToSqlType
        {
            get { return _userLinqToSqlType; }
            set { _userLinqToSqlType = value; }
        }

        public Type ServerActionLinqToSqlType
        {
            get { return _serverActionLinqToSqlType; }
            set { _serverActionLinqToSqlType = value; }
        }

        public Type ServerErrorLinqToSqlType
        {
            get { return _serverErrorLinqtoSqlType; }
            set { _serverErrorLinqtoSqlType = value; }
        }

        #region Email

        public bool SendEmailOnException
        {
            get { return _emailExceptions; }
            set { _emailExceptions = value; }
        }

        public bool AppendHostNameToExceptionEmails
        {
            get { return _appendHostNameToEmailBody; }
            set { _appendHostNameToEmailBody = value; }
        }

        public EmailClient EmailClient
        {
            get { return _emailClient; }
            set { _emailClient = value; }
        }

        #endregion //Email

        #endregion //Properties

        #region Methods

        public void AddByTypeName(object e)
        {
            string id = e.GetType().Name;
            if (Exists(id))
            {
                throw new ArgumentException(string.Format(
                    "Already added entity with type name {0}.", 
                    id));
            }
            Add(id, e);
        }

        public void DeleteByTypeName<E>()
        {
            string id = typeof(E).Name;
            if (!Exists(id))
            {
                throw new ArgumentException(string.Format(
                    "Could not find entity with type name {0} to delete.",
                    id));
            }
            Delete(id);
        }

        public bool ExistsByTypeName<E>()
        {
            string id = typeof(E).Name;
            return Exists(id);
        }

        public E GetByTypeName<E>()
        {
            string id = typeof(E).Name;
            if (!Exists(id))
            {
                return default(E);
            }
            return (E)this[id];
        }

        public W GetWebService<W>() where W : IWebService
        {
            return GetWebService<W>(null);
        }

        public W GetWebService<W>(bool validateBaseUrlIsSet) where W : IWebService
        {
            W result = GetWebService<W>(null);
            if (validateBaseUrlIsSet && string.IsNullOrEmpty(result.WebServiceBaseUrl))
            {
                throw new NullReferenceException(string.Format("Web Service Base URL for {0} is not set.", typeof(W).Name));
            }
            return result;
        }

        public W GetWebService<W>(string webServiceBaseUrl) where W : IWebService
        {
            string webServiceId = typeof(W).Name;
            W result;
            if (_webServiceCache.Exists(webServiceId))
            {
                result = (W)_webServiceCache[webServiceId];
            }
            else
            {
                result = Activator.CreateInstance<W>();
                _webServiceCache.Add(webServiceId, result);
            }
            if (!string.IsNullOrEmpty(webServiceBaseUrl))
            {
                result.WebServiceBaseUrl = webServiceBaseUrl;
            }
            return result;
        }

        public ISerializer GetSerializer(SerializerType serializerType)
        {
            switch (serializerType)
            {
                case SerializerType.XML:
                    return _xSerializer;
                case SerializerType.JSON:
                    return _jSerializer;
                case SerializerType.CSV:
                    return _csvSerializer;
                default:
                    throw new ArgumentException(string.Format(
                        "{0} is not a valid {1}.",
                        serializerType.ToString(),
                        serializerType.GetType().FullName));
            }
        }

        public void SetEncoding(TextEncodingType textEncodingType)
        {
            switch (textEncodingType)
            {
                case TextEncodingType.UTF8:
                    _encoding = Encoding.UTF8;
                    break;
                case TextEncodingType.ASCII:
                    _encoding = Encoding.ASCII;
                    break;
                case TextEncodingType.BigEndianUnicode:
                    _encoding = Encoding.BigEndianUnicode;
                    break;
                case TextEncodingType.Default:
                    _encoding = Encoding.Default;
                    break;
                case TextEncodingType.Unicode:
                    _encoding = Encoding.Unicode;
                    break;
                case TextEncodingType.UTF32:
                    _encoding = Encoding.UTF32;
                    break;
                case TextEncodingType.UTF7:
                    _encoding = Encoding.UTF7;
                    break;
                default:
                    break;
            }
        }

        #endregion //Methods
    }
}