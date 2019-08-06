namespace NKit.Extensions.DataBox.Events.Crud
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    #endregion //Using Directives

    public class BeforeAddInputControlsArgsWindows : BeforeDataBoxArgsWindows
    {
        #region Constructors

        public BeforeAddInputControlsArgsWindows(
            Dictionary<string, Control> inputControls,
            bool inputControlLabelsIncluded)
        {
            _inputControls = inputControls;
            _inputControlLabelsIncluded = inputControlLabelsIncluded;
        }

        #endregion //Constructors

        #region Fields

        protected Dictionary<string, Control> _inputControls;
        protected bool _inputControlLabelsIncluded;

        #endregion //Fields

        #region Properties

        public Dictionary<string, Control> InputControls
        {
            get { return _inputControls; }
        }

        public bool InputControlLabelsIncluded
        {
            get { return _inputControlLabelsIncluded; }
        }

        #endregion //Properties
    }
}
