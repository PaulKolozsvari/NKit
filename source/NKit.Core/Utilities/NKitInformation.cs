namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Hosting;
    using NKit.Utilities;

    #endregion //Using Directives

    public class NKitInformation : Information
    {
        #region Constants

        public const string ASP_NET_CORE_ENVIRONMENT_VARIABLE_NAME = "ASPNETCORE_ENVIRONMENT";

        #endregion //Constants

        #region Methods

        public static string GetAspNetCoreEnvironmentAppSettingsFileName()
        {
            string environment = GetAspNetCoreCurrentEnvironmentName();
            return string.IsNullOrEmpty(environment) ? $"appsettings.json" : $"appsettings.{environment}.json";
        }

        public static string GetAspNetCoreCurrentEnvironmentName()
        {
            return Environment.GetEnvironmentVariable(ASP_NET_CORE_ENVIRONMENT_VARIABLE_NAME);
        }

        public static bool IsApNetCoreDevelopmentEnvironment()
        {
            return GetAspNetCoreCurrentEnvironmentName() == Environments.Development;
        }

        public static bool IsApNetCoreProductionEnvironment()
        {
            return GetAspNetCoreCurrentEnvironmentName() == Environments.Production;
        }

        public static bool IsApNetCoreProductionStaging()
        {
            return GetAspNetCoreCurrentEnvironmentName() == Environments.Staging;
        }

        #endregion //Methods
    }
}
