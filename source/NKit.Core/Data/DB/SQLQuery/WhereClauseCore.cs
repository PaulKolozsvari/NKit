namespace NKit.Data.DB.SQLQuery
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class WhereClauseCore : IEnumerable<WhereClauseParanthesisCore>
    {
        #region Constructors

        public WhereClauseCore()
        {
            _whereClauseParanthesisList = new List<WhereClauseParanthesisCore>();
        }

        #endregion //Constructors

        #region Fields

        public List<WhereClauseParanthesisCore> _whereClauseParanthesisList;

        #endregion //Fields

        #region Properties

        public List<WhereClauseParanthesisCore> WhereClauseParanthesisList
        {
            get { return _whereClauseParanthesisList; }
            set { _whereClauseParanthesisList = value; }
        }

        public int Count
        {
            get { return _whereClauseParanthesisList.Count; }
        }

        #endregion //Properties

        #region Methods

        public void Add(WhereClauseParanthesisCore whereClauseParanthesis)
        {
            _whereClauseParanthesisList.Add(whereClauseParanthesis);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (WhereClauseParanthesisCore p in _whereClauseParanthesisList)
            {
                result.AppendLine(p.ToString());
            }
            return result.ToString();
        }

        public IEnumerator<WhereClauseParanthesisCore> GetEnumerator()
        {
            return _whereClauseParanthesisList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion //Methods
    }
}
