namespace NKit.Data.DB.LINQ
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    #endregion //Using Directives

    public class LinqFunnelSettingsWindows
    {
        #region Constructors

        public LinqFunnelSettingsWindows()
        {
        }

        public LinqFunnelSettingsWindows(string connectionString, int linqToSQLCommandTimeout)
        {
            _connectionString = connectionString;
            _linqToSQLCommandTimeout = linqToSQLCommandTimeout;
        }

        #endregion //Constructors

        #region Fields

        protected string _connectionString;
        protected int _linqToSQLCommandTimeout;

        #endregion //Fields

        #region Properties

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        public int LinqToSQLCommandTimeout
        {
            get { return _linqToSQLCommandTimeout; }
            set { _linqToSQLCommandTimeout = value; }
        }

        #endregion //Properties
    }
}