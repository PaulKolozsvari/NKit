﻿namespace NKit.Data
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Renci.SshNet;

    #endregion //Using Directives

    public partial class DataShaper
    {
        #region Methods

        public static string ShapeCamelCaseString(string inputString)
        {
            List<int> spaceIndexes = new List<int>();
            for (int i = 0; i < inputString.Length; i++)
            {
                if (char.IsUpper(inputString[i]))
                {
                    spaceIndexes.Add(i);
                }
            }
            for (int i = 1; i < spaceIndexes.Count; i++)
            {
                inputString = inputString.Insert(spaceIndexes[i], " ");
                for (int j = i; j < spaceIndexes.Count; j++)
                {
                    spaceIndexes[j] += 1;
                }
            }
            return inputString;
        }

        public static int GetIndexOfFirstUpperCaseLetter(string inputString)
        {
            for (int i = 0; i < inputString.Length; i++)
            {
                if (char.IsUpper(inputString[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string RestoreStringToCamelCase(string inputString)
        {
            return inputString.Replace(" ", string.Empty);
        }

        public static string MaskPasswordString(string passwordString, char passwordChar)
        {
            string result = string.Empty;
            return result.PadRight(passwordString.Length, passwordChar);
        }

        public static string GetUniqueIdentifier()
        {
            return string.Format("ID{0}", Guid.NewGuid().ToString().Replace("-", string.Empty));
        }

        protected static string DomainMapper(Match match, out bool invalid)
        {
            invalid = false;
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();
            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

        public static bool StringContainsKeywords(string stringToSearchIn, bool caseSensitive, List<string> keywords)
        {
            string originatinValueToSearch = caseSensitive ? stringToSearchIn : stringToSearchIn.ToLower();
            for (int k = 0; k < keywords.Count; k++)
            {
                string originatingValueToSearchFor = caseSensitive ? keywords[k] : keywords[k].ToLower();
                if (!originatinValueToSearch.Contains(originatingValueToSearchFor))
                {
                    break;
                }
                if (k == (keywords.Count - 1)) //This is the last of keywords to search for in string; hence this string contains all the keywords.
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetCurrencyValueString(double currencyValue, string currencySymbol)
        {
            string result = null;
            bool isNegative = currencyValue < 0;
            if(isNegative)
            {
                currencyValue = currencyValue * -1; //Make it a positive
            }
            if (!string.IsNullOrEmpty(currencySymbol))
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                NumberFormatInfo numberFormatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();
                numberFormatInfo.CurrencySymbol = currencySymbol; // Replace with "$" or "£" or whatever you need
                result = currencyValue.ToString("C2", numberFormatInfo);
            }
            else
            {
                result = currencyValue.ToString("C2", CultureInfo.CurrentCulture);
            }
            if (isNegative)
            {
                result = $"-{result}";
            }
            return result.Replace("(", string.Empty).Replace(")", string.Empty);
        }

        public static string GetCurrencyValueString(double currencyValue, bool includeWindowsRegionalCurrencySymbol)
        {
            if (includeWindowsRegionalCurrencySymbol)
            {
                return string.Format("{0:C}", currencyValue);
            }
            else
            {
                return string.Format("{0:N2}", currencyValue);
            }
        }

        /// <summary>
        /// Increments a string by treating it like a number and increments it as per UTF-16 table (default .NET encoding) e.g. ABC becomes ABD.
        /// Default .NET encoding for a char is UTF-16 (2 byte/16 bit)
        /// FYI: http://csharpindepth.com/Articles/General/Unicode.aspx
        /// </summary>
        /// <param name="input">The input string to be incremented.</param>
        /// <returns>The incremented result</returns>
        //public static string IncrementUnicodeString(string input)
        //{
        //    string result = string.Empty;
        //    for (int i = (input.Length - 1); i >= 0; i--) //Work backwards from the last character, increment the last character and rolling over the previous if required (last char is at char.MaxValue).
        //    {
        //        char c = input[i];
        //        if (c < char.MaxValue) //This character must be rolled over and prepended to the result. The loop must also stop i.e. no need to go through the rest of the characters.
        //        {
        //            char newChar = Convert.ToChar(c + 1);
        //            result = string.Format("{0}{1}", newChar, result);
        //            break;
        //        }
        //        else //This character must be set to reset (set to char.MinValue) and prepended to the result.
        //        {
        //            result = string.Format("{0}{1}", char.MinValue, result);
        //            if (i == 0) //This is the first character, which was just reset, therefore an extra character needs to be prepended.
        //            {
        //                char nextChar = Convert.ToChar(char.MinValue + 1);
        //                result = string.Format("{0}{1}", nextChar, result);
        //            }
        //        }
        //    }
        //    if (input.Length > result.Length) //Not all the characters in the input string have been replaced, therefore prepend the ones that haven't been replaced to the result.
        //    {
        //        result = string.Format("{0}{1}",
        //            input.Substring(0, (input.Length - result.Length)),
        //            result);
        //    }
        //    return result;
        //}

        /// <summary>
        /// Increments a string by treating it like a number and increments it as per UTF-16 table (default .NET encoding) e.g. ABC becomes ABD.
        /// Default .NET encoding for a char is UTF-16 (2 byte/16 bit)
        /// FYI: http://csharpindepth.com/Articles/General/Unicode.aspx
        /// </summary>
        /// <param name="input">The input string to be incremented.</param>
        /// <param name="minimumChar">The minimum character to be allowed in the range. Used in combination with the maximum char, it allows you to set a unicode character boundary for each character. If set to null it defaults to char.MinValue.</param>
        /// <param name="maximumChar">The maximum character to be allowed in the range. Used in combination with the minimum char, it allows you to set a unicode character boundary for each character. If set to null it defaults to char.MaxValue.</param>
        /// <param name="validCharacters"></param>
        /// <returns></returns>
        public static string IncrementUnicodeString(string input, Nullable<char> minimumChar, Nullable<char> maximumChar, List<char> validCharacters)
        {
            if (!minimumChar.HasValue)
            {
                minimumChar = char.MinValue;
            }
            if (!maximumChar.HasValue)
            {
                maximumChar = char.MaxValue;
            }
            ValidateInputUnicodeStringForIncrement(input, minimumChar, maximumChar, validCharacters);
            string result = string.Empty;
            for (int i = (input.Length - 1); i >= 0; i--) //Work backwards from the last character, increment the last character and rolling over the previous if required (last char is at maximumChar).
            {
                char c = input[i];
                if (c < maximumChar.Value) //This character must be rolled over and prepended to the result. The loop must also stop i.e. no need to go through the rest of the characters.
                {
                    char newChar = Convert.ToChar(c + 1);
                    while (!validCharacters.Contains(newChar) && (newChar < maximumChar)) //Increment character until a valid character is created.
                    {
                        newChar = Convert.ToChar(newChar + 1);
                    }
                    result = newChar > maximumChar ? RollOverString(result, minimumChar.Value, i) : string.Format("{0}{1}", newChar, result);
                    break;
                }
                else //This character must be reset (set to minimumChar) and prepended to the result.
                {
                    result = RollOverString(result, minimumChar.Value, i);
                }
            }
            if (input.Length > result.Length) //Not all the characters in the input string have been replaced, therefore prepend the ones that haven't been replaced to the result.
            {
                result = string.Format("{0}{1}",
                    input.Substring(0, (input.Length - result.Length)),
                    result);
            }
            return result;
        }

        /// <summary>
        /// Validates that an input string for incrementing is valid: that it does not contain invalid characters. 
        /// Also that the minimum char is smaller than the maximum char. Lastly that the minimum and maximum characters are not invalid characters.
        /// </summary>
        /// <param name="input">The input string to validate.</param>
        /// <param name="minimumChar">The minimum character.</param>
        /// <param name="maximumChar">The maximum character.</param>
        /// <param name="validCharacters">The list of valid characters.</param>
        protected static void ValidateInputUnicodeStringForIncrement(string input, Nullable<char> minimumChar, Nullable<char> maximumChar, List<char> validCharacters)
        {
            if (minimumChar > maximumChar)
            {
                throw new ArgumentOutOfRangeException("minimumChar may not be greater than maximumChar when incrementing a string.");
            }
            if (StringContainsInvalidCharacters(input, validCharacters))
            {
                throw new Exception(string.Format("Cannot increment input string '{0}'. It contains invalid characters.", input));
            }
            if (!validCharacters.Contains(minimumChar.Value))
            {
                throw new ArgumentOutOfRangeException("minimumChar does not exist in the validCharacters list.");
            }
            if (!validCharacters.Contains(maximumChar.Value))
            {
                throw new ArgumentOutOfRangeException("maximumChar does not exist in the validCharacters list.");
            }
        }

        /// <summary>
        /// Rolls over a string as part of the string increment algorithm e.g. 1A becomes 20.
        /// </summary>
        /// <param name="input">The input string to roll over.</param>
        /// <param name="minimumChar">The minimum character in the unicode table/range to inckude in the roll over e.g. if input is 1A and and minimum character is 0, then the result will be 20.</param>
        /// <param name="currentCharIndex">The current index of the charectsr we're working on for incrementing. This is to check if it is the first character, in which case an extra character needs to be prepended.</param>
        /// <returns></returns>
        protected static string RollOverString(string input, char minimumChar, int currentCharIndex)
        {
            string result = string.Format("{0}{1}", minimumChar, input);
            if (currentCharIndex == 0) //This is the first character, which was just reset, therefore an extra character needs to be prepended.
            {
                char nextChar = Convert.ToChar(minimumChar + 1);
                result = string.Format("{0}{1}", nextChar, result);
            }
            return result;
        }

        /// <summary>
        /// Gets a list of strings from the start string to the end string by incrementing the numeric part of the start string until the end string
        /// e.g. if the startString is B009 and end string is B012, the result list will contain strings B009, B010, B011 and B012. It will not increment
        /// the letters in the string if the numeric part rolls over, but instead it will expand the length of the numeric part e.g. If the startString is
        /// B9 and the endString is B12, the result list will contain the strings B9, B10, B11 and B12.
        /// </summary>
        /// <param name="startString"></param>
        /// <param name="endString"></param>
        /// <returns></returns>
        public static List<string> GetNumericStringRange(string startString, string endString)
        {
            string numericPartStart = GetNumericPartOfString(startString, out int startInt);
            string numericPartEnd = GetNumericPartOfString(endString, out int endInt);
            string alphaPartStart = GetAplhaPartOfString(startString);
            string alphaPartEnd = GetAplhaPartOfString(endString);
            if (!alphaPartStart.Equals(alphaPartEnd))
            {
                throw new Exception("Alpha letters in start string need to match alpha letters in end string when creating numeric string range.");
            }
            if (startInt > endInt)
            {
                throw new Exception("Numeric part of start string needs to be less than numeric part of end string when creating numeric string range.");
            }
            List<string> result = new List<string>() { startString };
            string nextString = startString;
            int nextInt = startInt;
            while (nextInt < endInt) //i.e. while the next string is smaller than the endString, keep incrementing and adding nextString to the result.
            {
                nextString = IncrementNumericPartOfString(nextString, out nextInt);
                result.Add(nextString);
            }
            return result;
        }

        public const string ALPHA_REGEX_PATTERN = @"^[a-zA-Z]+";
        public const string NUMERIC_REGEX_PATTERN = @"\d+";

        /// <summary>
        /// Increments the numeric part of a string e.g. if the input is B009, the incremented output will be B010.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="nextNumber"></param>
        /// <returns></returns>
        public static string IncrementNumericPartOfString(string input, out int nextNumber)
        {
            string alphaPart = GetNonNumericPartOfString(input);
            string numberPart = GetNumericPartOfString(input, out int number, out int startIndex, out int length);
            if (string.IsNullOrEmpty(numberPart))
            {
                throw new ArgumentException($"{nameof(input)} of '{input}' on {nameof(IncrementNumericPartOfString)} does not contain any numeric values.");
            }
            nextNumber = number + 1;
            length = nextNumber / (Math.Pow(10, length)) == 1 ? length + 1 : length;
            string nextNumberString = nextNumber.ToString("D" + length);
            string result = alphaPart.Insert(startIndex, nextNumberString);
            return result;
        }

        /// <summary>
        /// Removes the numeric part of a string i.e. any numeric characters are stripped from the string.
        /// </summary>
        /// <param name="input">The string to be processed.</param>
        /// <returns></returns>
        public static string GetNonNumericPartOfString(string input)
        {
            string numberPart = GetNumericPartOfString(input, out int number, out int startIndex, out int length);
            return !string.IsNullOrEmpty(numberPart) ? input.Replace(numberPart, string.Empty) : input;
        }

        /// <summary>
        /// Gets the numeric (integer) part of a string i.e. all the numbers without the letters.
        /// </summary>
        /// <param name="input">The string to be processed.</param>
        /// <param name="number">The resultant numeric number. If there is no numeric characters in the input string, this value will be -1.</param>
        /// <param name="startIndex">The start index in the input string of the numeric part. If there is no numeric characters in the input string, this value will be -1.</param>
        /// <param name="length">The length of the numeric part in the input string. If there is no numeric characters in the input string, this value will be -1.</param>
        /// <returns></returns>
        public static string GetNumericPartOfString(string input, out int number, out int startIndex, out int length)
        {
            //string pattern = ALPHA_REGEX_PATTERN;
            //string result = Regex.Replace(input, ALPHA_REGEX_PATTERN, "");
            //number = int.Parse(result);
            //return result;
            string pattern = NUMERIC_REGEX_PATTERN;
            Match match = Regex.Match(input, pattern);
            if (!match.Success)
            {
                number = -1;
                startIndex = -1;
                length = -1;
                return string.Empty;
            }
            number = int.Parse(match.Value);
            startIndex = match.Index;
            length = match.Length;
            return match.Value;
        }

        /// <summary>
        /// Gets the alphabetic part of string i.e. all the letters without the numbers.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetAplhaPartOfString(string input)
        {
            return Regex.Match(input, ALPHA_REGEX_PATTERN).Value;
        }

        /// <summary>
        /// Gets the numeric (integer) part of a string i.e. all the numbers without the letters.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetNumericPartOfString(string input, out int number)
        {
            string pattern = ALPHA_REGEX_PATTERN;
            string result = Regex.Replace(input, ALPHA_REGEX_PATTERN, "");
            number = int.Parse(result);
            return result;
        }

        /// <summary>
        /// Compares two strings, by treating them like numbers as per the UTF/ASCII table i.e. ABC is smaller than ABD.
        /// Default .NET encoding for a char is UTF-16 (2 byte/16 bit)
        /// FYI: http://csharpindepth.com/Articles/General/Unicode.aspx
        /// </summary>
        /// <param name="input">The input string in question.</param>
        /// <param name="toCompareAgainst">The string to compare the input string to.</param>
        /// <returns>Result of the comparison.</returns>
        public static bool IsUnicodeStringGreaterThan(string input, string toCompareAgainst)
        {
            if (input == toCompareAgainst)
            {
                return false;
            }
            if (input.Length > toCompareAgainst.Length) //The input string has more characters, thus it is greater i.e. has more weight.
            {
                return true;
            }
            else if (input.Length < toCompareAgainst.Length) //The input string has less characters, thus it is smaller i.e. has less weight.
            {
                return false;
            }
            //We know that they are of the same length i.e. same number or characters.
            bool result = false; //Whether or not the input string is greater.
            for (int i = 0; i < input.Length; i++) //Check if longer string is smaller than the shorter string.
            {
                char a = input[i];
                char b = toCompareAgainst[i];
                if (a == b) //Same characters, so continue with the loop and checking the next character e.g. with 3A vs 3B we check the first character which are equals then we check the next character.
                {
                    continue;
                }
                result = a > b; //longer string is smaller than the shorter string, or otherwise it's bigger. Break out of the loop, because we know which is greater or smaller.
                break;
            }
            return result;
        }

        /// <summary>
        /// Checks whether a string contains characters that are not in the provided list of valid characters.
        /// </summary>
        /// <param name="input">The input string to be checked.</param>
        /// <param name="validCharacters">The list of characters that are valid i.e. any character in the input that is not in this list is invalid. If a null is passed, the result of this method will always be false.</param>
        /// <returns>Returns a bool indicating whether or not the input string contains invalid characters i.e. characters that are not included in the validCharacters list.</returns>
        public static bool StringContainsInvalidCharacters(string input, List<char> validCharacters)
        {
            if (validCharacters == null)
            {
                return false;
            }
            foreach (char c in input.ToCharArray())
            {
                if (!validCharacters.Contains(c))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a list of all the characters from lower case a - z and then 0 - 9.
        /// </summary>
        /// <returns>Returns a list of all the characters from lower case a - z and then 0 - 9.</returns>
        public static List<char> GetValidAlphaNumericRangeCharacters()
        {
            List<char> result = new List<char>()
            {
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'g',
                'g',
                'i',
                'j',
                'k',
                'l',
                'm',
                'n',
                'o',
                'p',
                'q',
                'r',
                's',
                't',
                'v',
                'v',
                'w',
                'x',
                'y',
                'z',
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all the characters from lower case a - z.
        /// </summary>
        /// <returns>Returns a list of all the characters from lower case a - z.</returns>
        public static List<char> GetValidAlphabetRangeCharacters()
        {
            List<char> result = new List<char>()
            {
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'g',
                'g',
                'i',
                'j',
                'k',
                'l',
                'm',
                'n',
                'o',
                'p',
                'q',
                'r',
                's',
                't',
                'v',
                'v',
                'w',
                'x',
                'y',
                'z'
            };
            return result;
        }

        /// <summary>
        /// Gets a list of all characters from 0 - 9.
        /// </summary>
        /// <returns>Returns a list of all characters from 0 - 9.</returns>
        public static List<char> GetValidNumericRangeCharacters()
        {
            List<char> result = new List<char>()
            {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            };
            return result;
        }

        /// <summary>
        /// Returns a range of unicode strings from the start string to the end string 
        /// For example, setting the start string to 1A and the end string to 2F it will return a range of: 1A, 1B, 1C ... 1Z, 20, 21, 22 ... 29, 2A, 2B, 2C ... 2F.
        /// </summary>
        /// <param name="startString">The start of the string range.</param>
        /// <param name="endString">The end of the string range.</param>
        /// <param name="validCharacters">A list of characters that will considered valid. Any characters outside of this list will not be included in the range. If this parameter is set to null, then all characters in the UTF-16 table are valid and included in the result range.</param>
        /// <returns>Returns the range of unicode (UTF-16) strings as a list.</returns>
        public static List<string> GetUnicodeStringRange(
            string startString, 
            string endString, 
            Nullable<char> minimumChar, 
            Nullable<char> maximumChar,
            List<char> validCharacters)
        {
            if (IsUnicodeStringGreaterThan(startString, endString))
            {
                throw new Exception(string.Format("Start string '{0}' may not be greater than End string '{1}' when specifying a string range.",
                    startString,
                    endString));
            }
            if (StringContainsInvalidCharacters(startString, validCharacters)) //Returns false if the validCharacters is null.
            {
                throw new Exception(string.Format("Start string '{0}' contains invalid characters in the specified range.", startString));
            }
            if (StringContainsInvalidCharacters(endString, validCharacters)) //Returns false if the validCharacters is null.
            {
                throw new Exception(string.Format("End string '{0}' contains invalid characters in the specified range.", endString));
            }
            List<string> result = new List<string>() { startString };
            string nextString = startString;
            while (IsUnicodeStringGreaterThan(endString, nextString)) //i.e. while the next string is smaller than the endString, keep incrementing and adding nextString to the result.
            {
                nextString = IncrementUnicodeString(nextString, minimumChar, maximumChar, validCharacters); //Increment the string to the next unicode string.
                result.Add(nextString);
            }
            return result;
        }

        public static string GetDefaultDateString(DateTime date)
        {
            string year = date.Year.ToString();
            string month = date.Month.ToString();
            string day = date.Day.ToString();
            if (month.Length < 2)
            {
                month = string.Format("0{0}", month);
            }
            if (day.Length < 2)
            {
                day = string.Format("0{0}", day);
            }
            string result = string.Format("{0}-{1}-{2}", year, month, day);
            return result;
        }

        /// <summary>
        /// Checks each character of a string to determine whether all characters are digits.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsStringOnlyDigits(string input)
        {
            foreach (char c in input.ToCharArray())
            {
                if (!Char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the the given searchFilter text is null and sets it to an empty string, otherwise converts it to lowercase.
        /// </summary>
        public static string GetSearchFilterLowered(string searchFilter)
        {
            return searchFilter == null ? string.Empty : searchFilter.ToLower();
        }

        /// <summary>
        /// Checks if the given guid is set has the value of an empty Guid and if so, returns null, otherwise returns the passed in Guid unchanged.
        /// </summary>
        public static Nullable<Guid> ConvertEmptyGuidToNull(Nullable<Guid> guid)
        {
            if (guid.HasValue && guid == Guid.Empty)
            {
                guid = null;
            }
            return guid;
        }

        /// <summary>
        /// Checks if the given guid is set has the value of an empty Guid and if so, returns null, otherwise returns the passed in Guid unchanged.
        /// </summary>
        public static string ConvertEmptyGuidToNull(string guidString)
        {
            if (!string.IsNullOrEmpty(guidString) && guidString == Guid.Empty.ToString())
            {
                guidString = null;
            }
            return guidString;
        }

        public static string ReplaceNewlines(string text, string replaceWith)
        {
            return text.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
        }

        /// <summary>
        /// Returns the Major, Minor, Build and Revision of an app version string.
        /// 
        /// The version of an assembly is Major.Minor. From the aforementioned link, Microsoft says, "Subsequent versions of an assembly that differ only by build or revision numbers are considered to be Hotfix updates of the prior version."
        /// The Build represents a recompilation of the same source.
        /// The Revision represents a code change, but one that is fully interchangable with other revisions of the same[Major.Minor] version.
        /// But neither takes precedence over the other.
        /// + Major
        /// |
        ///     +-+ Minor
        ///     |
        ///         +-+ Build
        ///         |
        ///         +-+ Revision
        /// </summary>
        public static void GetAppVersionNumbersFromString(
            string versionString,
            out int major,
            out int minor,
            out int revision,
            out int build)
        {
            major = 0;
            minor = 0;
            revision = 0;
            build = 0;

            string[] versionParameters = versionString.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (versionParameters.Length > 0 && !int.TryParse(versionParameters[0], out major))
            {
                throw new ArgumentException($"Could not convert {versionParameters[0]} to an integer to extract the Major version out of the version string {versionString}.");
            }
            if (versionParameters.Length > 1 && !int.TryParse(versionParameters[1], out minor))
            {
                throw new ArgumentException($"Could not convert {versionParameters[1]} to an integer to extract the Minor version out of the version string {versionString}.");
            }
            if (versionParameters.Length > 2 && !int.TryParse(versionParameters[2], out revision))
            {
                throw new ArgumentException($"Could not convert {versionParameters[2]} to an integer to extract the Revision version out of the version string {versionString}.");
            }
            if (versionParameters.Length > 3 && !int.TryParse(versionParameters[3], out build))
            {
                throw new ArgumentException($"Could not convert {versionParameters[3]} to an integer to extract the Build version out of the version string {versionString}.");
            }
        }

        /// <summary>
        /// Converts the version strings to integers and compares whether versionString integer values are greater than the versionStringToCompareTo integer values i.e.
        /// major, minor, build and revision of the versionString need to be greater than those of versionStringToCompare to in order for the result to be true.
        /// </summary>
        public static bool IsAppVersionGreaterOrEqualTo(string versionString, string versionStringToCompareTo)
        {
            GetAppVersionNumbersFromString(versionString, out int major, out int minor, out int build, out int revision);
            GetAppVersionNumbersFromString(versionStringToCompareTo, out int majorOther, out int minorOther, out int buildOther, out int revisionOther);
            if ((major >= majorOther) && 
                (minor >= minorOther) &&
                (revision >= revisionOther) &&
                (build >= buildOther))
            {
                return true;
            }
            return false;
        }

        public const string HTTPS_PREFIX = "https://";
        public const string HTTP_PREFIX = "http://";

        public static bool UrlStartsWithHttpPrefix(string url, out string prefix)
        {
            string formattedUrl = url.ToLower().Trim();
            if (formattedUrl.StartsWith(HTTPS_PREFIX))
            {
                prefix = HTTPS_PREFIX;
                return true;
            }
            else if (formattedUrl.Contains(HTTP_PREFIX))
            {
                prefix = HTTP_PREFIX;
                return true;
            }
            prefix = null;
            return false;
        }

        public static string RemoveHttpPrefixFromUrl(string url)
        {
            if (UrlStartsWithHttpPrefix(url, out string prefix))
            {
                url = url.ToLower().Trim().Replace(prefix, string.Empty);
            }
            return url;
        }

        public static string AppendHttpPrefixToUrl(string url)
        {
            return url.Contains(HTTP_PREFIX) || url.Contains(HTTPS_PREFIX) ? url : $"{HTTP_PREFIX}{url}";
        }

        public static string AppendHttpsPrefixToUrl(string url)
        {
            return url.Contains(HTTP_PREFIX) || url.Contains(HTTPS_PREFIX) ? url : $"{HTTPS_PREFIX}{url}";
        }

        public static string GetClickableHtmlUrl(string url, string displayText)
        {
            url = AppendHttpPrefixToUrl(url);
            displayText = displayText ?? url;
            return $"<a href={url} target=_blank>{displayText}</a>";
        }

        #endregion //Methods
    }
}