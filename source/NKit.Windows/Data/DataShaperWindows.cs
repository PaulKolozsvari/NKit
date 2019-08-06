namespace NKit.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    #endregion //Using Directives

    public class DataShaperWindows : DataShaper
    {
        #region Methods

        public static bool IsValidPhoneNumber(string phoneNumber, out string formattedPhoneNumber)
        {
            return new PhoneNumberValidator().IsValidPhoneNumber(phoneNumber, out formattedPhoneNumber);
        }

        public static bool IsValidEmail(string emailAddress)
        {
            return (new EmailAddressAttribute().IsValid(emailAddress));
        }

        public static string GeneratePassword(int passwordLength, int numberOfNonAlphanumericCharacters)
        {
            return System.Web.Security.Membership.GeneratePassword(passwordLength, numberOfNonAlphanumericCharacters);
        }

        #endregion //Methods
    }
}