namespace NKit.Utilities.Email
{
    using NKit.Data;
    using NKit.Utilities.Logging;
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class EmailListConverterCore
    {
        #region Constants

        public const char SEPARATOR = ';';

        #endregion //Constants

        #region Methods

        public static string GetEmailRecipientListString(List<EmailNotificationRecipient> emailAddressList)
        {
            if (emailAddressList == null || emailAddressList.Count < 1)
            {
                return string.Empty;
            }
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < emailAddressList.Count; i++)
            {
                var e = emailAddressList[i];
                result.Append($"{e.EmailAddress}");
                if (i < (emailAddressList.Count - 1))
                {
                    result.Append(SEPARATOR);
                }
            }
            return result.ToString();
        }

        public static List<EmailNotificationRecipient> GetEmailRecipientList(string emailAddressListString)
        {
            if (string.IsNullOrEmpty(emailAddressListString))
            {
                return new List<EmailNotificationRecipient>();
            }
            emailAddressListString = emailAddressListString.Trim();
            List<EmailNotificationRecipient> result = new List<EmailNotificationRecipient>();
            string[] emailAddresses = emailAddressListString.Split(SEPARATOR);
            for (int i = 0; i < emailAddresses.Length; i++)
            {
                string e = emailAddresses[i].Trim();
                if (string.IsNullOrEmpty(e))
                {
                    continue;
                }
                if (!DataShaperCore.IsValidEmail(e))
                {
                    GOC.Instance.Logger.LogMessage(new LogMessage($"{e} is not a valid Email Address.", LogMessageType.Warning, LoggingLevel.Normal));
                    continue;
                }
                result.Add(new EmailNotificationRecipient() { DisplayName = e, EmailAddress = e });
            }
            return result;
        }

        #endregion //Methods
    }
}
