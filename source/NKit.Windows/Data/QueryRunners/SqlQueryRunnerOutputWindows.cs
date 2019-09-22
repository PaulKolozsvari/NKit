namespace NKit.Data.QueryRunners
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    [Serializable]
    public class SqlQueryRunnerOutputWindows
    {
        #region Constructors

        public SqlQueryRunnerOutputWindows()
        {
        }

        public SqlQueryRunnerOutputWindows(
            bool success,
            string resultMessage)
        {
            _success = success;
            _resultMessage = resultMessage;
        }

        #endregion //Constructors

        #region Fields

        protected bool _success;
        protected string _resultMessage;

        #endregion //Fields

        #region Properties

        public bool Success
        {
            get { return _success; }
        }

        public string ResultMessage
        {
            get { return _resultMessage; }
        }

        #endregion //Properties
    }
}