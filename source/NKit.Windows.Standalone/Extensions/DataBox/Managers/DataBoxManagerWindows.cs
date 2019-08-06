namespace NKit.Extensions.DataBox.Managers
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using NKit.Data;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    public class DataBoxManagerWindows : IDisposable
    {
        #region Constructors

        public DataBoxManagerWindows(IDataBoxWindows dataBox)
        {
            _dataBox = dataBox;
            _dataBox.OnDataBoxPropertiesChanged += _dataBox_OnDataBoxPropertiesChanged;
        }

        #endregion //Constructors

        #region Fields

        private IDataBoxWindows _dataBox;
        private DataBoxPropertiesChangedArgsWindows _dataBoxPropertiesArgs;

        #endregion //Fields

        #region Properties

        public DataBoxPropertiesChangedArgsWindows DataBoxProperties
        {
            get { return _dataBoxPropertiesArgs; }
        }

        public IDataBoxWindows DataBox
        {
            get { return _dataBox; }
        }

        #endregion //Properties

        #region Methods

        public void Dispose()
        {
            _dataBox.OnDataBoxPropertiesChanged -= _dataBox_OnDataBoxPropertiesChanged;
        }

        #endregion //Methods

        #region Event Handlers

        private void _dataBox_OnDataBoxPropertiesChanged(object sender, DataBoxPropertiesChangedArgsWindows e)
        {
            _dataBoxPropertiesArgs = e;
        }

        #endregion //Event Handlers
    }
}