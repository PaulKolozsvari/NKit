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

    public class ExtensionManagedEntityCacheWindows : EntityCacheGeneric<string, ExtensionManagedEntityWindows>
    {
        #region Methods

        public ExtensionManagedEntityWindows AddExtensionManagedEntity(string entityFullTypeName)
        {
            if (string.IsNullOrEmpty(entityFullTypeName))
            {
                throw new NullReferenceException(string.Format(
                    "{0} of {1} to be added may not be null or empty.",
                    EntityReaderGeneric<ExtensionManagedEntityWindows>.GetPropertyName(p => p.EntityFullTypeName, false),
                    typeof(ExtensionManagedEntityWindows).FullName));
            }
            if (Exists(entityFullTypeName))
            {
                throw new ArgumentException(string.Format(
                    "A {0} of {1} has already been added to this {2}.",
                    EntityReaderGeneric<ExtensionManagedEntityWindows>.GetPropertyName(p => p.EntityFullTypeName, false),
                    entityFullTypeName,
                    this.GetType().FullName));
            }
            ExtensionManagedEntityWindows result = new ExtensionManagedEntityWindows(entityFullTypeName);
            base.Add(entityFullTypeName, result);
            return result;
        }

        #endregion //Methods
    }
}
