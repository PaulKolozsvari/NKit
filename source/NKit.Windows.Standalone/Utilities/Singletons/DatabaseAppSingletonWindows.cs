namespace NKit.Utilities.Singletons
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.LINQ;
    using NKit.Utilities.SettingsFile.Default;

    #endregion //Using Directives

    public class DatabaseAppSingletonWindows<E> : AppSingletonWindows<E> where E : class
    {
        #region Methods

        protected void InitializeAllDefaultSettings<D>(
            DatabaseAppSettings settings,
            Type userLinqToSqlType,
            Type serverActionLinqToSqlType,
            Type serverErrorLinqToSqlType,
            bool logSettings) where D : DataContext
        {
            base.InitializeAllDefaultSettings(settings, logSettings);
            InitializeDatabaseSettings<D>(settings, userLinqToSqlType, serverActionLinqToSqlType, serverErrorLinqToSqlType);
        }

        protected virtual void InitializeDatabaseSettings<D>(
            DatabaseAppSettings settings, 
            Type userLinqToSqlType,
            Type serverActionLinqToSqlType,
            Type serverErrorLinqToSqlType) where D : DataContext
        {
            LinqFunnelSettingsWindows linqFunnelSettings = new LinqFunnelSettingsWindows(settings.DatabaseConnectionString, settings.DatabaseCommandTimeout);
            GOCWindows.Instance.AddByTypeName(linqFunnelSettings); //Adds an object to Global Object Cache with the key being the name of the type.
            string linqToSqlAssemblyFilePath = Path.Combine(InformationWindows.GetExecutingDirectory(), settings.LinqToSQLClassesAssemblyFileName);

            //Grab the LinqToSql context from the specified assembly and load it in the GOC to be used from anywhere in the application.
            //The point of doing this is that you can rebuild the context if the schema changes and reload without having to reinstall the web service. You just stop the service and overwrite the EOH.RainMaker.ORM.dll with the new one.
            //It also allows the NKit Service Toolkit to be business data agnostic.
            GOCWindows.Instance.LinqToClassesAssembly = Assembly.LoadFrom(linqToSqlAssemblyFilePath);
            GOCWindows.Instance.LinqToSQLClassesNamespace = settings.LinqToSQLClassesNamespace;
            GOCWindows.Instance.SetLinqToSqlDataContextType<D>();
            if (userLinqToSqlType != null)
            {
                GOCWindows.Instance.UserLinqToSqlType = userLinqToSqlType;
            }
            if (serverActionLinqToSqlType != null)
            {
                GOCWindows.Instance.ServerActionLinqToSqlType = serverActionLinqToSqlType;
            }
            if (serverErrorLinqToSqlType != null)
            {
                GOCWindows.Instance.ServerErrorLinqToSqlType = serverErrorLinqToSqlType;
            }
            GOCWindows.Instance.DatabaseTransactionScopeOption = settings.DatabaseTransactionScopeOption;
            GOCWindows.Instance.DatabaseTransactionIsolationLevel = settings.DatabaseTransactionIsolationLevel;
            GOCWindows.Instance.DatabaseTransactionTimeoutSeconds = settings.DatabaseTransactionTimeoutSeconds;
            GOCWindows.Instance.DatabaseTransactionDeadlockRetryAttempts = settings.DatabaseTransactionDeadlockRetryAttempts;
            GOCWindows.Instance.DatabaseTransactionDeadlockRetryWaitPeriod = settings.DatabaseTransactionDeadlockRetryWaitPeriod;
        }

        #endregion //Methods
    }
}
