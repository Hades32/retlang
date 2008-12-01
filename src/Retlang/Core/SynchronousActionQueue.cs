using System;

namespace Retlang.Core
{
    /// <summary>
    /// A synchronous queue typically used for testing.
    /// </summary>
    public class SynchronousActionQueue : IActionExecutor
    {
        private readonly DisposableList _disposables = new DisposableList();
        private bool _running = true;

        /// <summary>
        /// <see cref="IDisposingExecutor.Enqueue"/>
        /// </summary>
        /// <param name="actions"></param>
        public void EnqueueAll(params Action[] actions)
        {
            if (_running)
            {
                foreach (var toExecute in actions)
                {
                    toExecute();
                }
            }
        }

        /// <summary>
        /// Queue action
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_running)
            {
                action();
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