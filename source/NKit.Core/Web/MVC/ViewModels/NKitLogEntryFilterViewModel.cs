namespace NKit.Web.MVC.ViewModels
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Web.MVC.Models;

    #endregion //Using Directives

    public class NKitLogEntryFilterViewModel : FilterModelCore<NKitLogEntryViewModel>
    {
        #region Constructors

        public NKitLogEntryFilterViewModel()
        {
            this.FilterByDateRange = true;
        }

        #endregion //Constructors
    }
}
