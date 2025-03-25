namespace NKit.Web.MVC.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using NKit.Data.DB.SQLQuery;
    using NKit.Web.MVC.Models;

    #endregion //Using Directives

    public class FilterModelCore<T> : FilterModelStandard<T> where T : class
    {
        #region Constructors

        public FilterModelCore()
        {
            if (DataModel == null)
            {
                DataModel = new List<T>();
            }
            this.PageSize = 10;
        }

        #endregion //Constructors

        #region Properties

        public SortDirectionTypeCore SortDirectionType
        {
            get { return SortDirectionCore.GetSortDirectionType(Sortdir); }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> EndDate { get; set; }

        public bool FilterByDateRange { get; set; }

        #endregion //Properties

        #region Methods

        /// <summary>
        /// Gets an entity based on the row index in the list. 
        /// </summary>
        /// <param name="rowIndex">The row index</param>
        /// <param name="isOneBasedRowIndex">Whether or not this is a 1 based row index i.e. where the first record index is 1 instead of a zero based index where the first row is 0.</param>
        /// <returns></returns>
        public T GetByRowIndex(Nullable<int> rowIndex, bool isOneBasedRowIndex)
        {
            if (!rowIndex.HasValue)
            {
                return null;
            }
            if (isOneBasedRowIndex) //If this is a one based row index, we need to convert it to a zero based index.
            {
                rowIndex--;
            }
            return rowIndex.HasValue && rowIndex.Value < this.DataModel.Count ? this.DataModel[rowIndex.Value] : null;
        }

        #endregion //Methods
    }
}