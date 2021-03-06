﻿namespace NKit.Data.QueryRunners
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Utilities;

    #endregion //Using Directives

    [Serializable]
    public class SqlQueryRunnerDispatcherWindows
    {
        #region Constructors

        public SqlQueryRunnerDispatcherWindows()
        {
        }

        public SqlQueryRunnerDispatcherWindows(SqlQueryRunnerConfigWindows sqlQueryRunnerConfig)
        {
            _sqlQueryRunnerConfig = sqlQueryRunnerConfig;
        }

        #endregion //Constructors

        #region Fields

        //public const string QUERY_RUNNER_APP_DOMAIN_NAME = "QueryRunnerDomain";
        public const string SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_NAME = "ExecuteQuery";
        public const string SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_RESULT_NAME = "ExecuteQueryResult";

        #endregion //Fields

        #region Fields

        protected SqlQueryRunnerConfigWindows _sqlQueryRunnerConfig;

        #endregion //Fields

        #region Methods

        public string DispatchSqlQueryRunner(
            string ormAssemblyName,
            string ormTypeName,
            string sqlQueryString,
            string acceptContentType,
            string connectionString,
            bool includeOrmTypeNamesInJsonResponse)
        {
            AppDomain queryRunnerDomain = null;
            try
            {
                SqlQueryRunnerInputWindows input = new SqlQueryRunnerInputWindows(
                    ormAssemblyName,
                    ormTypeName,
                    sqlQueryString,
                    acceptContentType,
                    connectionString,
                    includeOrmTypeNamesInJsonResponse);
                AppDomainSetup domainSetup = new AppDomainSetup()
                { 
                    ApplicationBase = Path.GetDirectoryName(_sqlQueryRunnerConfig.SqlQueryRunnerAssemblyPath) 
                };
                queryRunnerDomain = AppDomain.CreateDomain(DataShaperWindows.GetUniqueIdentifier(), null, domainSetup);
                queryRunnerDomain.SetData(SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_NAME, input);
                queryRunnerDomain.DoCallBack(new CrossAppDomainDelegate(ExecuteSqlQueryRunnerInAnotherDomain));
                SqlQueryRunnerOutputWindows result = (SqlQueryRunnerOutputWindows)queryRunnerDomain.GetData(SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_RESULT_NAME);
                if (!result.Success)
                {
                    throw new Exception(result.ResultMessage);
                }
                return result.ResultMessage;
            }
            finally
            {
                if (queryRunnerDomain != null)
                {
                    AppDomain.Unload(queryRunnerDomain);
                }
            }
        }

        private void ExecuteSqlQueryRunnerInAnotherDomain()
        {
            try
            {
                SqlQueryRunnerInputWindows input = (SqlQueryRunnerInputWindows)AppDomain.CurrentDomain.GetData(SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_NAME);
                Assembly queryRunnerAssembly = AppDomain.CurrentDomain.Load(_sqlQueryRunnerConfig.QueryRunnerAssemblyBytes);
                Type sqlQueryRunnerType = queryRunnerAssembly.GetType(_sqlQueryRunnerConfig.SqlQueryRunnerFullTypeName);
                ISqlQueryRunnerWindows sqlQueryRunner = Activator.CreateInstance(sqlQueryRunnerType) as ISqlQueryRunnerWindows;
                if (sqlQueryRunner == null)
                {
                    throw new InvalidCastException(string.Format(
                        "Specified SQL Query runner type {0} does not implement the interface {1}.",
                        _sqlQueryRunnerConfig.SqlQueryRunnerFullTypeName,
                        typeof(ISqlQueryRunnerWindows).FullName));
                }
                SqlQueryRunnerOutputWindows output = sqlQueryRunner.ExecuteQuery(input);
                AppDomain.CurrentDomain.SetData(
                    SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_RESULT_NAME, 
                    output);
            }
            catch (Exception ex)
            {
                AppDomain.CurrentDomain.SetData(
                    SQL_QUERY_RUNNER_EXECUTE_QUERY_METHOD_RESULT_NAME, 
                    new SqlQueryRunnerOutputWindows(false, ex.Message));
            }
        }

        #endregion //Methods
    }
}