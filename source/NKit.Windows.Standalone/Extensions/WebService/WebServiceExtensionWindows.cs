namespace NKit.Extensions.WebService
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Extensions.WebService.Handlers;

    #endregion //Using Directives

    public abstract class WebServiceExtensionWindows
    {
        #region Constructors

        public WebServiceExtensionWindows()
        {
            _webRequestToExtensionHandlerCache = new WebRequestToExtensionHandlerCacheWindows();
            AddWebRequestToExtensionHandlers();
        }

        #endregion //Constructors

        #region Fields

        protected WebRequestToExtensionHandlerCacheWindows _webRequestToExtensionHandlerCache;

        #endregion //Fields

        #region Methods

        public abstract void AddWebRequestToExtensionHandlers();

        public WebRequestToExtensionHandlerWindows GetHandler(string handler)
        {
            if (!_webRequestToExtensionHandlerCache.Exists(handler))
            {
                throw new NullReferenceException(string.Format(
                    "No {0} named {1} exists in {2}.",
                    typeof(WebRequestToExtensionHandlerWindows).FullName,
                    handler,
                    this.GetType().FullName));
            }
            return _webRequestToExtensionHandlerCache[handler];
        }

        public void AddHandler(WebRequestToExtensionHandlerWindows handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(string.Format(
                    "The {0} to be added to {1} may not null.",
                    typeof(WebRequestToExtensionHandlerWindows).FullName,
                    this.GetType().FullName));
            }
            _webRequestToExtensionHandlerCache.Add(handler.Name, handler);
        }

        #endregion //Methods
    }
}
