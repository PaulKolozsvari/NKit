﻿namespace NKit.Web.MVC.Models
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    #endregion //Using Directives

    public partial class FilterModelStandard<T> : NKitBaseViewModel where T : class
    {
        #region Constructors

        public FilterModelStandard()
        {
            if (DataModel == null)
            {
                DataModel = new List<T>();
            }
        }

        #endregion //Constructors

        #region Fields

        private bool _resetPage;
        private int _page;

        #endregion //Fields

        #region Properties

        public bool IsAdministrator { get; set; }

        public bool IsViewingAllowed { get; set; }

        public bool IsCreateAllowed { get; set; }

        public bool IsDeleteAllowed { get; set; }

        public bool IsEditAllowed { get; set; }

        public bool ResetPage
        {
            get { return _resetPage; }
            set { _resetPage = value; }
        }

        public int Page
        {
            get
            {
                _page = _page <= 0 || _resetPage ? 1 : _page;
                return _page;
            }
            set
            {
                _page = value;
            }
        }

        public int PageSize { get; set; }

        /// <summary>
        /// When using paging this is used to tell the query how many records to skip to query only for the current page.
        /// </summary>
        public int NumberOfRecordsToSkipForCurrentPage
        {
            get
            {
                _page = _page <= 0 ? 1 : _page;
                return (_page - 1) * PageSize;
            }
        }

        /// <summary>
        /// When using paging this is used to tell the query how many records to skip to query only for the current page.
        /// </summary>
        /// <returns></returns>
        public static int GetNumberOfRecordsToSkipForCurrentPage(int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            return (page - 1) * pageSize;
        }

        /// <summary>
        /// The 1 based index of the first record on the page that can be displayed to the user on the view.
        /// </summary>
        public int FirstRecordOnPageDisplayIndex
        {
            get
            {
                int recordsToSkip = NumberOfRecordsToSkipForCurrentPage;
                int result = recordsToSkip;
                if ((recordsToSkip != 0) || (DataModel.Count > 0)) //We are not on the first page or there's at least one record in the DataModel.
                {
                    result++;
                }
                return result;
            }
        }

        /// <summary>
        /// The 1 based index of the last record on the page that can be displayed to the user on the view.
        /// </summary>
        public int LastRecordOnPageDisplayIndex
        {
            get
            {
                int firstRecordIndex = FirstRecordOnPageDisplayIndex;
                int result = (firstRecordIndex + DataModel.Count);
                if (result != 0) //If the last index is not 0, meaning we have some records on the page, then subtract 1 e.g. if the first record on the page is 10 and there are 10 records to be displayed on the page then the last record index should 19 i.e. records 10 to 19 will be displayed which is 10 records in total.
                {
                    result--;
                }
                return result;
            }
        }

        /// <summary>
        /// Total Records returned in this model to the browser i.e. based on current page the user is on, in other words the subset records from the full databaset.
        /// </summary>
        public int TotalCount
        {
            get { return DataModel.Count; }
        }

        /// <summary>
        /// Total Records that would be returned from the query, where TotalCount is the number of records actually returned.
        /// </summary>
        public int TotalFullDatasetRecordCount { get; set; }

        /// <summary>
        /// Total number of records available in the database.
        /// </summary>
        public long TotalTableCount { get; set; }

        public string SearchText { get; set; }

        public string SearchFieldIdentifier { get; set; }

        public string ExtraSearchText1 { get; set; }

        public string ExtraSearchText2 { get; set; }

        public string ExtraSearchText3 { get; set; }

        public string ExtraSearchText4 { get; set; }

        public string ExtraSearchText5 { get; set; }

        public string Sort { get; set; }

        public string Sortdir { get; set; }

        public List<T> DataModel { get; set; }

        public Nullable<Guid> ParentId { get; set; }

        public string ParentCaption { get; set; }

        public Nullable<Guid> SecondParentId { get; set; }

        public string SecondParentCaption { get; set; }

        #endregion //Properties

        #region Methods

        public FilterModelStandard<E> Clone<E>() where E : class
        {
            FilterModelStandard<E> result = new FilterModelStandard<E>();
            result.Page = this.Page;
            result.PageSize = this.PageSize;
            result.SearchText = this.SearchText;
            result.Sort = this.Sort;
            result.Sortdir = this.Sortdir;
            return result;
        }

        #endregion //Methods
    }
}