using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace SharpBrowser
{
    /// <summary>
    /// Provides Debounce() and Throttle() methods.
    /// Use these methods to ensure that events aren't handled too frequently.
    /// 
    /// Throttle() ensures that events are throttled by the interval specified.
    /// Only the last event in the interval sequence of events fires.
    /// 
    /// Debounce() fires an event only after the specified interval has passed
    /// in which no other pending event has fired. Only the last event in the
    /// sequence is fired.
    /// </summary>
    public class Debouncer
    {
        private Timer timer;

        /// <summary>
        /// Debounce an event by resetting the event timeout every time the event is 
        /// fired. The behavior is that the Action passed is fired only after events
        /// stop firing for the given timeout period.
        /// 
        /// Use Debounce when you want events to fire only after events stop firing
        /// after the given interval timeout period.
        /// 
        /// Wrap the logic you would normally use in your event code into
        /// the  Action you pass to this method to debounce the event.
        /// Example: https://gist.github.com/RickStrahl/0519b678f3294e27891f4d4f0608519a
        /// </summary>
        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="param">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="disp">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>        
        public void Debounce(int interval, Action<object> action,    object param = null  )
        {
            // kill pending timer and pending ticks
            timer?.Stop();
            timer = null;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            timer = new Timer();
            timer.Interval = (int)TimeSpan.FromMilliseconds(interval).TotalMilliseconds;
            timer.Tick += (s, e) =>
            {
                if (timer == null)
                    return;

                timer?.Stop();
                timer = null;
                action.Invoke(param);
            };

            timer.Start();
        }

    
    }


    /// <summary>
    ///  in a given interval  only the last action is run.  middle ones are dropped. with timer.
    /// </summary>
    public class Throttler
    {
        private Timer timer = new Timer();
        private DateTime _Last_Action_Started { get; set; } = DateTime.UtcNow.AddYears(-1);

        string _lockObj="";

        public void Throttle(int interval, Action<object> action, object param = null)
        {
            lock (_lockObj)
            {
                var curTime = DateTime.UtcNow;
                if (curTime.Subtract(_Last_Action_Started).TotalMilliseconds < interval) 
                {
                    interval -= (int)curTime.Subtract(_Last_Action_Started).TotalMilliseconds;
                }
                else
                {
                    //already passed the interval  . run immediately.
                    interval = 1; //1ms. cant be 0.
                }

                timer?.Stop();
                timer = null;

                timer = new Timer();
                timer.Interval = interval;
                timer.Tick += (s1, e1) => 
                {
                    if (timer == null)
                        return;
                    timer?.Stop();
                    timer = null;

                    action.Invoke(param);
                    _Last_Action_Started = curTime;
                };
                timer.Start();


            }

        }


    }


    /// <summary>
    /// uses thread.join. much more ui freeze??
    /// </summary>
    public class Throttlerv2
    {
        private System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
        private DateTime _Last_Action_Started { get; set; } = DateTime.UtcNow.AddYears(-1);

        string _lockObj = "";

        public void Throttle(int interval, Action<object> action, object param = null)
        {
            lock (_lockObj)
            {
                var caller_thread = System.Threading.Thread.CurrentThread; //without this it worked

                var curTime = DateTime.UtcNow;
                if (curTime.Subtract(_Last_Action_Started).TotalMilliseconds < interval)
                {
                    interval -= (int)curTime.Subtract(_Last_Action_Started).TotalMilliseconds;
                    cts.Cancel();
                    cts = new System.Threading.CancellationTokenSource();

                    Task.Delay(interval, cts.Token).ContinueWith(x => {
                        caller_thread.Join();
                        action.Invoke(param);
                        _Last_Action_Started = curTime;
                    });

                }
                else
                {
                    //already passed the interval  . run immediately.
                    action.Invoke(param);
                    _Last_Action_Started = curTime;
                }


            }

        }


    }




    /*    **examples**
          
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
       
        private void TextSearchText_KeyUp(object sender, KeyEventArgs e)
        {
            debounceTimer.Debounce(500, (p) =>
            {                
                Model.AppModel.Window.ShowStatus("Searching topics...");
                Model.TopicsFilter = TextSearchText.Text;
                Model.AppModel.Window.ShowStatus();
            });        
        }
     

     */

    /* **example model bind**


    public string TopicsFilter
    {
        get { return _topicsFilter; }
        set
        {
            if (value == _topicsFilter) return;
            _topicsFilter = value;
            OnPropertyChanged();

            // debounce the tree filter change notifications
            debounceTopicsFilter.Debounce(500, e => OnPropertyChanged(nameof(FilteredTopicTree)));
        }
    }
    private string _topicsFilter;
    private readonly DebounceDispatcher debounceTopicsFilter = new DebounceDispatcher();
     
     
     */


}
