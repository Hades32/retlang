using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    /// <summary>
    /// Stores and removes pending commands.
    /// </summary>
    public interface IPendingCommandRegistry
    {
        /// <summary>
        /// Remove timer
        /// </summary>
        /// <param name="timer"></param>
        void Remove(ITimerControl timer);

        /// <summary>
        /// Queue event to target queue.
        /// </summary>
        /// <param name="command"></param>
        void EnqueueTask(Command command);
    }

    /// <summary>
    /// Controller to cancel event timer.
    /// </summary>
    public interface ITimerControl
    {
        /// <summary>
        /// Cancels scheduled timer.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Methods for schedule events that will be executed in the future.
    /// </summary>
    public interface ICommandTimer
    {
        /// <summary>
        /// Schedules an event to be executes once.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <returns>a controller to cancel the event.</returns>
        ITimerControl Schedule(Command command, long firstIntervalInMs);

        /// <summary>
        /// Schedule an event on a recurring interval.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <param name="regularIntervalInMs"></param>
        /// <returns>controller to cancel timer.</returns>
        ITimerControl ScheduleOnInterval(Command command, long firstIntervalInMs, long regularIntervalInMs);
    }

    internal class CommandTimer : IPendingCommandRegistry, ICommandTimer, IDisposable
    {
        private volatile bool _running = true;
        private readonly ICommandQueue _queue;
        private List<ITimerControl> _pending = new List<ITimerControl>();

        public CommandTimer(ICommandQueue queue)
        {
            _queue = queue;
        }

        public ITimerControl Schedule(Command comm, long timeTillEnqueueInMs)
        {
            if (timeTillEnqueueInMs <= 0)
            {
                PendingCommand pending = new PendingCommand(comm);
                _queue.Enqueue(pending.ExecuteCommand);
                return pending;
            }
            else
            {
                TimerCommand pending = new TimerCommand(comm, timeTillEnqueueInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        public ITimerControl ScheduleOnInterval(Command comm, long firstInMs, long intervalInMs)
        {
            TimerCommand pending = new TimerCommand(comm, firstInMs, intervalInMs);
            AddPending(pending);
            return pending;
        }

        public void Remove(ITimerControl toRemove)
        {
            Command removeCommand = delegate { _pending.Remove(toRemove); };
            _queue.Enqueue(removeCommand);
        }

        public void EnqueueTask(Command toExecute)
        {
            _queue.Enqueue(toExecute);
        }

        private void AddPending(TimerCommand pending)
        {
            Command addCommand = delegate
                                     {
                                         if (_running)
                                         {
                                             _pending.Add(pending);
                                             pending.Schedule(this);
                                         }
                                     };
            _queue.Enqueue(addCommand);
        }

        public void Dispose()
        {
            _running = false;
            List<ITimerControl> old = Interlocked.Exchange(ref _pending, new List<ITimerControl>());
            foreach (ITimerControl control in old)
            {
                control.Cancel();
            }
        }
    }
}