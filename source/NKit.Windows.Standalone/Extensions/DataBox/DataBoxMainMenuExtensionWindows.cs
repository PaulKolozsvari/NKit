namespace NKit.Extensions.DataBox
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Extensions.DataBox.Events.MainMenu;
    using NKit.Extensions.DataBox;
    using NKit.Extensions.ExtensionManaged;
    using NKit.Extensions.DataBox.Managers;

    #endregion //Using Directives

    public abstract class DataBoxMainMenuExtensionWindows
    {
        #region Constructors

        public DataBoxMainMenuExtensionWindows()
        {
            _extensionManagedMainMenu = new ExtensionManagedMainMenuWindows();
            AddExtensionManagedMenuItems();
            SubscribeToAddedMenuItemsEvents();
        }

        #endregion //Constructors

        #region Fields

        private ExtensionManagedMainMenuWindows _extensionManagedMainMenu;
        private DataBoxManagerWindows _dataBoxManager;

        #endregion //Fields

        #region Properties

        public ExtensionManagedMainMenuWindows ExtensionManagedMainMenu
        {
            get { return _extensionManagedMainMenu; }
        }

        public DataBoxManagerWindows DataBoxManager
        {
            get { return _dataBoxManager; }
        }

        #endregion //Properties

        #region Methods

        public void SetDataBoxManager(DataBoxManagerWindows dataBoxManager)
        {
            _dataBoxManager = dataBoxManager;
        }

        public abstract void AddExtensionManagedMenuItems();

        public abstract void SubscribeToAddedMenuItemsEvents();

        #endregion //Methods
    }
}
