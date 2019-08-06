namespace NKit.Extensions.ExtensionManaged
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;
    using NKit.Utilities;

    #endregion //Using Directives

    public class ExtensionManagedEntityWindows : EntityCacheGeneric<string, ExtensionManagedEntityPropertyWindows>
    {
        #region Constructors

        public ExtensionManagedEntityWindows(string entityFullTypeName)
            : base()
        {
            if (string.IsNullOrEmpty(entityFullTypeName))
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be null when constructing an {1}.",
                    EntityReaderGeneric<ExtensionManagedEntityWindows>.GetPropertyName(p => p.EntityFullTypeName, false),
                    typeof(ExtensionManagedEntityWindows).FullName));
            }
            _entityFullTypeName = entityFullTypeName;
        }

        #endregion //Constructors

        #region Fields

        protected string _entityFullTypeName;

        #endregion //Fields

        #region Properties

        public string EntityFullTypeName
        {
            get { return _entityFullTypeName; }
        }

        #endregion //Properties

        #region Methods

        public ExtensionManagedEntityPropertyWindows AddExtensionManagedProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new NullReferenceException(string.Format(
                    "{0} of {1} to be added may not be null or empty.",
                    EntityReaderGeneric<ExtensionManagedEntityPropertyWindows>.GetPropertyName(p => p.PropertyName, false),
                    typeof(ExtensionManagedEntityPropertyWindows).FullName));
            }
            if (Exists(propertyName))
            {
                throw new ArgumentException(string.Format(
                    "A {0} of {1} has already been added to this {2} for {3}.",
                    EntityReaderGeneric<ExtensionManagedEntityPropertyWindows>.GetPropertyName(p => p.PropertyName, false),
                    propertyName,
                    this.GetType().FullName,
                    _entityFullTypeName));
            }
            ExtensionManagedEntityPropertyWindows result = new ExtensionManagedEntityPropertyWindows(_entityFullTypeName, propertyName);
            base.Add(propertyName, result);
            return result;
        }

        #endregion //Methods
    }
}