namespace NKit.Web.MVC.Extensions
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.AspNetCore.Http;

    #endregion //Using Directives

    public static class ExtensionMethodsCore
    {
        #region Methods

        public static string GetAbsoluteFilePath(string relativePath, HttpRequest request)
        {
            return request.PathBase + relativePath[1..]; //https://stackoverflow.com/questions/50603901/asp-net-core-replacement-for-virtualpathutility
        }

        public static string RelativeFromAbsolutePath(string absolutePath, HttpRequest request)
        {
            string pathBase = request.PathBase;
            string result = absolutePath.Replace(pathBase, string.Empty);
            return result;
        }

        #endregion //Methods
    }
}