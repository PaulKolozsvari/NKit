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
            SqlDatabaseTableWindows currentTable,
            object selectedEntity,
            bool dataBoxInUpdateMode)
        {
            _currentTable = currentTable;
            _selectedEntity = selectedEntity;
            _dataBoxInUpdateMode = dataBoxInUpdateMode;
        }

        #endregion //Constructors

        #region Fields

        protected SqlDatabaseTableWindows _currentTable;
        protected object _selectedEntity;
        protected bool _dataBoxInUpdateMode;

        #endregion //Fields

        #region Properties

        public SqlDatabaseTableWindows CurrentTable
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
