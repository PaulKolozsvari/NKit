namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
using NKit.Winforms;

    #endregion //Using Directives

    /// <summary>
    /// A helper class for displaying a wait cursor.
    /// On this creation of an object of this WaiCursor class the current cursor will be set to a wait cursor.
    /// On calling Dispose on this object, the current cursor will set back to its default.
    /// This class should be used in a using clause e.g.
    /// 
    /// using(WaitCursor w = new WaitCursor())
    /// {
    ///     //Some code that will take some time to execute.
    /// }
    /// </summary>
    public class WaitProcessWindows : IDisposable
    {
        #region Constructors

        /// <summary>
        /// A helper class for displaying a wait cursor.
        /// On this calling this constructor the current cursor will be set to a wait cursor.
        /// On calling Dispose on this object, the current cursor will set back to its default.
        /// </summary>
        public WaitProcessWindows()
        {
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(MenuStrip mainMenuToDisable)
        {
            _mainMenu = mainMenuToDisable;
            UIHelperWindows.MenuEnabled(_mainMenu, false);
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(MenuStrip mainMenuToDisable, StatusBar statusBar)
        {
            _mainMenu = mainMenuToDisable;
            UIHelperWindows.MenuEnabled(_mainMenu, false);
            _statusBar = statusBar;
            if (_statusBar != null)
            {
                _originalStatus = _statusBar.Text;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(MenuStrip mainMenuToDisable, Label statusLabel)
        {
            _mainMenu = mainMenuToDisable;
            UIHelperWindows.MenuEnabled(_mainMenu, false);
            _statusLabel = statusLabel;
            if (_statusLabel != null)
            {
                _originalStatus = _statusLabel.Text;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(MenuStrip mainMenuToDisable, BorderlessFormWindows borderlessForm)
        {
            _mainMenu = mainMenuToDisable;
            UIHelperWindows.MenuEnabled(_mainMenu, false);
            _borderlessForm = borderlessForm;
            if (_borderlessForm != null)
            {
                _originalStatus = _borderlessForm.Status;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(BorderlessFormWindows borderlessForm)
        {
            _borderlessForm = borderlessForm;
            if (_borderlessForm != null)
            {
                _originalStatus = _borderlessForm.Status;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(StatusBar statusBar)
        {
            _statusBar = statusBar;
            if (_statusBar != null)
            {
                _originalStatus = _statusBar.Text;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public WaitProcessWindows(Label statusLabel)
        {
            _statusLabel = statusLabel;
            if (_statusBar != null)
            {
                _originalStatus = _statusLabel.Text;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        #endregion //Constructors

        #region Fields

        protected MenuStrip _mainMenu;
        protected StatusBar _statusBar;
        protected Label _statusLabel;
        protected BorderlessFormWindows _borderlessForm;
        protected string _originalStatus;

        #endregion //Fields

        #region Methods

        public void ChangeStatus(string status)
        {
            if (_statusBar != null)
            {
                _statusBar.Text = status;
            }
            if (_statusLabel != null)
            {
                _statusLabel.Text = status;
            }
            if (_borderlessForm != null)
            {
                _borderlessForm.Status = status;
            }
            Application.DoEvents();
        }

        /// <summary>
        /// The current cursor will set back to a default cursor.
        /// </summary>
        public void Dispose()
        {
            if (_mainMenu != null)
            {
                UIHelperWindows.MenuEnabled(_mainMenu, true);
            }
            if (_statusBar != null && _statusBar.Text != _originalStatus)
            {
                _statusBar.Text = _originalStatus;
            }
            if (_statusLabel != null && _statusLabel.Text != _originalStatus)
            {
                _statusLabel.Text = _originalStatus;
            }
            if (_borderlessForm != null && _borderlessForm.Status != _originalStatus)
            {
                _borderlessForm.Status = _originalStatus;
            }
            Cursor.Current = Cursors.Default;
        }

        #endregion //Methods
    }
}