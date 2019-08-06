namespace NKit.Extensions.ExtensionManaged
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;
    using NKit.Extensions.ExtensionManaged;
    using NKit.Extensions.DataBox.Events.MainMenu;

    #endregion //Using Directives

    public class ExtensionManagedMenuItemWindows : EntityCacheGeneric<string, ExtensionManagedMenuItemWindows>
    {
        #region Constructors

        public ExtensionManagedMenuItemWindows(string name)
            : base(name)
        {
            _extensionManagedEntityCache = new ExtensionManagedEntityCacheWindows();
        }

        #endregion //Constructors

        #region Fields

        protected object _tag;
        protected ExtensionManagedEntityCacheWindows _extensionManagedEntityCache;

        #endregion //Fields

        #region Events

        public event OnMenuClickWindows OnMenuClick;

        #endregion //Events

        #region Properties

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public ExtensionManagedEntityCacheWindows ExtensionManagedEntities
        {
            get { return _extensionManagedEntityCache; }
        }

        #endregion //Properties

        #region Methods

        public void PerformOnClick(MenuClickArgsWindows e)
        {
            if (OnMenuClick != null)
            {
                OnMenuClick(this, e);
            }
        }

        #endregion //Methods
    }
}