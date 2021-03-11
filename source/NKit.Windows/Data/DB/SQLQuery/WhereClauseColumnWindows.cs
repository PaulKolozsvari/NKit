namespace NKit.Data.DB.SQLQuery
{
    #region Using Directives

    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class WhereClauseColumnWindows
    {
        #region Constructors

        public WhereClauseColumnWindows()
        {
        }

        public WhereClauseColumn(string customClause, WhereClauseLogicalOperator logicalOperatorAgainstNextColumn)
        {
            _customClause = customClause;
            _isCustomClause = !string.IsNullOrEmpty(_customClause);
            LogicalOperatorAgainstNextColumn = logicalOperatorAgainstNextColumn;
        }

        public WhereClauseColumnWindows(
            string columnName,
            WhereClauseComparisonOperatorWindows comparisonOperator,
            object columnValue,
            bool useParameter,
            bool wrapValueWithQuotes)
        {
            _columnName = columnName;
            _comparisonOperator = comparisonOperator;
            _columnValue = columnValue;
            _useParameter = useParameter;
            _wrapValueWithQuotes = wrapValueWithQuotes;
        }

        public WhereClauseColumnWindows(
            string columnName,
            WhereClauseComparisonOperatorWindows comparisonOperator,
            object columnValue,
            bool useParameter,
            bool wrapValueWithQuotes,
            WhereClauseLogicalOperatorWindows logicalOperatorAgainstNextColumn)
        {
            _columnName = columnName;
            _comparisonOperator = comparisonOperator;
            _columnValue = columnValue;
            _useParameter = useParameter;
            _wrapValueWithQuotes = wrapValueWithQuotes;
            LogicalOperatorAgainstNextColumn = logicalOperatorAgainstNextColumn;
        }

        #endregion //Constructors

        #region Fields

        protected string _customClause;
        protected bool _isCustomClause;
        protected string _columnName;
        protected WhereClauseComparisonOperatorWindows _comparisonOperator;
        protected object _columnValue;
        protected bool _useParameter;
        protected bool _wrapValueWithQuotes;
        protected WhereClauseLogicalOperatorWindows _logicalOperatorAgainstNextColumn;

        #endregion //Fields

        #region Properties

        public string CustomClause
        {
            get { return _customClause; }
        }

        public bool IsCustomClause
        {
            get { return _isCustomClause; }
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public WhereClauseComparisonOperatorWindows ComparisonOperator
        {
            get { return _comparisonOperator; }
            set { _comparisonOperator = value; }
        }

        public object ColumnValue
        {
            get { return _columnValue; }
            set { _columnValue = value; }
        }

        public bool UseParameter
        {
            get { return _useParameter; }
            set { _useParameter = value; }
        }

        public bool WrapValueWithQuotes
        {
            get { return _wrapValueWithQuotes; }
            set { _wrapValueWithQuotes = value; }
        }

        public WhereClauseLogicalOperatorWindows LogicalOperatorAgainstNextColumn
        {
            get { return _logicalOperatorAgainstNextColumn; }
            set { _logicalOperatorAgainstNextColumn = value; }
        }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_customClause))
            {
                return _customClause;
            }
            StringBuilder result = new StringBuilder();
            result.AppendFormat(
                "{0}_{1}_{2}",
                ColumnName,
                ComparisonOperator.ToString(),
                ColumnValue);
            if (LogicalOperatorAgainstNextColumn != null)
            {
                result.AppendFormat("_{0}", LogicalOperatorAgainstNextColumn.Value);
            }
            return result.ToString();
        }

        #endregion //Methods
    }
}
