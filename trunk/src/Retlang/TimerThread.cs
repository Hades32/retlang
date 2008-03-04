using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IPendingEvent : ITimerControl
    {
        /// <summary>
        /// Time of expiration for this event
        /// </summary>
        DateTime Expiration { get; }

        /// <summary>
        /// Execute this event and optionally schedule another execution.
        /// </summary>
        /// <returns></returns>
        IPendingEvent Execute();
    }

    public class SingleEvent : IPendingEvent
    {
        private readonly ICommandQueue _queue;
        private readonly Command _toExecute;
        private readonly DateTime _expiration;
        private bool _canceled;

        public SingleEvent(ICommandQueue queue, Command toExecute, long scheduledTimeInMs, DateTime now)
        {
            _expiration = now.AddMilliseconds(scheduledTimeInMs);
            _queue = queue;
            _toExecute = toExecute;
        }

        public DateTime Expiration
        {
            get { return _expiration; }
        }

        public IPendingEvent Execute()
        {
            if (!_canceled)
            {
                _queue.Enqueue(_toExecute);
            }
            return null;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }

    internal class RecurringEvent : IPendingEvent
    {
        private readonly ICommandQueue _queue;
        private readonly Command _toExecute;
        private readonly long _regularInterval;
        private DateTime _expiration;
        private bool _canceled;

        public RecurringEvent(ICommandQueue queue, Command toExecute, long scheduledTimeInMs, long regularInterval)
        {
            _expiration = CalculateExpiration(scheduledTimeInMs);
            _queue = queue;
            _toExecute = toExecute;
            _regularInterval = regularInterval;
        }

        private static DateTime CalculateExpiration(long scheduledTimeInMs)
        {
            return DateTime.Now.AddMilliseconds(scheduledTimeInMs);
        }

        public DateTime Expiration
        {
            get { return _expiration; }
        }

        public IPendingEvent Execute()
        {
            if (!_canceled)
            {
                _queue.Enqueue(_toExecute);
                _expiration = CalculateExpiration(_regularInterval);
                return this;
            }
            return null;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }

    /// <summary>
    /// A Thread dedicated to event scheduling.
    /// </summary>
    public class TimerThread : IDisposable
    {
        private readonly SortedList<DateTime, List<IPendingEvent>> _pending =
            new SortedList<DateTime, List<IPendingEvent>>();

        private readonly Thread _thread;
        private readonly object _lock = new object();
        private bool _running = true;

        public TimerThread()
        {
            _thread = new Thread(RunTimer);
            _thread.Name = "RetlangTimerThread";
            _thread.IsBackground = true;
        }

        public void Start()
        {
            _thread.Start();
        }

        public ITimerControl Schedule(ICommandQueue targetQueue, Command toExecute, long scheduledTimeInMs)
        {
            SingleEvent pending = new SingleEvent(targetQueue, toExecute, scheduledTimeInMs, DateTime.Now);
            QueueEvent(pending);
            return pending;
        }

        public ITimerControl ScheduleOnInterval(ICommandQueue queue, Command toExecute, long scheduledTimeInMs,
                                                long intervalInMs)
        {
            RecurringEvent pending = new RecurringEvent(queue, toExecute, scheduledTimeInMs, intervalInMs);
            QueueEvent(pending);
            return pending;
        }

        public void QueueEvent(IPendingEvent pending)
        {
            lock (_lock)
            {
                List<IPendingEvent> list = null;
                if (!_pending.TryGetValue(pending.Expiration, out list))
                {
                    list = new List<IPendingEvent>(2);
                    _pending[pending.Expiration] = list;
                }
                list.Add(pending);
                Monitor.Pulse(_lock);
            }
        }

        private void RunTimer(object state)
        {
            while (_running)
            {
                SortedList<DateTime, List<IPendingEvent>> expired = RemoveExpired();
                List<IPendingEvent> rescheduled = null;
                if (expired.Count > 0)
                {
                    foreach (KeyValuePair<DateTime, List<IPendingEvent>> pair in expired)
                    {
                        foreach (IPendingEvent pendingEvent in pair.Value)
                        {
                            IPendingEvent next = pendingEvent.Execute();
                            if (next != null)
                            {
                                if (rescheduled == null)
                                {
                                    rescheduled = new List<IPendingEvent>(1);
                                }
                                rescheduled.Add(next);
                            }
                        }
                    }
                }
                lock (_lock)
                {
                    if(rescheduled != null)
                    {
                        foreach (IPendingEvent pendingEvent in rescheduled)
                        {
                            QueueEvent(pendingEvent);
                        }
                    }
                    if (_pending.Count > 0)
                    {
                        TimeSpan timeInTicks = TimeSpan.Zero;
                        if(GetTimeTilNext(ref timeInTicks, DateTime.Now))
                        {

                        if (timeInTicks != TimeSpan.Zero)
                        {
                            if (timeInTicks.TotalMilliseconds < 1)
                            {
                                Monitor.Wait(_lock, 1);
                            }
                            else
                            {
                                Monitor.Wait(_lock, timeInTicks, false);
                            }
                        }
                        }
                    }
                    else
                    {
                        if (_running)
                        {
                            Monitor.Wait(_lock);
                        }
                    }
                }
            }
        }

        private SortedList<DateTime, List<IPendingEvent>> RemoveExpired()
        {
            lock (_lock)
            {
                SortedList<DateTime, List<IPendingEvent>> expired = new SortedList<DateTime, List<IPendingEvent>>();
                DateTime now = DateTime.Now;
                foreach (KeyValuePair<DateTime, List<IPendingEvent>> pair in _pending)
                {
                    if (now >= pair.Key)
                    {
                        expired.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (KeyValuePair<DateTime, List<IPendingEvent>> pair in expired)
                {
                    _pending.Remove(pair.Key);
                }
                return expired;
            }
        }

        public bool GetTimeTilNext(ref TimeSpan time, DateTime now)
        {
            time = TimeSpan.Zero;
            if (_pending.Count > 0)
            {
                foreach (KeyValuePair<DateTime, List<IPendingEvent>> pair in _pending)
                {
                    if(now >= pair.Key)
                    {
                        return false;
                    }
                    time = (pair.Key - now);
                    return true;
                }
            }
            return false;
        }

        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.Pulse(_lock);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}