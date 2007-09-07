using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public delegate void OnCommand();
    public delegate void OnException(OnCommand command, Exception failure);

    public interface ICommandQueue
    {
        void Enqueue(OnCommand command);
    }

    public interface ICommandRunner: ICommandQueue
    {
        event OnException ExceptionEvent;
        bool ExecuteNext();
        void Run();
        void Stop();
    }

    public class CommandQueue: ICommandRunner
    {
        private readonly object _lock = new object();
        private bool _running = true;
        private int _maxQueueDepth = -1;

        private readonly Queue<OnCommand> _commands = new Queue<OnCommand>();
        public event OnException ExceptionEvent;

        public int MaxDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        public void Enqueue(OnCommand command)
        {
            lock (_lock)
            {
                if(_maxQueueDepth> 0 && _commands.Count >= _maxQueueDepth)
                {
                    throw new QueueFullException(_commands.Count);
                }
                _commands.Enqueue(command);
                Monitor.PulseAll(_lock);
            }
        }

        public OnCommand Dequeue()
        {
            lock (_lock)
            {
                while (_commands.Count == 0 && _running)
                {
                    Monitor.Wait(_lock);
                }
                if (!_running)
                {
                    return null;
                }
                return _commands.Dequeue();
            }
        }

        public bool ExecuteNext()
        {
            OnCommand comm = Dequeue();
            if (comm != null)
            {
                try
                {
                    comm();
                }
                catch (Exception exc)
                {
                    OnException onExc = ExceptionEvent;
                    if (onExc != null)
                    {
                        onExc(comm, exc);
                    }
                    else
                    {
                        throw;
                    }
                }
                return true;
            }
            return false;
        }

        public void Run()
        {
            while (ExecuteNext())
            {
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
