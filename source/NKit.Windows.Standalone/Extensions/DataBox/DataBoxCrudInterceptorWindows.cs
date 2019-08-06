namespace NKit.Extensions.DataBox
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using NKit.Data.DB.SQLServer;
    using NKit.Extensions.DataBox.Events;
    using NKit.Extensions.DataBox.Events.Crud;
    using NKit.Extensions.DataBox;
    using NKit.Winforms;
    using NKit.Extensions.ExtensionManaged;
    using NKit.Extensions.DataBox.Managers;

    #endregion //Using Directives

    public abstract class DataBoxCrudInterceptorWindows
    {
        #region Constructors

        public DataBoxCrudInterceptorWindows()
        {
            _extensionManagedEntityCache = new ExtensionManagedEntityCacheWindows();
            AddExtensionManagedEntities();
            SubscribeToCrudEvents();
        }

        #endregion //Constructors

        #region Fields

        private ExtensionManagedEntityCacheWindows _extensionManagedEntityCache;

        private SqlDatabaseTableBaseWindows _currentTable;
        private CustomDataGridViewWindows _currentGrid;
        private bool _filtersEnabled;
        private List<string> _hiddenProperties;
        private List<Type> _hiddenTypes;
        private Dictionary<string, Control> _inputControls;
        private bool _inputControlLabelsIncluded;
        private Control _firstInputControl;

        protected DataBoxManagerWindows _dataBoxManager;

        #endregion //Fields

        #region Properties

        public ExtensionManagedEntityCacheWindows ExtensionManagedEntities
        {
            get { return _extensionManagedEntityCache; }
        }

        public SqlDatabaseTableBaseWindows CurrentTable
        {
            get { return _currentTable; }
        }

        public CustomDataGridViewWindows CurrentGrid
        {
            get { return _currentGrid; }
        }

        public bool FiltersEnabled
        {
            get { return _filtersEnabled; }
        }

        public List<string> HiddenProperties
        {
            get { return _hiddenProperties; }
        }

        public List<Type> HiddenTypes
        {
            get { return _hiddenTypes; }
        }

        public Dictionary<string, Control> InputControls
        {
            get { return _inputControls; }
        }

        public bool InputControlLabelsIncluded
        {
            get { return _inputControlLabelsIncluded; }
        }

        public Control FirstInputControl
        {
            get { return _firstInputControl; }
        }

        public DataBoxManagerWindows DataBoxManager
        {
            get { return _dataBoxManager; }
        }

        #endregion //Properties

        #region Events

        public event OnBeforeRefreshFromServerWindows OnBeforeRefreshFromServer;
        public event OnAfterRefreshFromServerWindows OnAfterRefreshFromServer;

        public event OnBeforeGridRefreshWindows OnBeforeGridRefresh;
        public event OnAfterGridRefreshWindows OnAfterGridRefresh;
        
        public event OnBeforeAddInputControlsWindows OnBeforeAddInputControls;
        public event OnAfterAddInputControlsWindows OnAfterAddInputControls;

        public event OnBeforeCrudOperationWindows OnBeforeUpdate;
        public event OnAfterCrudOperationWindows OnAfterUpdate;

        public event OnBeforeCrudOperationWindows OnBeforeCancelUpdate;
        public event OnAfterCrudOperationWindows OnAfterCancelUpdate;

        public event OnBeforeCrudOperationWindows OnBeforePrepareForUpdate;
        public event OnAfterCrudOperationWindows OnAfterPrepapreForUpdate;

        public event OnBeforeCrudOperationWindows OnBeforeAdd;
        public event OnAfterCrudOperationWindows OnAfterAdd;

        public event OnBeforeCrudOperationWindows OnBeforeDelete;
        public event OnAfterCrudOperationWindows OnAfterDelete;

        public event OnBeforeSaveWindows OnBeforeSave;
        public event OnAfterSaveWindows OnAfterSave;

        #endregion //Events

        #region Methods

        public void SetDataBoxManager(DataBoxManagerWindows dataBoxManager)
        {
            _dataBoxManager = dataBoxManager;
        }

        #region Event Firing Methods

        public void PerformOnBeforeRefreshFromServer(BeforeRefreshFromServerArgsWindows e)
        {
            if (OnBeforeRefreshFromServer != null)
            {
                OnBeforeRefreshFromServer(this, e);
            }
        }

        public void PerformOnAfterRefreshFromServer(AfterRefreshFromServerArgsWindows e)
        {
            if (OnAfterRefreshFromServer != null)
            {
                OnAfterRefreshFromServer(this, e);
            }
        }

        public void PerformOnBeforeGridRefresh(BeforeGridRefreshArgsWindows e)
        {
            if (OnBeforeGridRefresh != null)
            {
                _currentGrid = e.CurrentGrid;
                _filtersEnabled = e.FiltersEnabled;
                OnBeforeGridRefresh(this, e);
            }
        }

        public void PerformOnAfterGridRefresh(AfterGridRefreshArgsWindows e)
        {
            if (OnAfterGridRefresh != null)
            {
                _currentGrid = e.CurrentGrid;
                _filtersEnabled = e.FiltersEnabled;
                _hiddenProperties = e.HiddenProperties;
                _hiddenTypes = e.HiddenTypes;
                OnAfterGridRefresh(this, e);
            }
        }

        public void PerformOnBeforeAddInputControls(BeforeAddInputControlsArgsWindows e)
        {
            if (OnBeforeAddInputControls != null)
            {
                _inputControls = e.InputControls;
                _inputControlLabelsIncluded = e.InputControlLabelsIncluded;
                OnBeforeAddInputControls(this, e);
            }
        }

        public void PerformOnAfterAddInputControls(AfterAddInputControlsArgsWindows e)
        {
            if (OnAfterAddInputControls != null)
            {
                _inputControls = e.InputControls;
                _inputControlLabelsIncluded = e.InputControlLabelsIncluded;
                _firstInputControl = e.FirstInputControl;
                OnAfterAddInputControls(this, e);
            }
        }

        public void PerformOnBeforeUpdate(BeforeCrudOperationArgsWindows e)
        {
            if (OnBeforeUpdate != null)
            {
                OnBeforeUpdate(this, e);
            }
        }

        public void PerformOnAfterUpdate(AfterCrudOperationArgsWindows e)
        {
            if (OnAfterUpdate != null)
            {
                OnAfterUpdate(this, e);
            }
        }

        public void PerformOnBeforeCancelUpdate(BeforeCrudOperationArgsWindows e)
        {
            if (OnBeforeCancelUpdate != null)
            {
                OnBeforeCancelUpdate(this, e);
            }
        }

        public void PerformOnAfterCancelUpdate(AfterCrudOperationArgsWindows e)
        {
            if (OnAfterCancelUpdate != null)
            {
                OnAfterCancelUpdate(this, e);
            }
        }

        public void PerformOnBeforePrepareForUpdate(BeforeCrudOperationArgsWindows e)
        {
            if (OnBeforePrepareForUpdate != null)
            {
                OnBeforePrepareForUpdate(this, e);
            }
        }

        public void PerformOnAfterPrepareForUpdate(AfterCrudOperationArgsWindows e)
        {
            if (OnAfterPrepapreForUpdate != null)
            {
                OnAfterPrepapreForUpdate(this, e);
            }
        }

        public void PerformOnBeforeAdd(BeforeCrudOperationArgsWindows e)
        {
            if (OnBeforeAdd != null)
            {
                OnBeforeAdd(this, e);
            }
        }

        public void PerformOnAfterAdd(AfterCrudOperationArgsWindows e)
        {
            if (OnAfterAdd != null)
            {
                OnAfterAdd(this, e);
            }
        }

        public void PerformOnBeforeDelete(BeforeCrudOperationArgsWindows e)
        {
            if (OnBeforeDelete != null)
            {
                OnBeforeDelete(this, e);
            }
        }

        public void PerformOnAfterDelete(AfterCrudOperationArgsWindows e)
        {
            if (OnAfterDelete != null)
            {
                OnAfterDelete(this, e);
            }
        }

        public void PerformOnBeforeSave(BeforeSaveArgsWindows e)
        {
            if (OnBeforeSave != null)
            {
                OnBeforeSave(this, e);
            }
        }

        public void PerformOnAfterSave(AfterSaveArgsWindows e)
        {
            if (OnAfterSave != null)
            {
                OnAfterSave(this, e);
            }
        }

        #endregion //Event Firing Methods

        #region Abstract Methods

        public abstract void AddExtensionManagedEntities();

        public abstract void SubscribeToCrudEvents();

        #endregion //Abstract Methods

        #region Helper Methods

        public virtual bool IsEntityToBeManaged(object entity)
        {
            bool result = entity != null && _extensionManagedEntityCache.Exists(entity.GetType().FullName);
            return result;
        }

        public virtual bool IsEntityOfType<E>(object entity)
        {
            bool result = entity.GetType().FullName == typeof(E).FullName;
            return result;
        }

        #endregion //Helper Methods

        #endregion //Methods
    }
}