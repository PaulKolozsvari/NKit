namespace NKit.Utilities.SMS
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class SmsReceivedEventArgsWindows : EventArgs
    {
        #region Constructors

        public SmsReceivedEventArgsWindows(SmsReceivedInfoWindows smsReceivedInfo)
        {
            _smsReceivedInfo = smsReceivedInfo;
        }

        #endregion //Constructors

        #region Fields

        private SmsReceivedInfoWindows _smsReceivedInfo;

        #endregion //Fields

        #region Properties

        public SmsReceivedInfoWindows SmsReceivedInfo
        {
            get { return _smsReceivedInfo; }
        }

        #endregion //Properties
    }
}