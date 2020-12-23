namespace NKit.Utilities.SettingsFile.Default
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Utilities.SettingsFile;

    #endregion //Using Directives

    public partial class WebServiceMobileClientAppSettings : WebServiceClientAppSettings
    {
        #region Constructors

        public WebServiceMobileClientAppSettings() : base()
        {
        }

        public WebServiceMobileClientAppSettings(string filePath) : base(filePath)
        {
        }

        public WebServiceMobileClientAppSettings(string name, string filePath) : base(name, filePath)
        {
        }

        #endregion //Constructors

        #region FTP

        /// <summary>
        /// The base URI of the FTP site.
        /// </summary>
        [SettingInfoAttribute("FTP", DisplayName = "FTP Base URI", Description = "The base URI of the FTP site.", CategorySequenceId = 0)]
        public string FtpBaseUri { get; set; }

        [SettingInfoAttribute("FTP", DisplayName = "FTP Username", Description = "The Username to use to connect to the FTP site.", CategorySequenceId = 1)]
        public string FtpUsername { get; set; }

        [SettingInfoAttribute("FTP", DisplayName = "FTP Password", Description = "The Password to use to connect to the FTP site.", CategorySequenceId = 2)]
        public string FtpPassword { get; set; }

        [SettingInfoAttribute("FTP", DisplayName = "FTP File Transfer Chunk Size", Description = "The size of the buffer to use when transferring files to and from the FTP site.", CategorySequenceId = 3)]
        public int FtpFileTransferChunkSize { get; set; }

        #endregion //FTP
    }
}
