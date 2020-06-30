namespace NKit.Data.DB.SQLQuery
{
    /// <summary>
    /// Contains and manages sort directions used for running queries against a database.
    /// </summary>
    public class SortDirectionWindows
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

        #endregion //Constants

        #region Methods

        /// <summary>
        /// Gets the SortDirectionType from a string representation of the sort directtion.
        /// </summary>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public static SortDirectionTypeWindows GetSortDirectionType(string sortDirection)
        {
            sortDirection = sortDirection != null ? sortDirection.ToUpper() : string.Empty;
            switch (sortDirection)
            {
                case ASCENDING:
                    return SortDirectionTypeWindows.Ascending;
                case DESCENDING:
                    return SortDirectionTypeWindows.Descending;
                default:
                    return SortDirectionTypeWindows.Ascending;
            }
        }

        /// <summary>
        /// Gets the string representation of th sort direction from the SortDirection type.
        /// </summary>
        /// <param name="sortDirectionType"></param>
        /// <returns></returns>
        public static string GetSortDirection(SortDirectionTypeWindows sortDirectionType)
        {
            switch (sortDirectionType)
            {
                case SortDirectionTypeWindows.Ascending:
                    return ASCENDING;
                case SortDirectionTypeWindows.Descending:
                    return DESCENDING;
                default:
                    return ASCENDING;
            }
        }

        #endregion //Methods
    }
}
