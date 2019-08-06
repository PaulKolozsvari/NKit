namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class WcfRestWebServiceAppSettingsWindows : WcfRestWebServiceAppSettings
    {
        #region Constructors

        public WcfRestWebServiceAppSettingsWindows() : base()
        {
        }

        public WcfRestWebServiceAppSettingsWindows(string filePath) : base(filePath)
        {
        }

        public WcfRestWebServiceAppSettingsWindows(string name, string filePath) : base(name, filePath)
        {
        }

        #endregion //Constructors

        #region Properties

        #region Service
        
        /// <summary>
        /// The scope of the performance counters to enable on the service in order to view the counters with Windows perfmon.exe.
        /// </summary>
        [SettingInfo("REST Service", AutoFormatDisplayName = true, Description = "The scope of the performance counters to enable on the service in order to view the counters with Windows perfmon.exe.", CategorySequenceId = 17)]
        public PerformanceCounterScope RestServicePerformanceCounterScope { get; set; }

        #endregion //Service

        #endregion //Properties
    }
}
