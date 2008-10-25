using System;

namespace Retlang.Core
{
    /// <summary>
    /// A synchronous queue typically used for testing.
    /// </summary>
    public class SynchronousCommandQueue : ICommandExecutor
    {
        private readonly DisposableList _disposables = new DisposableList();
        private bool _running = true;

        /// <summary>
        /// <see cref="IDisposingExecutor.Enqueue"/>
        /// </summary>
        /// <param name="commands"></param>
        public void EnqueueAll(params Command[] commands)
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
        /// Queue command
        /// </summary>
        /// <param name="command"></param>
        public void Enqueue(Command command)
        {
            if (_running)
            {
                command();
            }
        }

        /// <summary>
        /// Add Disposable.
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            _disposables.Add(toAdd);
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        public bool Remove(IDisposable victim)
        {
            return _disposables.Remove(victim);
        }

        /// <summary>
        /// Count of Disposables.
        /// </summary>
        public int DisposableCount
        {
            get { return _disposables.Count; }
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