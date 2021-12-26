namespace NKit.Web.MVC.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;
    using NKit.Data.DB.SQLQuery;

    #endregion //Using Directives

    public class FilterModelWindows<T> : FilterModelStandard<T> where T : class
    {
        #region Constructors

        public FilterModelWindows()
        {
            if (DataModel == null)
            {
                DataModel = new List<T>();
            }
            this.PageSize = 10;
        }

        #endregion //Constructors

        #region Properties

        public SortDirectionTypeWindows SortDirectionType
        {
            get { return SortDirectionWindows.GetSortDirectionType(Sortdir); }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> EndDate { get; set; }

        #endregion //Properties
    }
}