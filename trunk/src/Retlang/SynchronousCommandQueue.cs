namespace Retlang
{
    /// <summary>
    /// A synchronous queue typically used for testing.
    /// </summary>
    public class SynchronousCommandQueue : ICommandQueue, ICommandRunner
    {
        private bool _running = true;

        /// <summary>
        /// <see cref="ICommandQueue.Enqueue"/>
        /// </summary>
        /// <param name="commands"></param>
        public void Enqueue(params Command[] commands)
        {
            if (_running)
            {
                foreach (Command toExecute in commands)
                {
                    toExecute();
                }
            }
        }

        /// <summary>
        /// Start Consuming events.
        /// </summary>
        public void Run()
        {
            _running = true;
        }

        /// <summary>
        /// Stop consuming events.
        /// </summary>
        public void Stop()
        {
            _running = false;
        }
    }
}
