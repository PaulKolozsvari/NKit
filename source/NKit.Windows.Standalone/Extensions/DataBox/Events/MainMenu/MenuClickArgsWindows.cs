namespace NKit.Extensions.DataBox.Events.MainMenu
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.SQLServer;
    using NKit.Extensions.DataBox;

    #endregion //Using Directives

    public class MenuClickArgsWindows : EventArgs
    {
        #region Constructors

        public MenuClickArgsWindows(
            SqlDatabaseTableBaseWindows currentTable,
            object selectedEntity,
            bool dataBoxInUpdateMode)
        {
            _currentTable = currentTable;
            _selectedEntity = selectedEntity;
            _dataBoxInUpdateMode = dataBoxInUpdateMode;
        }

        #endregion //Constructors

        #region Fields

        protected SqlDatabaseTableBaseWindows _currentTable;
        protected object _selectedEntity;
        protected bool _dataBoxInUpdateMode;

        #endregion //Fields

        #region Properties

        public SqlDatabaseTableBaseWindows CurrentTable
        {
            get { return _currentTable; }
        }

        public object SelectedEntity
        {
            get { return _selectedEntity; }
        }

        public bool DataBoxInUpdateMode
        {
            get { return _dataBoxInUpdateMode; }
        }

        #endregion //Properties
    }
}
