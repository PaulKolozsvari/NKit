﻿namespace NKit.Utilities.Email
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public partial class EmailNotificationRecipient
    {
        #region Properties

        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }

        #endregion //Properties

        #region Methods

        public override string ToString()
        {
            return ($"{this.DisplayName} ({this.EmailAddress})");
        }

        #endregion //Methods
    }
}
