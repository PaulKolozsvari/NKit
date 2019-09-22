namespace NKit.Data.QueryRunners
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public interface ISqlQueryRunnerWindows
    {
        #region Methods

        SqlQueryRunnerOutputWindows ExecuteQuery(SqlQueryRunnerInputWindows input);

        #endregion //Methods
    }
}
