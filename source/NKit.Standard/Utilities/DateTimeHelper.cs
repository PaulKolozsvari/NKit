namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion //Using Directives

    public class DateTimeHelper
    {
        #region Fields

        private static List<string> SUPPORTED_DATE_TIME_FORMATS = new List<string>()
        {
            "dd/MM/yyyy HH:mm:ss"
        };

        #endregion //Fields

        #region Methods

        public static DateTime Parse(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            foreach (string format in SUPPORTED_DATE_TIME_FORMATS)
            {
                if (DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out result))
                {
                    return result;
                }
            }
            throw new FormatException($"Could not parse {value} to a valid {nameof(DateTime)}.");
        }

        #endregion //Methods
    }
}