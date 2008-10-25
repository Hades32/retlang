using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    /// <summary>
    /// Synchronous Fiber does not use a backing thread or a thread pool for execution. Events are added to pending
    /// lists for execution. These events can be executed synchronously by a calling thread. This class
    /// is not thread safe and probably should not be used in production code. 
    /// 
    /// The class is typically used for unit testing asynchronous code to make it completely synchronous and
    /// deterministic.
    /// </summary>
    public class SynchronousFiber : IFiber
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<Command> _pending = new List<Command>();
        private readonly List<ScheduledEvent> _scheduled = new List<ScheduledEvent>();
        private bool _executePendingImmediately;

        /// <summary>
        /// No Op
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// No Op
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Adds all events to pending list.
        /// </summary>
        /// <param name="commands"></param>
        public void EnqueueAll(params Command[] commands)
        {
            _pending.AddRange(commands);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();       
            }
        }

        /// <summary>
        /// Add event to pending list.
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            _pending.Add(command);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();
            }
        }

        /// <summary>
        /// add to disposable list.
        /// </summary>
        /// <param name="disposable"></param>
        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="disposable"></param>
        /// <returns></returns>
        public bool Remove(IDisposable disposable)
        {
            return _disposables.Remove(disposable);
        }

        /// <summary>
        /// Count of Disposables.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        /// <summary>
        /// Adds a scheduled event to the list. 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <returns></returns>
        public ITimerControl Schedule(Command command, long timeTilEnqueueInMs)
        {
            ScheduledEvent toAdd = new ScheduledEvent(command, timeTilEnqueueInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerCommand(command, timeTilEnqueueInMs, 
                timeTilEnqueueInMs, _scheduled, toAdd);
        }

        /// <summary>
        /// Adds scheduled event to list.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long regularInMs)
        {
            ScheduledEvent toAdd = new ScheduledEvent(command, firstInMs, regularInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerCommand(command, firstInMs,
                regularInMs, _scheduled, toAdd);
        }

        /// <summary>
        /// All Disposables.
        /// </summary>
        public List<IDisposable> Disposables
        {
            get { return _disposables; }
        }

        /// <summary>
        /// All Pending commands.
        /// </summary>
        public List<Command> Pending
        {
            get { return _pending; }
        }

        /// <summary>
        /// All Scheduled events.
        /// </summary>
        public List<ScheduledEvent> Scheduled
        {
            get { return _scheduled; }
        }

        /// <summary>
        /// If true events will be executed immediately rather than added to a pending list.
        /// </summary>
        public bool ExecutePendingImmediately
        {
            get { return _executePendingImmediately; }
            set { _executePendingImmediately = value; }
        }

        /// <summary>
        /// Execute all commands in the pending list.
        /// </summary>
        public void ExecuteAllPending()
        {
            foreach (Command command in _pending)
            {
                command();
            }
            _pending.Clear();
        }

        /// <summary>
        /// execute all scheduled.
        /// </summary>
        public void ExecuteAllScheduled()
        {
            foreach (ScheduledEvent scheduledEvent in _scheduled)
            {
                scheduledEvent.Command();
            }
            _scheduled.Clear();
        }
    }
}
