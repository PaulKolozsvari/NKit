﻿namespace NKit.Extensions.DataBox.Events
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class BeforeDataBoxArgsWindows : EventArgs
    {
        #region Properties

        public bool Cancel { get; set; }

        #endregion //Properties
    }
}
