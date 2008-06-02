using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Retlang
{
    /// <summary>
    /// A scheduled event.
    /// </summary>
    public interface IPendingEvent : ITimerControl
    {
        /// <summary>
        /// Time of expiration for this event
        /// </summary>
        long Expiration { get; }

        /// <summary>
        /// Execute this event and optionally schedule another execution.
        /// </summary>
        /// <returns></returns>
        IPendingEvent Execute(long currentTime);
    }

    internal class SingleEvent : IPendingEvent
    {
        private readonly ICommandQueue _queue;
        private readonly Command _toExecute;
        private readonly long _expiration;
        private bool _canceled;

        public SingleEvent(ICommandQueue queue, Command toExecute, long scheduledTimeInMs, long now)
        {
            _expiration = now + scheduledTimeInMs;
            _queue = queue;
            _toExecute = toExecute;
        }

        public long Expiration
        {
            get { return _expiration; }
        }

        public IPendingEvent Execute(long currentTime)
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
        private long _expiration;
        private bool _canceled;

        public RecurringEvent(ICommandQueue queue, Command toExecute, long scheduledTimeInMs, long regularInterval,
                              long currentTime)
        {
            _expiration = currentTime + scheduledTimeInMs;
            _queue = queue;
            _toExecute = toExecute;
            _regularInterval = regularInterval;
        }

        public long Expiration
        {
            get { return _expiration; }
        }

        public IPendingEvent Execute(long currentTime)
        {
            if (!_canceled)
            {
                _queue.Enqueue(_toExecute);
                _expiration = currentTime + _regularInterval;
                //Console.WriteLine(currentTime + " - " + _expiration);
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
    internal class TimerThread : IDisposable
    {
        private readonly SortedList<long, List<IPendingEvent>> _pending =
            new SortedList<long, List<IPendingEvent>>();

        private static readonly long _freq = Stopwatch.Frequency;
        private static readonly double MsMultiplier = 1000.00/_freq;
        private readonly long _startTimeInTicks = Stopwatch.GetTimestamp();

        private ManualResetEvent _waiter;
        //private RegisteredWaitHandle _cancel = null;
        private readonly object _lock = new object();
        private bool _running = true;

        public void Start()
        {
        }

        public ITimerControl Schedule(ICommandQueue targetQueue, Command toExecute, long scheduledTimeInMs)
        {
            SingleEvent pending = new SingleEvent(targetQueue, toExecute, scheduledTimeInMs, ElapsedMs());
            QueueEvent(pending);
            return pending;
        }

        public ITimerControl ScheduleOnInterval(ICommandQueue queue, Command toExecute, long scheduledTimeInMs,
                                                long intervalInMs)
        {
            RecurringEvent pending = new RecurringEvent(queue, toExecute, scheduledTimeInMs, intervalInMs, ElapsedMs());
            QueueEvent(pending);
            return pending;
        }

        private long ElapsedMs()
        {
            return (long) ((Stopwatch.GetTimestamp() - _startTimeInTicks)*MsMultiplier);
        }

        public void QueueEvent(IPendingEvent pending)
        {
            lock (_lock)
            {
                AddPending(pending);
                if (_waiter != null)
                {
                    _waiter.Set();
                    _waiter = null;
                }
                else
                {
                    OnTimeCheck(null, false);
                }
            }
        }

        private void AddPending(IPendingEvent pending)
        {
            List<IPendingEvent> list = null;
            if (!_pending.TryGetValue(pending.Expiration, out list))
            {
                list = new List<IPendingEvent>(2);
                _pending[pending.Expiration] = list;
            }
            list.Add(pending);
        }

        private bool SetTimer()
        {
            if (_pending.Count > 0)
            {
                long timeInMs = 0;
                if (GetTimeTilNext(ref timeInMs, ElapsedMs()))
                {
                    _waiter = new ManualResetEvent(false);
                    ThreadPool.RegisterWaitForSingleObject(_waiter, OnTimeCheck, timeInMs,
                                                           (uint) timeInMs, true);
                    //Console.WriteLine("Time till next: " + timeInMs);
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void OnTimeCheck(object sender, bool timeout)
        {
            if (!_running)
                return;
            lock (_lock)
            {
                do
                {
                    List<IPendingEvent> rescheduled = ExecuteExpired();
                    Queue(rescheduled);
                } while (!SetTimer());
            }
        }

        private void Queue(List<IPendingEvent> rescheduled)
        {
            if (rescheduled != null)
            {
                foreach (IPendingEvent pendingEvent in rescheduled)
                {
                    QueueEvent(pendingEvent);
                }
            }
        }

        private List<IPendingEvent> ExecuteExpired()
        {
            SortedList<long, List<IPendingEvent>> expired = RemoveExpired();
            List<IPendingEvent> rescheduled = null;
            if (expired.Count > 0)
            {
                foreach (KeyValuePair<long, List<IPendingEvent>> pair in expired)
                {
                    foreach (IPendingEvent pendingEvent in pair.Value)
                    {
                        IPendingEvent next = pendingEvent.Execute(ElapsedMs());
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
            return rescheduled;
        }

        private SortedList<long, List<IPendingEvent>> RemoveExpired()
        {
            lock (_lock)
            {
                SortedList<long, List<IPendingEvent>> expired = new SortedList<long, List<IPendingEvent>>();
                foreach (KeyValuePair<long, List<IPendingEvent>> pair in _pending)
                {
                    if (ElapsedMs() >= pair.Key)
                    {
                        expired.Add(pair.Key, pair.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (KeyValuePair<long, List<IPendingEvent>> pair in expired)
                {
                    _pending.Remove(pair.Key);
                }
                return expired;
            }
        }

        public bool GetTimeTilNext(ref long time, long now)
        {
            time = 0;
            if (_pending.Count > 0)
            {
                foreach (KeyValuePair<long, List<IPendingEvent>> pair in _pending)
                {
                    if (now >= pair.Key)
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
            _running = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}