namespace NKit.Data.DB.MicrosoftSqlite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Data.Sqlite;
    using NKit.Data.DB.SQLQuery;

    #endregion //Using Directives

    public class MicrosoftSqliteQueryCore : QueryCore
    {
        #region Constructors

        public MicrosoftSqliteQueryCore() : base()
        {
        }

        public MicrosoftSqliteQueryCore(SqlQueryKeyword keyword) : base(keyword)
        {
        }

        public MicrosoftSqliteQueryCore(string sqlQueryString) : base(sqlQueryString)
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
                if (whereColumn.IsCustomClause)
                {
                    _sqlQueryString.AppendLine(whereColumn.ToString());
                }
                if (whereColumn.UseParameter)
                {
                    string parameterName = string.Format("@{0}", DataShaper.GetUniqueIdentifier());
                    if (whereColumn.ComparisonOperator.Value == "IN")
                    {
                        parameterName = $"({parameterName})";
                    }
                    if (SqlParameterExists(_sqlParameters, parameterName))
                    {
                        throw new Exception(string.Format("Parameter with name {0} already added for where column {1}.", whereColumnName));
                    }
                    _sqlParameters.Add(new SqliteParameter(parameterName, whereColumn.ColumnValue));
                    _sqlQueryString.Append(string.Format("[{0}] {1} {2}",
                        whereColumnName,
                        whereColumn.ComparisonOperator.ToString(),
                        parameterName));
                }
                else
                {
                    string value = whereColumn.WrapValueWithQuotes ? $"'{whereColumn.ColumnValue}'" : $"{whereColumn.ColumnValue}";
                    if (whereColumn.ComparisonOperator.Value == "IN")
                    {
                        value = $"({value})";
                    }
                    _sqlQueryString.Append(string.Format("[{0}] {1} {2}", whereColumnName, whereColumn.ComparisonOperator.ToString(), value));
                }
                if (whereColumn.LogicalOperatorAgainstNextColumn == null)
                {
                    _sqlQueryString.Append("");
                    break;
                }
                _sqlQueryString.Append(string.Format(" {0} ", whereColumn.LogicalOperatorAgainstNextColumn.ToString()));
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
