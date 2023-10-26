namespace NKit.Data.DB.SQLServer
{
    #region Using Directives

    using NKit.Data.DB.SQLQuery;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Text;

    #endregion //Using Directives

    public class SqlQueryCore : QueryCore
    {
        #region Constructors

        public SqlQueryCore() : base()
        {
        }

        public SqlQueryCore(SqlQueryKeyword keyword) : base(keyword)
        {
        }

        public SqlQueryCore(string sqlQueryString) : base(sqlQueryString)
        {
        }

        #endregion //Constructors

        #region Methods

        public override void AppendWhereColumns(List<WhereClauseColumnCore> whereClause)
        {
            AppendWhereColumns(whereClause, true);
        }

        public override void AppendWhereColumns(List<WhereClauseColumnCore> whereClause, bool appendWhereStatement)
        {
            if (appendWhereStatement)
            {
                _sqlQueryString.AppendLine("WHERE");
            }
            foreach (WhereClauseColumnCore whereColumn in whereClause)
            {
                string whereColumnName = whereColumn.ColumnName;
                if (whereColumn.UseParameter)
                {
                    string parameterName = string.Format("@{0}", DataShaper.GetUniqueIdentifier());
                    if (SqlParameterExists(_sqlParameters, parameterName))
                    {
                        throw new Exception(string.Format("Parameter with name {0} already added for where column {1}.", whereColumnName));
                    }
                    _sqlParameters.Add(new SqlParameter(parameterName, whereColumn.ColumnValue));
                    _sqlQueryString.Append(string.Format("[{0}] {1} {2}",
                        whereColumnName,
                        whereColumn.ComparisonOperator.ToString(),
                        parameterName));
                }
                else
                {
                    string value = whereColumn.WrapValueWithQuotes ? $"'{whereColumn.ColumnValue}'" : $"{whereColumn.ColumnValue}";
                    _sqlQueryString.Append(string.Format("[{0}] {1} {2}", whereColumnName, whereColumn.ComparisonOperator.ToString(), value));
                }
                if (whereColumn.LogicalOperatorAgainstNextColumn == null)
                {
                    _sqlQueryString.Append("");
                    break;
                }
                _sqlQueryString.AppendLine(string.Format(" {0}", whereColumn.LogicalOperatorAgainstNextColumn.ToString()));
            }
            whereClause.ForEach(w => _whereClause.Add(w));
        }

        public override void AppendWhereClause(WhereClauseCore whereClause)
        {
            _sqlQueryString.AppendLine("WHERE");
            foreach (WhereClauseParanthesisCore p in whereClause)
            {
                _sqlQueryString.Append("(");
                AppendWhereColumns(p.WhereClauseColumns, false);
                _sqlQueryString.Append(")");
                if (p.LogicalOperatorAgainstNextParanthesis == null)
                {
                    _sqlQueryString.Append("");
                    break;
                }
                _sqlQueryString.AppendLine(string.Format(" {0}", p.LogicalOperatorAgainstNextParanthesis.ToString()));
            }
        }

        #endregion //Methods
    }
}
