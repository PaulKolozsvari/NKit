namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #endregion //Using Directives

    public class AfterCrudOperationArgsWindows : AfterDataBoxArgsWindows
    {
        #region Constructors

        public AfterCrudOperationArgsWindows(object entity)
        {
            _entity = entity;
        }

        #endregion //Constructors

        #region Fields

        protected object _entity;

        #endregion //Fields

        #region Properties

        public object Entity
        {
            get { return _entity; }
        }

        #endregion //Properties
    }
}