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

        /// <summary>
        /// Generates password with special characters.
        /// </summary>
        public static string GeneratePassword(int passwordLength, int numberOfSpecialCharacters)
        {
            return System.Web.Security.Membership.GeneratePassword(passwordLength, numberOfSpecialCharacters);
        }

        /// <summary>
        /// Generates password without special characters.
        /// </summary>
        public static string GenerateSimplePassword(int passwordLength, int numberOfExtraSpecialCharacters)
        {
            string result = GeneratePassword(passwordLength, 1);
            result = Regex.Replace(result, @"[^a-zA-Z0-9]", m => "9");
            string specialCharacters = GeneratePassword(numberOfExtraSpecialCharacters, numberOfExtraSpecialCharacters);
            return string.Concat(result, specialCharacters);
        }

        //Generates a password from a Guid removing all the dashes and appends the specified number of special characters.
        public static string GenerateGuidPassword(int numberOfExtraSpecialCharacters)
        {
            string result = Guid.NewGuid().ToString().Replace("-", string.Empty);
            string specialCharacters = GeneratePassword(numberOfExtraSpecialCharacters, numberOfExtraSpecialCharacters);
            return string.Concat(result, specialCharacters);
        }

        #endregion //Methods
    }
}