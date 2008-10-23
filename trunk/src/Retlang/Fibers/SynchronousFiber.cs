using System;
using System.Collections.Generic;
using Retlang.Core;

namespace Retlang.Fibers
{
    public class SynchronousFiber : IFiber
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly List<Command> _pending = new List<Command>();
        private readonly List<ScheduledEvent> _scheduled = new List<ScheduledEvent>();
        private bool _executePendingImmediately;

        public void Start()
        {
        }

        public void Dispose()
        {
        }

        public void EnqueueAll(params Command[] commands)
        {
            _pending.AddRange(commands);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();       
            }
        }

        public void Enqueue(Command command)
        {
            _pending.Add(command);
            if (_executePendingImmediately)
            {
                ExecuteAllPending();
            }
        }

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public bool Remove(IDisposable disposable)
        {
            return _disposables.Remove(disposable);
        }

        public int DisposableCount
        {
            get { return _disposables.Count; }
        }

        public ITimerControl Schedule(Command command, long timeTilEnqueueInMs)
        {
            ScheduledEvent toAdd = new ScheduledEvent(command, timeTilEnqueueInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerCommand(command, timeTilEnqueueInMs, 
                timeTilEnqueueInMs, _scheduled, toAdd);
        }

        public ITimerControl ScheduleOnInterval(Command command, long firstInMs, long regularInMs)
        {
            ScheduledEvent toAdd = new ScheduledEvent(command, firstInMs, regularInMs);
            _scheduled.Add(toAdd);

            return new SynchronousTimerCommand(command, firstInMs,
                regularInMs, _scheduled, toAdd);
        }

        public List<IDisposable> Disposables
        {
            get { return _disposables; }
        }

        public List<Command> Pending
        {
            get { return _pending; }
        }

        public List<ScheduledEvent> Scheduled
        {
            get { return _scheduled; }
        }

        public bool ExecutePendingImmediately
        {
            get { return _executePendingImmediately; }
            set { _executePendingImmediately = value; }
        }

        public void ExecuteAllPending()
        {
            foreach (Command command in _pending)
            {
                command();
            }
            _pending.Clear();
        }

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
