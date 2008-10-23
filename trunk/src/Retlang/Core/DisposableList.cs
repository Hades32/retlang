using System;
using System.Collections.Generic;

namespace Retlang.Core
{
    public class DisposableList : IDisposable
    {
        private readonly object _lock = new object();
        private readonly List<IDisposable> _items = new List<IDisposable>();

        public void Add(IDisposable toAdd)
        {
            lock (_lock)
            {
                _items.Add(toAdd);
            }
        }

        public bool Remove(IDisposable victim)
        {
            lock (_lock)
            {
                return _items.Remove(victim);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (IDisposable victim in _items.ToArray())
                {
                    victim.Dispose();
                }
            }
        }

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
