namespace NKit.Data.DB.SQLQuery
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class WhereClauseParanthesisWindows : IEnumerable<WhereClauseColumnWindows>
    {
        #region Constructors

        public WhereClauseParanthesisWindows()
        {
            _whereClauseColumns = new List<WhereClauseColumnWindows>();
        }

        #endregion //Constructors

        #region Fields

        private List<WhereClauseColumnWindows> _whereClauseColumns;
        private WhereClauseLogicalOperatorWindows _whereClauseLogicalOperatorAgainsNextParanthesis;

        #endregion //Fields

        #region Properties

        public List<WhereClauseColumnWindows> WhereClauseColumns
        {
            get { return _whereClauseColumns; }
            set { _whereClauseColumns = value; }
        }

        public WhereClauseLogicalOperatorWindows LogicalOperatorAgainstNextParanthesis
        {
            get { return _whereClauseLogicalOperatorAgainsNextParanthesis; }
            set { _whereClauseLogicalOperatorAgainsNextParanthesis = value; }
        }

        public int Count
        {
            get { return _whereClauseColumns.Count; }
        }

        #endregion //Properties

        #region Methods

        public void Add(WhereClauseColumnWindows whereClauseColumn)
        {
            _whereClauseColumns.Add(whereClauseColumn);
        }

        public override string ToString()
        {
            if (_whereClauseColumns == null || _whereClauseColumns.Count < 1)
            {
                return string.Empty;
            }
            StringBuilder result = new StringBuilder();
            result.Append("(");
            for (int i = 0; i < _whereClauseColumns.Count; ++i)
            {
                WhereClauseColumnWindows c = _whereClauseColumns[i];
                result.Append(c.ToString());
                if (i < (_whereClauseColumns.Count - 1)) //Not the last where clause column in the where clause paranthesis.
                {
                    result.Append("_");
                }
            }
            result.Append(")");
            if (LogicalOperatorAgainstNextParanthesis != null)
            {
                result.AppendFormat("_{0}", LogicalOperatorAgainstNextParanthesis.Value);
            }
            return result.ToString();
        }

        public IEnumerator<WhereClauseColumnWindows> GetEnumerator()
        {
            return _whereClauseColumns.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion //Methods
    }
}