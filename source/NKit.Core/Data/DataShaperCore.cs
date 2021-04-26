namespace NKit.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;

    #endregion //Using Directives

    public class DataShaperCore : DataShaper
    {
        #region Methods

        public static bool IsValidPhoneNumber(string phoneNumber, out string formattedPhoneNumber)
        {
            return new PhoneNumberValidator().IsValidPhoneNumber(phoneNumber, out formattedPhoneNumber);
        }

        static Regex ValidEmailRegex = CreateValidEmailRegex();

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
                !ValidEmailRegex.IsMatch(emailAddress) ||
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

        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        /// <summary>
        /// Generates password with special characters.
        /// </summary>
        public static string GeneratePassword(int passwordLength, int numberOfSpecialCharacters)
        {
            if (passwordLength < 1 || passwordLength > 128)
            {
                throw new ArgumentException(nameof(passwordLength));
            }
            if (numberOfSpecialCharacters > passwordLength || numberOfSpecialCharacters < 0)
            {
                throw new ArgumentException(nameof(numberOfSpecialCharacters));
            }
            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[passwordLength];
                rng.GetBytes(byteBuffer);
                var count = 0;
                var characterBuffer = new char[passwordLength];
                for (var iter = 0; iter < passwordLength; iter++)
                {
                    var i = byteBuffer[iter] % 87;
                    if (i < 10)
                    {
                        characterBuffer[iter] = (char)('0' + i);
                    }
                    else if (i < 36)
                    {
                        characterBuffer[iter] = (char)('A' + i - 10);
                    }
                    else if (i < 62)
                    {
                        characterBuffer[iter] = (char)('a' + i - 36);
                    }
                    else
                    {
                        characterBuffer[iter] = Punctuations[i - 62];
                        count++;
                    }
                }
                if (count >= numberOfSpecialCharacters)
                {
                    return new string(characterBuffer);
                }
                int j;
                var rand = new Random();
                for (j = 0; j < numberOfSpecialCharacters - count; j++)
                {
                    int k;
                    do
                    {
                        k = rand.Next(0, passwordLength);
                    }
                    while (!char.IsLetterOrDigit(characterBuffer[k]));
                    characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
                }
                return new string(characterBuffer);
            }
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

        /// <summary>
        /// Gets the alphabetic part of string i.e. all the letters without the numbers.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetAlphaPartOfString(string input)
        {
            return Regex.Match(input, ALPHA_REGEX_PATTERN).Value;
        }

        #endregion //Methods
    }
}
