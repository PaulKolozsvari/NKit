namespace NKit.Utilities
{
    #region region name

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //region name

    public static class StringExtensions
    {
        #region Methods

        /// <summary>
        /// Parses the string to a specified enum.
        /// </summary>
        public static T ParseToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        /// <summary>
        /// Parses the string to a specified enum.
        /// </summary>
        public static bool TryParseToEnum<TEnum>(this string enumString, out TEnum result) where TEnum : struct
        {
            return Enum.TryParse<TEnum>(enumString, out result);
        }

        #endregion //Methods
    }
}
