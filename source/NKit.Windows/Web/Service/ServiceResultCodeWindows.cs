﻿namespace NKit.Web.Service
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    #endregion //Using Directives

    public enum ServiceResultCodeWindows
    {
        Success = 0,
        Information = 1,
        Warning = 2,
        OperationError = 3,
        FatalError = 4,
    }
}