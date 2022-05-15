namespace NKit.Web.MVC.CsvModels
{
    #region Using Directives

    using NKit.Data.DB.LINQ.Models;
    using NKit.Web.MVC.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Data;

    #endregion //Using Directives

    public class NKitLogEntryCsvModel : NKitEntityModel<NKitLogEntryCsvModel, NKitLogEntry>
    {
        #region Constructors

        public NKitLogEntryCsvModel()
        {
        }

        public NKitLogEntryCsvModel(NKitLogEntry e)
        {
            DataValidator.ValidateObjectNotNull(e, nameof(e), nameof(NKitLogEntryCsvModel));
            CopyPropertiesFrom(e);
        }

        #endregion //Constructors

        #region Properties

        public Guid NKitLogEntryId { get; set; }

        public string Message { get; set; }

        public string Source { get; set; }

        public string ClassName { get; set; }

        public string FunctionName { get; set; }

        public string StackTrace { get; set; }

        public int EventId { get; set; }

        public string EventName { get; set; }

        public DateTime DateCreated { get; set; }

        #region Display Properties

        #endregion //Display Properties

        #endregion //Properties

        #region Methods

        public override bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            return true;
        }

        public override void CopyPropertiesFrom(NKitLogEntry e)
        {
            base.CopyPropertiesFrom(e);
        }

        #endregion //Methods
    }
}
