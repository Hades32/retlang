using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Retlang.Core
{
    /// <summary>
    /// A Thread dedicated to event scheduling.
    /// </summary>
    internal class TimerThread : IDisposable
    {
        private readonly SortedList<long, List<IPendingEvent>> _pending = new SortedList<long, List<IPendingEvent>>();

        private static readonly long _freq = Stopwatch.Frequency;
        private static readonly double MsMultiplier = 1000.00/_freq;

        private readonly long _startTimeInTicks = Stopwatch.GetTimestamp();
        private readonly object _lock = new object();

        private ManualResetEvent _waiter;
        private bool _running = true;

        public void Start()
        {}

        public ITimerControl Schedule(IDisposingExecutor executor, Command toExecute, long scheduledTimeInMs)
        {
            SingleEvent pending = new SingleEvent(executor, toExecute, scheduledTimeInMs, ElapsedMs());
            QueueEvent(pending);
            return pending;
        }

        public ITimerControl ScheduleOnInterval(IDisposingExecutor executor, Command toExecute, long scheduledTimeInMs,
                                                long intervalInMs)
        {
            RecurringEvent pending = new RecurringEvent(executor, toExecute, scheduledTimeInMs, intervalInMs, ElapsedMs());
            QueueEvent(pending);
            return pending;
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

        private long ElapsedMs()
        {
            return (long)((Stopwatch.GetTimestamp() - _startTimeInTicks) * MsMultiplier);
        }

        private void AddPending(IPendingEvent pending)
        {
            List<IPendingEvent> list;
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
            return true;
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

        private void Queue(IEnumerable<IPendingEvent> rescheduled)
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
    }
}