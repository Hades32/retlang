using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IProcessThread: ICommandQueue, ICommandTimer
    {
        void Start();
        void Stop();
        void Join();
    }

    public class ProcessThread: IProcessThread
    {
        private readonly Thread _thread;
        private readonly ICommandRunner _queue;
        private readonly CommandTimer _scheduler;

        public ProcessThread(ICommandRunner queue)
        {
            _queue = queue;
            _thread = new Thread(RunThread);
            _scheduler = new CommandTimer(this);
        }

        private void RunThread()
        {
            _queue.Run();
        }

        public void Enqueue(OnCommand command)
        {
            _queue.Enqueue(command);
        }

        public void Schedule(OnCommand command, int intervalInMs)
        {
            _scheduler.Schedule(command, intervalInMs);
        }

        public void ScheduleOnInterval(OnCommand command, int firstInMs, int intervalInMs)
        {
            _scheduler.ScheduleOnInterval(command, firstInMs, intervalInMs);
        }

        public void Stop()
        {
            _queue.Stop();
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Join()
        {
            _thread.Join();
        }
    }
}
