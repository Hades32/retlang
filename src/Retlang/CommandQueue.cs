using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public delegate void Command();
    public delegate void OnException(Command command, Exception failure);

    public interface ICommandQueue
    {
        void Enqueue(Command command);
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

        private readonly Queue<Command> _commands = new Queue<Command>();
        public event OnException ExceptionEvent;

        public int MaxDepth
        {
            get { return _maxQueueDepth; }
            set { _maxQueueDepth = value; }
        }

        public void Enqueue(Command command)
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

        public Command Dequeue()
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

        public Command[] DequeueAll()
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
                Command[] toReturn = _commands.ToArray();
                _commands.Clear();
                return toReturn;
            }
        }
        public bool ExecuteNextBatch()
        {
            Command[] toExecute = DequeueAll();
            if(toExecute == null)
            {
                return false;
            }
            foreach (Command command in toExecute)
            {
                ExecuteSingleCommand(command);
            }
            return true;
        }

        public bool ExecuteNext()
        {
            Command comm = Dequeue();
            if (comm != null)
            {
                ExecuteSingleCommand(comm);
                return true;
            }
            return false;
        }

        private void ExecuteSingleCommand(Command comm)
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
        }

        public void Run()
        {
            while (ExecuteNextBatch())
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
