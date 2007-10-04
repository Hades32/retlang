namespace Retlang
{
    public class SynchronousCommandQueue : ICommandQueue, ICommandRunner
    {
        private bool _running = true;

        public void Enqueue(Command command)
        {
            if(_running)
                command();
        }

        public void Run()
        {
            _running = true;
        }

        public void Stop()
        {
            _running = false;
        }
    }
}