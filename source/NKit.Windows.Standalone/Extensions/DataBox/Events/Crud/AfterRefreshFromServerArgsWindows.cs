﻿namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data.DB.SQLServer;

    #endregion //Using Directives

    public class AfterRefreshFromServerArgsWindows : AfterDataBoxArgsWindows
    {
        public AfterRefreshFromServerArgsWindows(bool presentLoadDataBoxForm, SqlDatabaseTableBaseWindows currentTable)
        {
            _presentLoadDataBoxForm = presentLoadDataBoxForm;
            _currentTable = currentTable;
        }

        #region Fields

        protected bool _presentLoadDataBoxForm;
        protected SqlDatabaseTableBaseWindows _currentTable;

        #endregion //Fields

        #region Properties

        public bool PresentLoadDataBoxForm
        {
            get { return _presentLoadDataBoxForm; }
        }

        public SqlDatabaseTableBaseWindows CurrentTable
        {
            get { return _currentTable; }
        }

        #endregion //Properties
    }
}
