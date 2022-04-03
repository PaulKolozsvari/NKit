namespace NKit.Data.DB.SQLQuery
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class SortDirectionCore
    {
        #region Constants

        /// <summary>
        /// Sort in an ascending order.
        /// </summary>
        public const string ASCENDING = "ASC";
        /// <summary>
        /// Sort in a descending order
        /// </summary>
        public const string DESCENDING = "DESC";

        /// <summary>
        /// Sort in an ascending order.
        /// </summary>
        public const string ASCENDING_LONG = "ascending";
        /// <summary>
        /// Sort in a descending order
        /// </summary>
        public const string DESCENDING_LONG = "descending";

        #endregion //Constants

        #region Methods

        /// <summary>
        /// Gets the SortDirectionType from a string representation of the sort directtion.
        /// </summary>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static SortDirectionTypeCore GetSortDirectionType(string sortDirection)
        {
            sortDirection = sortDirection != null ? sortDirection.ToUpper() : string.Empty;
            switch (sortDirection)
            {
                case ASCENDING:
                    return SortDirectionTypeCore.Ascending;
                case DESCENDING:
                    return SortDirectionTypeCore.Descending;
                default:
                    return SortDirectionTypeCore.Ascending;
            }
        }

        /// <summary>
        /// Gets the string representation of th sort direction from the SortDirection type.
        /// </summary>
        /// <param name="sortDirectionType"></param>
        /// <returns></returns>
        public static string GetSortDirection(SortDirectionTypeCore sortDirectionType)
        {
            switch (sortDirectionType)
            {
                case SortDirectionTypeCore.Ascending:
                    return ASCENDING;
                case SortDirectionTypeCore.Descending:
                    return DESCENDING;
                default:
                    return ASCENDING;
            }
        }

        /// <summary>
        /// Gets the string representation of th sort direction from the SortDirection type.
        /// </summary>
        /// <param name="sortDirectionType"></param>
        /// <returns></returns>
        public static string GetSortDirectionLong(SortDirectionTypeCore sortDirectionType)
        {
            switch (sortDirectionType)
            {
                case SortDirectionTypeCore.Ascending:
                    return ASCENDING_LONG;
                case SortDirectionTypeCore.Descending:
                    return DESCENDING_LONG;
                default:
                    return ASCENDING_LONG;
            }
        }

        #endregion //Methods
    }
}
