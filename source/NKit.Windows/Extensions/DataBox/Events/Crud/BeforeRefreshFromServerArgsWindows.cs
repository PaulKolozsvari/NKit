namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    public class BeforeRefreshFromServerArgsWindows : BeforeDataBoxArgsWindows
    {
        #region Constructors

        public BeforeRefreshFromServerArgsWindows(bool presentLoadDataBoxForm, SqlDatabaseTableBaseWindows selectedTable)
        {
            _presentLoadDataBoxForm = presentLoadDataBoxForm;
            _selectedTable = selectedTable;
        }

        #endregion //Constructors

        #region Fields

        protected bool _presentLoadDataBoxForm;
        protected SqlDatabaseTableBaseWindows _selectedTable;

        #endregion //Fields

        #region Properties

        public bool PresentLoadDataBoxForm
        {
            get { return _presentLoadDataBoxForm; }
        }

        public SqlDatabaseTableBaseWindows SelectedTable
        {
            get { return _selectedTable; }
        }

        #endregion //Properties
    }
}
