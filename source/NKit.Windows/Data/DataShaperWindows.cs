﻿namespace NKit.Data
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
        #region Constants

        private static List<Regex> EMAIL_ADDRESS_REGEX_INDICATORS = new List<Regex>()
        {
            new Regex("^\\S+@\\S+\\.\\S+$"), //The basic validation.
            new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$"), //The more complex email regex: This C# regular expression will match 99% of valid email addresses and will not pass validation for email addresses that have, for instance: Dots in the beginning, Multiple dots at the end, But at the same time it will allow part after @ to be IP address.
            new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])"), //RFC 5322 compliant regex: This C# regular expression is compliant to RFC 5322 standard which allows for the most complete validation. Usually, you should not use it because it is an overkill. In most cases apps are not able to handle all emails that this regex allows.

        };

        /// <summary>
        /// https://uibakery.io/regex-library/phone-number-csharp
        /// </summary>
        private static List<Regex> PHONE_NUMBER_REGEX_INDICATORS = new List<Regex>()
        {
            new Regex("^\\+?[1-9][0-9]{7,14}$"), //A simple regex to validate string against a valid international phone number format without delimiters and with an optional plus sign.
            new Regex("^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$"), //This regular expression will match phone numbers entered with delimiters (spaces, dots, brackets, etc.)
        };
        private static Regex VALID_EMAIL_REGEX = CreateValidEmailRegex();
        private static readonly char[] PUNCTUATIONS = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        #endregion //Constants

        #region Methods

        public static bool IsValidPhoneNumber(string phoneNumber, out string formattedPhoneNumber)
        {
            return new PhoneNumberValidator().IsValidPhoneNumber(phoneNumber, out formattedPhoneNumber);
        }

        /// <summary>
        /// Reads an input text and runs regex expressions on it to read phone numbers inside the text.
        /// Returns a list of phone numbers.
        /// https://uibakery.io/regex-library/email-regex-csharp
        /// </summary>
        public static List<string> ParsePhoneNumbersFromText(string inputText)
        {
            List<string> result = new List<string>();
            string[] words = inputText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                string cleanedText = word.Trim().Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);
                cleanedText = Regex.Replace(cleanedText, " {2,}", string.Empty); //Remove all whitespaces
                cleanedText = Regex.Replace(cleanedText, "[^0-9]", string.Empty); //Remove all non numeric characters
                foreach (Regex regex in PHONE_NUMBER_REGEX_INDICATORS)
                {
                    MatchCollection matches = regex.Matches(cleanedText);
                    foreach (Match match in matches)
                    {
                        string value = match.Value;
                        if (!string.IsNullOrEmpty(match.Value) && match.Success && IsValidPhoneNumber(value, out string formattedPhoneNumber))
                        {
                            result.Add(formattedPhoneNumber);
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Taken from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
        /// </summary>
        /// <returns></returns>
        protected static Regex CreateValidEmailRegex()
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }

        public static bool IsValidEmail(string emailAddress)
        {
            if (
                string.IsNullOrEmpty(emailAddress) ||
                !(new EmailAddressAttribute().IsValid(emailAddress)) ||
                !VALID_EMAIL_REGEX.IsMatch(emailAddress) ||
                emailAddress.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailAddress);
                return addr.Address == emailAddress;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads an input text and runs regex expressions on it to read email addresses inside the text.
        /// Returns a list of email addresses.
        /// https://uibakery.io/regex-library/email-regex-csharp
        /// </summary>
        public static List<string> ParseEmailAddressesFromText(string inputText)
        {
            List<string> result = new List<string>();
            string[] words = inputText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                string cleanedText = word.Trim().Replace(" ", string.Empty);
                cleanedText = Regex.Replace(cleanedText, " {2,}", string.Empty); // Remove all whitespaces
                foreach (Regex regex in EMAIL_ADDRESS_REGEX_INDICATORS)
                {
                    MatchCollection matches = regex.Matches(cleanedText);
                    foreach (Match match in matches)
                    {
                        string value = match.Value;
                        if (!string.IsNullOrEmpty(match.Value) && match.Success && IsValidEmail(value) && !result.Contains(value))
                        {
                            result.Add(value);
                        }
                    }
                }
            }
            return result;
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
            string result = GeneratePassword(passwordLength, 0);
            result = Regex.Replace(result, @"[^a-zA-Z0-9]", m => "9");
            if (numberOfExtraSpecialCharacters < 1)
            {
                return result;
            }
            string specialCharacters = GeneratePassword(passwordLength, numberOfExtraSpecialCharacters);
            return string.Concat(result, specialCharacters);
        }

        //Generates a password from a Guid removing all the dashes and appends the specified number of special characters.
        public static string GenerateGuidPassword(int numberOfExtraSpecialCharacters)
        {
            string result = Guid.NewGuid().ToString().Replace("-", string.Empty);
            string specialCharacters = GeneratePassword(numberOfExtraSpecialCharacters, numberOfExtraSpecialCharacters);
            return string.Concat(result, specialCharacters);
        }

        /// <summary>
        /// Gets the alphabetic part of string i.e. all the letters without the numbers.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetAlphaPartOfString(string input)
        {
            return Regex.Match(input, ALPHA_REGEX_PATTERN).Value;
        }

        /// <summary>
        /// Concatenates a list of strings into a single CSV string.
        /// </summary>
        public static string GetCsvOfStringList(List<string> strings)
        {
            StringBuilder result = new StringBuilder();
            int count = strings.Count;
            int lastIndex = count - 1;
            for (int i = 0; i < count; i++)
            {
                result.Append(strings[i]);
                if (i < lastIndex)
                {
                    result.Append(',');
                }
            }
            return result.ToString();
        }

        #endregion //Methods
    }
}