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

    public abstract class IntervalJobWindows : IntervalJob
    {
        #region Constructors

        public IntervalJobWindows(int executionMilliSecondsInterval, bool startImmediately) : base(executionMilliSecondsInterval, startImmediately)
        {
        }

        #endregion //Constructors

        #region Methods

        protected override void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
                if (!this.IsEnabled())
                {
                    StartJob();
                }
            }
        }

        #endregion //Methods
    }
}