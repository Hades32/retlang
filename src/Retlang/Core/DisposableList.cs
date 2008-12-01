using System;
using System.Collections.Generic;

namespace Retlang.Core
{
    /// <summary>
    /// Registry for disposables. Provides thread safe methods for list of disposables.
    /// </summary>
    public class DisposableList : IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<IDisposable> _items = new List<IDisposable>();

        /// <summary>
        /// Add Disposable
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            lock (_lock)
            {
                _items.Add(toAdd);
            }
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="victim"></param>
        /// <returns></returns>
        public bool Remove(IDisposable victim)
        {
            lock (_lock)
            {
                return _items.Remove(victim);
            }
        }

        /// <summary>
        /// Disposes all disposables registered in list.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var victim in _items.ToArray())
                {
                    victim.Dispose();
                }
            }
        }

        /// <summary>
        /// Number of registered disposables.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }
    }
}
