namespace NKit.Web.MVC.ViewModels
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using NKit.Data;
    using NKit.Data.DB.LINQ.Models;
    using NKit.Web.MVC.Models;

    #endregion //Using Directives

    public class NKitLogEntryViewModel : NKitEntityModel<NKitLogEntryViewModel, NKitLogEntry>
    {
        #region Constructors

        public NKitLogEntryViewModel()
        {
        }

        public NKitLogEntryViewModel(NKitLogEntry e)
        {
            DataValidator.ValidateObjectNotNull(e, nameof(e), nameof(NKitLogEntryViewModel));
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

        public string MessageShortened { get; set; }

        public string MessageMediumLength { get; set; }

        public string StackTraceShortened { get; set; }

        #endregion //Display Properties

        #endregion //Properties

        #region Methods

        public override bool IsValid(out string errorMessage)
        {
            if (!IsGuidFieldNotEmpty(EntityReaderGeneric<NKitLogEntryViewModel>.GetPropertyName(p => p.NKitLogEntryId, true), this.NKitLogEntryId, out errorMessage))
            {
                return false;
            }
            return true;
        }

        public override void CopyPropertiesFrom(NKitLogEntry e)
        {
            base.CopyPropertiesFrom(e);
            this.MessageShortened = !string.IsNullOrEmpty(this.Message) && this.Message.Length > 30 ? this.Message.Substring(0, 30) : this.Message;
            this.MessageMediumLength = !string.IsNullOrEmpty(this.Message) && this.Message.Length > 50 ? this.Message.Substring(0, 50) : this.Message;
            this.StackTraceShortened = !string.IsNullOrEmpty(this.StackTrace) && this.StackTrace.Length > 20 ? this.StackTrace.Substring(0, 20) : this.Message;
        }

        #endregion //Methods
    }
}