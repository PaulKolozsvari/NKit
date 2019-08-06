namespace NKit.Extensions.ExtensionManaged
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;

    #endregion //Using Directives

    public class ExtensionManagedEntityPropertyWindows
    {
        #region Constructors

        public ExtensionManagedEntityPropertyWindows(
            string parentEntityFullTypeName,
            string propertyName)
        {
            _propertyName = propertyName;
        }

        #endregion //Constructors

        #region Fields

        protected string _propertyName;

        #endregion //Fields

        #region Properties

        public string PropertyName
        {
            get { return _propertyName; }
        }

        #endregion //Properties
    }
}