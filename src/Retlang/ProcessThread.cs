using System;
using System.Collections.Generic;
using System.Threading;

namespace Retlang
{
    public interface IProcessThread: ICommandQueue
    {
        void Start();
        void Stop();
        void Join();
    }

    public class ProcessThread: IProcessThread
    {
        private readonly Thread _thread;
        private readonly ICommandRunner _queue;

        public ProcessThread(ICommandRunner queue)
        {
            _queue = queue;
            _thread = new Thread(RunThread);
        }

        private void RunThread()
        {
            _queue.Run();
        }

        public void Enqueue(OnCommand command)
        {
            _queue.Enqueue(command);
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
