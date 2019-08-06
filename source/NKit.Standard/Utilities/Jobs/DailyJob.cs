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

    public abstract partial class DailyJob
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

        public DailyJob(
            DateTime nextExecutionDateTime,
            bool startImmediately)
        {
            _nextExecutionDateTime = nextExecutionDateTime;
            if (startImmediately)
            {
                StartJob();
            }
        }

        #endregion //Constructors

        #region Events

        public event OnDailyJobFeedBack OnDailyJobStarted;
        public event OnDailyJobFeedBack OnDailyJobStopped;

        #endregion //Events

        #region Fields

        protected DateTime _nextExecutionDateTime;
        protected TimeSpan _waitTimeBeforeStart;
        protected System.Threading.Timer _timer;
        protected readonly object _lockObject = new object();

        protected bool _enabled;
        protected bool _currentlyExecuting;

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

        public bool IsEnabled
        {
            get { return _enabled; }
        }

        public bool CurrentlyExecuting
        {
            get { return _currentlyExecuting; }
        }

        #endregion //Properties

        #region Methods

        #region Helper Methods

        protected DateTime GetTodayMidnightDateTime(Nullable<DateTime> currentDateTime)
        {
            if (!currentDateTime.HasValue)
            {
                currentDateTime = DateTime.Now;
            }
            DateTime midnight = new DateTime(
                currentDateTime.Value.Year,
                currentDateTime.Value.Month,
                currentDateTime.Value.Day,
                0,
                0,
                0);
            return midnight.AddDays(1);
        }

        #endregion //Helper Methods

        public void StartJob()
        {
            DateTime currentDateTime = DateTime.Now;
            if (_nextExecutionDateTime < currentDateTime) //The execution date has passed already, therefore set the waitTimeBeforeStart to tomorrow.
            {
                DateTime midnight = GetTodayMidnightDateTime(currentDateTime);
                TimeSpan timeBeforeMidnight = midnight.Subtract(currentDateTime);
                if (_nextExecutionDateTime.TimeOfDay < currentDateTime.TimeOfDay) //Next execution time has also passed, therefore make it tomorrow at the scheduled next execution time.
                {
                    _waitTimeBeforeStart = timeBeforeMidnight.Add(new TimeSpan( //Make the next execution tomorrow.
                        0, //Days
                        _nextExecutionDateTime.Hour,
                        _nextExecutionDateTime.Minute,
                        _nextExecutionDateTime.Second,
                        _nextExecutionDateTime.Millisecond));
                }
                else
                {
                    _waitTimeBeforeStart = _nextExecutionDateTime.TimeOfDay.Subtract(currentDateTime.TimeOfDay); //Next execution time has not passed yet, therefore make it today at the scheduled next execution time.
                }
            }
            else
            {
                _waitTimeBeforeStart = _nextExecutionDateTime.Subtract(currentDateTime);
            }
            _nextExecutionDateTime = currentDateTime.Add(_waitTimeBeforeStart); //The sub class till have to update its settings/database with the next execution time.
            if (_timer == null)
            {
                _timer = new System.Threading.Timer(TimerElapsed, null, _waitTimeBeforeStart, new TimeSpan(0, 0, 0, 0, -1)); //Will run once off.
            }
            else
            {
                _timer.Change(_waitTimeBeforeStart, new TimeSpan(0, 0, 0, 0, -1)); //Will run once off.
            }
            _enabled = true;
            if (OnDailyJobStarted != null)
            {
                OnDailyJobStarted(this, new OnDailyJobFeedbackEventArgs(_nextExecutionDateTime, _waitTimeBeforeStart, _currentlyExecuting));
            }
        }

        public void StopJob()
        {
            _timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); //Stops the timer from ever running.
            _enabled = false;
            if (OnDailyJobStopped != null)
            {
                OnDailyJobStopped(this, new OnDailyJobFeedbackEventArgs(_nextExecutionDateTime, _waitTimeBeforeStart, _currentlyExecuting));
            }
        }

        public void SetCurrentlyExecutingFlag(bool currentlyExecuting)
        {
            _currentlyExecuting = currentlyExecuting;
        }

        protected virtual void TimerElapsed(object state)
        {
            StopJob();
            try
            {
                BeginExecution(this);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex);
            }
            finally
            {
                if (!this.IsEnabled)
                {
                    StartJob();
                }
            }
        }

        protected virtual void BeginExecution(DailyJob scheduledJob)
        {
            if (scheduledJob.CurrentlyExecuting)
            {
                return;
            }
            lock (_lockObject)
            {
                try
                {
                    scheduledJob.SetCurrentlyExecutingFlag(true);
                    ExecuteJob(scheduledJob);
                }
                finally
                {
                    scheduledJob.SetCurrentlyExecutingFlag(false);
                }
            }
        }

        protected abstract void ExecuteJob(DailyJob scheduledJob);

        #endregion //Methods
    }
}