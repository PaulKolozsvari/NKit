namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.Drawing;
    using System.Linq;
    using NKit.Data;
    using System.Collections;
    using Microsoft.Win32;
    using System.Threading;

    #endregion //Using Directives

    /// <summary>
    /// A helper class helps in retrieving system information.
    /// </summary>
    public partial class Information
    {
        #region Methods

        /// <summary>
        /// Gets the executing directory of the current application.
        /// </summary>
        public static string GetExecutingDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6);
        }


        //Gets the name of the current executing assembly.
        public static string GetExecutingAssemblyName()
        {
            return Path.GetFileName(Assembly.GetCallingAssembly().GetName().CodeBase).Remove(0, 6);
        }

        /// <summary>
        /// Returns a dictionary of all the system colors with their names as the keys
        /// to the dictionary.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Color> GetSystemColors()
        {
            Type colorType = typeof(Color);
            Dictionary<string, Color> result = new Dictionary<string, Color>();
            foreach (PropertyInfo p in colorType.GetProperties().Where(p => p.PropertyType == colorType))
            {
                Color c = (Color)p.GetValue(null, null);
                result.Add(p.Name, c);
            }
            return result;
        }

        public static string GetDomainAndMachineName()
        {
            return string.Format("{0}.{1}", Environment.UserDomainName, Environment.MachineName);
        }

        #endregion //Methods
    }
}