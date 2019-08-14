namespace NKit.Utilities.Singletons
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Utilities.Logging;
    using NKit.Utilities.SettingsFile.Default;
    using NKit.Web.Service.Inspector;
    using NKit.Web.Service.WcfRest;
    using System.Data.Linq;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Channels;
    using NKit.Web.Service.ContentMapping;

    #endregion //Using Directives

    public class WcfRestWebServiceAppSingletonWindows<E> : DatabaseAppSingletonWindows<E> where E : class
    {
        #region Methods

        protected void InitializeAllDefaultSettings<R, I, D>(
            bool startRestWebService,
            WcfRestWebServiceAppSettingsWindows settings,
            WebHttpSecurityMode webHttpSecurityMode,
            HttpClientCredentialType httpClientCredentialType,
            UserNamePasswordValidator userNamePasswordValidator,
            Type userLinqToSqlType,
            Type serverActionLinqToSqlType,
            Type serverErrorLinqToSqlType,
            bool logSettings,
            out string restWebServiceUrl,
            out string restWebServiceStartedLogMessage)
            where R : WcfRestServiceWindows
            where I : IWcfRestServiceWindows
            where D : DataContext
        {
            base.InitializeAllDefaultSettings(
                settings, 
                logSettings);
            base.InitializeDatabaseSettings<D>(
                settings, 
                userLinqToSqlType, 
                serverActionLinqToSqlType, 
                serverErrorLinqToSqlType);
            InitializeRestWebServiceServiceHost<R, I>(
                startRestWebService, 
                settings, 
                webHttpSecurityMode, 
                httpClientCredentialType, 
                userNamePasswordValidator, 
                out restWebServiceUrl, 
                out restWebServiceStartedLogMessage);
        }

        protected CustomBinding GetBinding(
            WcfRestWebServiceAppSettingsWindows settings,
            WebHttpSecurityMode webHttpSecurityMode,
            HttpClientCredentialType httpClientCredentialType,
            out WebHttpBinding webHttpBinding)
        {
           webHttpBinding = new WebHttpBinding()
            {
                MaxBufferPoolSize = settings.RestServiceMaxBufferPoolSize,
                MaxBufferSize = Convert.ToInt32(settings.RestServiceMaxBufferSize),
                MaxReceivedMessageSize = settings.RestServiceMaxReceivedMessageSize
            };
            if (settings.RestServiceUseAuthentication)
            {
                webHttpBinding.Security.Mode = webHttpSecurityMode;
                webHttpBinding.Security.Transport.ClientCredentialType = httpClientCredentialType;
            }
            CustomBinding result = new CustomBinding(webHttpBinding);
            WebMessageEncodingBindingElement webMEBE = result.Elements.Find<WebMessageEncodingBindingElement>();
            webMEBE.ContentTypeMapper = new WcfRawContentTypeMapperWindows();
            return result;
        }

        protected virtual void InitializeRestWebServiceServiceHost<R, I>(
            bool startRestWebService,
            WcfRestWebServiceAppSettingsWindows settings,
            WebHttpSecurityMode webHttpSecurityMode,
            HttpClientCredentialType httpClientCredentialType,
            UserNamePasswordValidator userNamePasswordValidator,
            out string restWebServiceUrl,
            out string restWebServiceStartedLogMessage) 
            where R : WcfRestServiceWindows  
            where I : IWcfRestServiceWindows
        {
            GOCWindows.Instance.JsonSerializer.IncludeOrmTypeNamesInJsonResponse = settings.RestServiceIncludeOrmTypeNamesInJsonResponse;
            GOCWindows.Instance.SetEncoding(settings.RestServiceTextResponseEncoding);

            CustomBinding customBinding = GetBinding(settings, webHttpSecurityMode, httpClientCredentialType, out WebHttpBinding webHttpBinding);
            ServiceHost serviceHost = new ServiceHost(typeof(R));
            restWebServiceUrl = string.Format("http://127.0.0.1:{0}/{1}", settings.RestServicePortNumber, settings.RestServiceHostAddressSuffix);

            SetServiceHostDebugBehavior(serviceHost, settings);
            SetServiceHostThrottlingBehavior(serviceHost, settings);
            ConfigurationFileHelperWindows.SetApplicationServiceModelPerformanceCounters(settings.RestServicePerformanceCounterScope);

            ServiceEndpoint httpEndpoint = serviceHost.AddServiceEndpoint(typeof(I), customBinding, restWebServiceUrl);
            httpEndpoint.Behaviors.Add(new WebHttpBehavior());
            httpEndpoint.EndpointBehaviors.Add(new WcfServiceMessageInspectorBehaviorWindows(settings.RestServiceTraceHttpMessages, settings.RestServiceTraceHttpMessageHeaders));

            if (settings.RestServiceUseAuthentication && (userNamePasswordValidator != null))
            {
                serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.Custom;
                serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = userNamePasswordValidator;
            }
            if (startRestWebService)
            {
                if (GOCWindows.Instance.GetByTypeName<ServiceHost>() != null)
                {
                    GOCWindows.Instance.DeleteByTypeName<ServiceHost>();
                }
                GOCWindows.Instance.AddByTypeName(serviceHost); //The service's stop method will access it from the GOC to close the service host.
                serviceHost.Open();

                restWebServiceStartedLogMessage = string.Format($"Started - {settings.ApplicationName}: {restWebServiceUrl}");
                GOCWindows.Instance.Logger.LogMessage(new LogMessage(restWebServiceStartedLogMessage, LogMessageType.SuccessAudit, LoggingLevel.Minimum));
            }
            else
            {
                restWebServiceStartedLogMessage = string.Format($"Not Started - {settings.ApplicationName}: {restWebServiceUrl}");
            }
        }

        protected virtual void SetServiceHostDebugBehavior(ServiceHost serviceHost, WcfRestWebServiceAppSettingsWindows settings)
        {
            ServiceDebugBehavior debugBehaviour = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehaviour == null) //This should never be, but just in case.
            {
                debugBehaviour = new ServiceDebugBehavior();
                serviceHost.Description.Behaviors.Add(debugBehaviour);
            }
            debugBehaviour.IncludeExceptionDetailInFaults = settings.RestServiceIncludeExceptionDetailInResponse;
        }

        protected virtual void SetServiceHostThrottlingBehavior(ServiceHost serviceHost, WcfRestWebServiceAppSettingsWindows settings)
        {
            ServiceThrottlingBehavior throttlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttlingBehavior == null)
            {
                throttlingBehavior = new ServiceThrottlingBehavior();
                serviceHost.Description.Behaviors.Add(throttlingBehavior);
            }
            //Service Throttling: http://www.dotnetconsult.co.uk/weblog2/PermaLink,guid,1efac821-2b89-47bb-9d27-48f0d6347914.aspx
            //Service Throttling in Detail: https://www.itprotoday.com/microsoft-visual-studio/concurrency-and-throttling-configurations-wcf-services
            throttlingBehavior.MaxConcurrentCalls = settings.RestServiceMaxConcurrentCalls;
            throttlingBehavior.MaxConcurrentSessions = settings.RestServiceMaxConcurrentSessions;
            throttlingBehavior.MaxConcurrentInstances = settings.RestServiceMaxConcurrentInstances;
        }

        #endregion //Methods
    }
}
