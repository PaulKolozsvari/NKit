namespace NKit.Winforms
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing.Drawing2D;

    #endregion //Using Directives

    public partial class CustomMainMenuWindows : MenuStrip
    {
        #region Constructors

        public CustomMainMenuWindows()
        {
            InitializeComponent();
            _colors = new CustomProfessionalColorsWindows();
            this.Renderer = new ToolStripProfessionalRenderer(_colors);
        }

        public CustomMainMenuWindows(CustomProfessionalColorsWindows colors)
        {
            InitializeComponent();
            _colors = colors;
            this.Renderer = new ToolStripProfessionalRenderer(_colors);
        }

        #endregion //Constructors

        #region Fields

        protected CustomProfessionalColorsWindows _colors;

        #endregion //Fields

        #region Methods

        public void SetColors(CustomProfessionalColorsWindows colors)
        {
            _colors = colors;
            this.Renderer = new ToolStripProfessionalRenderer(_colors);
        }

        #endregion //Methods

        #region Properties

        public Color MenuStripGradientBegin
        {
            get { return _colors.MenuStripGradientBegin; }
            set
            {
                _colors.SetMenuStripGradientBegin(value);
                this.Renderer = new ToolStripProfessionalRenderer(_colors);
                this.Refresh();
            }
        }

        public Color MenuStripGradientEnd
        {
            get { return _colors.MenuStripGradientEnd; }
            set
            {
                _colors.SetMenuStripGradientEnd(value);
                this.Renderer = new ToolStripProfessionalRenderer(_colors);
                this.Refresh();
            }
        }

        #endregion //Properties
    }
}