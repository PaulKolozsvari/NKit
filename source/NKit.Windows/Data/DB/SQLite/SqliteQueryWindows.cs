﻿namespace NKit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.SQLQuery;

    #endregion //Using Directives

    public class SqliteQueryWindows : QueryWindows
    {
        #region Constructors

        public SqliteQueryWindows() : base()
        {
        }

        public SqliteQueryWindows(SqlQueryKeyword keyword) : base(keyword)
        {
        }

        public SqliteQueryWindows(string sqlQueryString) : base(sqlQueryString)
        {
        }

        #endregion //Constructors

        #region Methods

        public override void AppendWhereColumns(List<WhereClauseColumnWindows> whereClause)
        {
            AppendWhereColumns(whereClause, true);
        }

        public override void AppendWhereColumns(List<WhereClauseColumnWindows> whereClause, bool appendWhereStatement)
        {
            if (appendWhereStatement)
            {
                _sqlQueryString.AppendLine("WHERE");
            }
            foreach (WhereClauseColumnWindows whereColumn in whereClause)
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
                    _sqlParameters.Add(new SQLiteParameter(parameterName, whereColumn.ColumnValue));
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

        public override void AppendWhereClause(WhereClause whereClause)
        {
            _sqlQueryString.AppendLine("WHERE");
            foreach (WhereClauseParanthesisWindows p in whereClause)
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
