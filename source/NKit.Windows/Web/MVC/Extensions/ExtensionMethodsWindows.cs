namespace NKit.Web.MVC.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;

    #endregion //Using Directives

    public static class ExtensionMethodsWindows
    {
        #region Methods

        public static string RelativePath(this HttpServerUtility srv, string path, HttpRequest context)
        {
            return path.Replace(context.ServerVariables["APPL_PHYSICAL_PATH"], "~/").Replace(@"\", "/");
        }
        
        public static string RelativeFromAbsolutePath(string path)
        {
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                var applicationPath = request.PhysicalApplicationPath;
                var virtualDir = request.ApplicationPath;
                virtualDir = virtualDir == "/" ? virtualDir : (virtualDir + "/");
                return path.Replace(applicationPath, virtualDir).Replace(@"\", "/");
            }

            throw new InvalidOperationException("We can only map an absolute back to a relative path if an HttpContext is available.");
        }

        public static HtmlString GetApplicationVersion(this HtmlHelper helper)
        {
            Assembly assembly = GetWebEntryAssembly();
            Version version = assembly.GetName().Version;
            AssemblyProductAttribute product = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).FirstOrDefault() as AssemblyProductAttribute;
            return version != null && product != null ? new HtmlString($"<span>v{version.Major}.{version.Minor}.{version.Build} ({version.Revision})</span>") : new HtmlString("");
        }

        public static string GetApplicationVersionRawString(this HtmlHelper helper)
        {
            Assembly assembly = GetWebEntryAssembly();
            Version version = assembly.GetName().Version;
            AssemblyProductAttribute product = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true).FirstOrDefault() as AssemblyProductAttribute;
            return version != null && product != null ? $"v{version.Major}.{version.Minor}.{version.Build} ({version.Revision})" : string.Empty;
        }

        private static Assembly GetWebEntryAssembly()
        {
            if (System.Web.HttpContext.Current == null ||
                System.Web.HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }
            Type type = System.Web.HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }
            return type == null ? null : type.Assembly;
        }

        #endregion //Methods
    }
}