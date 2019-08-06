namespace NKit.Utilities.Jobs
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NKit.Data;

    #endregion //Using Directives

    public abstract class DailyJobWindows : DailyJob
    {
        #region Inner Types

        public class OnDailyJobFeedbackEventArgs : EventArgs
        {
            #region Constructors

            public OnDailyJobFeedbackEventArgs(
                DateTime nextExecutionDateTime, 
                TimeSpan waitTimeBeforeStart,
                bool currentlyExecuting)
            {
                _nextExecutionDateTime = nextExecutionDateTime;
                _waitTimeBeforeStart = waitTimeBeforeStart;
                _currentlyExecuting = currentlyExecuting;
            }

            #endregion //Constructors

            #region Fields

            private DateTime _nextExecutionDateTime;
            private TimeSpan _waitTimeBeforeStart;
            private bool _currentlyExecuting;

            #endregion //Fields

            #region Properties

            public DateTime NextExecutionDateTime
            {
                get { return _nextExecutionDateTime; }
            }

            public TimeSpan WaitTimeBeforeStart
            {
                get { return _waitTimeBeforeStart; }
            }

            public bool CurrentlyExecuting
            {
                get { return _currentlyExecuting; }
            }

            #endregion //Properties
        }

        public delegate void OnDailyJobFeedBack(object sender, OnDailyJobFeedbackEventArgs e);

        #endregion //Inner Types

        #region Constructors

        public DailyJobWindows(
            DateTime nextExecutionDateTime,
            bool startImmediately) : base(nextExecutionDateTime, startImmediately)
        {
        }

        #endregion //Constructors

        #region Methods

        protected override void TimerElapsed(object state)
        {
            StopJob();
            try
            {
                BeginExecution(this);
            }
            catch (Exception ex)
            {
                ExceptionHandlerWindows.HandleException(ex);
            }
            finally
            {
                if (!this.IsEnabled)
                {
                    StartJob();
                }
            }
        }

        #endregion //Methods
    }
}