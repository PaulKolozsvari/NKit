namespace NKit.Extensions
{
    
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using NKit.Settings.Default;

    #endregion //Using Directives

    public static class WebHostBuilderExtensions
    {
        #region Methods

        /// <summary>
        /// Configures the IWebHostBuilder to host the URL of the HosUrl setting in the NKitWebApiControllerSettings section of the appsettings.json file.
        /// </summary>
        /// <param name="webHostBuilder"></param>
        public static IWebHostBuilder UseNKitHostUrl(this IWebHostBuilder webHostBuilder)
        {
            NKitWebApiControllerSettings webApiControllerSettings = NKitWebApiControllerSettings.GetSettings();
            webHostBuilder.UseUrls(webApiControllerSettings.HostUrl);
            return webHostBuilder;
        }

        #endregion //Methods
    }
}
