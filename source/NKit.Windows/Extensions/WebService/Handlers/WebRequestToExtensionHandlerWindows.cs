namespace NKit.Extensions.WebService.Handlers
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;
    using NKit.Extensions.WebService.Events;

    #endregion //Using Directives

    public abstract class WebRequestToExtensionHandlerWindows
    {
        #region Constructors

        public WebRequestToExtensionHandlerWindows(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NullReferenceException(string.Format(
                    "{0} may not be null or empty when constructing a {1}.",
                    EntityReaderGeneric<WebRequestToExtensionHandlerWindows>.GetPropertyName(p => p.Name, false),
                    typeof(WebRequestToExtensionHandlerWindows).FullName));
            }
            _name = name;
        }

        #endregion //Constructors

        #region Fields

        protected string _name;

        #endregion //Fields

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        #endregion //Properties

        #region Methods

        public abstract WebRequestToExtensionOutputWindows HandleWebRequest(WebRequestToExtensionInputWindows input);

        #endregion //Methods
    }
}