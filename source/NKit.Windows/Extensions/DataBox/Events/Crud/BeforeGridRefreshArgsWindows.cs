namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Winforms;

    #endregion //Using Directives

    public class BeforeGridRefreshArgsWindows : BeforeDataBoxArgsWindows
    {
        #region Constructors

        public BeforeGridRefreshArgsWindows(
            int selectedRowIndex,
            CustomDataGridViewWindows currentGrid,
            bool filtersEnabled,
            bool refreshFromServer)
        {
            _selectedRowIndex = selectedRowIndex;
            _currentGrid = currentGrid;
            _filtersEnabled = filtersEnabled;
            _refreshFromServer = refreshFromServer;
        }

        #endregion //Constructors

        #region Fields

        protected int _selectedRowIndex;
        protected CustomDataGridViewWindows _currentGrid;
        protected bool _filtersEnabled;
        protected bool _refreshFromServer;

        #endregion //Fields

        #region Properties

        public int SelectedRowIndex
        {
            get { return _selectedRowIndex; }
        }

        public CustomDataGridViewWindows CurrentGrid
        {
            get { return _currentGrid; }
        }

        public bool FiltersEnabled
        {
            get { return _filtersEnabled; }
        }

        public bool RefreshFromServer
        {
            get { return _refreshFromServer; }
        }

        #endregion //Properties
    }
}
