namespace NKit.Extensions.WebService
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Extensions.ExtensionManaged;
    using NKit.Extensions.WebService.Events.Crud;

    #endregion //Using Directives

    public abstract class WebServiceCrudInterceptorWindows
    {
        #region Constructors

        public WebServiceCrudInterceptorWindows()
        {
            _extensionManagedEntityCache = new ExtensionManagedEntityCacheWindows();
            AddExtensionManagedEntities();
            SubscribeToCrudEvents();
        }

        #endregion //Constructors

        #region Fields

        protected ExtensionManagedEntityCacheWindows _extensionManagedEntityCache;

        #endregion //Fields

        #region Properties

        public ExtensionManagedEntityCacheWindows ExtensionManagedEntities
        {
            get { return _extensionManagedEntityCache; }
        }

        #endregion //Properties

        #region Events

        public event OnBeforeWebGetSqlTableWindows OnBeforeWebGetSqlTable;
        public event OnAfterWebGetSqlTableWindows OnAfterWebGetSqlTable;
        public event OnBeforeWebInvokeSqlTableWindows OnBeforeWebInvokeSqlTable;
        public event OnAfterWebInvokeSqlTableWindows OnAfterWebInvokeSqlTable;
        public event OnBeforeWebInvokeSqlWindows OnBeforeWebInvokeSql;
        public event OnAfterWebInvokeSqlWindows OnAfterWebInvokeSql;

        #endregion //Events

        #region Methods

        #region Event Firing Methods

        public void PerformOnBeforeWebGetSqlTable(BeforeWebGetSqlTableArgsWindows e)
        {
            if (OnBeforeWebGetSqlTable != null)
            {
                OnBeforeWebGetSqlTable(this, e);
            }
        }

        public void PerformOnAfterWebGetSqlTable(AfterWebGetSqlTableArgsWindows e)
        {
            if (OnAfterWebGetSqlTable != null)
            {
                OnAfterWebGetSqlTable(this, e);
            }
        }

        public void PerformOnBeforeWebInvokeSqlTable(BeforeWebInvokeSqlTableArgsWindows e)
        {
            if (OnBeforeWebInvokeSqlTable != null)
            {
                OnBeforeWebInvokeSqlTable(this, e);
            }
        }

        public void PerformOnAfterWebInvokeSqlTable(AfterWebInvokeSqlTableArgsWindows e)
        {
            if (OnAfterWebInvokeSqlTable != null)
            {
                OnAfterWebInvokeSqlTable(this, e);
            }
        }

        public void PerformOnBeforeWebInvokeSql(BeforeWebInvokeSqlArgsWindows e)
        {
            if (OnBeforeWebInvokeSql != null)
            {
                OnBeforeWebInvokeSql(this, e);
            }
        }

        public void PerformOnAfterWebInvokeSql(AfterWebInvokeSqlArgsWindows e)
        {
            if (OnAfterWebInvokeSqlTable != null)
            {
                OnAfterWebInvokeSql(this, e);
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

        public virtual bool IsEntityToBeManaged(string entityFullTypeName)
        {
            bool result = _extensionManagedEntityCache.Exists(entityFullTypeName);
            return result;
        }

        public virtual bool IsEntityOfType<E>(object entity)
        {
            bool result = entity.GetType().FullName == typeof(E).FullName;
            return result;
        }

        public virtual bool IsEntityOfType<E>(string entityFullTypeName)
        {
            bool result = entityFullTypeName == typeof(E).FullName;
            return result;
        }

        #endregion //Helper Methods

        #endregion //Methods
    }
}