namespace NKit.Web.Service
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class FileUploadSessionsWindows
    {
        #region Singleton Setup

        private static FileUploadSessionsWindows _instance;

        public static FileUploadSessionsWindows Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileUploadSessionsWindows();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private FileUploadSessionsWindows()
        {
            _fileStreams = new Dictionary<string, FileStream>();
        }

        #endregion //Constructors

        #region Fields

        private Dictionary<string, FileStream> _fileStreams;

        #endregion //Fields

        #region Properties

        public Dictionary<string, FileStream> FileStreams
        {
            get { return _fileStreams; }
        }

        #endregion //Properties
    }
}