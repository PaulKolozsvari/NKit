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

        #endregion //Constructors
    }
}