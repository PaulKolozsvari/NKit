namespace NKit.Web.MVC.Models
{
    #region Using Directives

    using System;

    #endregion //Using Directives

    /// <summary>
    /// This base view model can be used to share data between all models on an MVC web application i.e. for example if wanting to create a search box in the _Layout view which will be visible to all pages and partial views when loaded.
    /// For more info see: https://stackoverflow.com/questions/4154407/asp-net-mvc-razor-pass-model-to-layout
    /// </summary>
    public class NKitBaseViewModel
    {
        #region Properties

        /// <summary>
        /// A guid value of the context of the data. This can be used for current parent ID (e.g. SiteId) or something else.
        /// </summary>
        public Nullable<Guid> ContextId { get; set; }

        /// <summary>
        /// A string value for the context of the data. This can be used for an entered search text or something else etc.
        /// </summary>
        public string ContextText { get; set; }

        /// <summary>
        /// An integer value for the context of the data. This can be used for an entered search text or something else etc
        /// </summary>
        public Nullable<int> ContextInteger { get; set; }

        /// <summary>
        /// An boolean value for the context of the data. This can be used for an entered search text or something else etc
        /// </summary>
        public Nullable<bool> ContextBoolean { get; set; }

        #endregion //Properties
    }
}
