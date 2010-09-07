using System;

namespace Retlang.Core
{
    internal class PendingAction : IDisposable
    {
        private readonly Action _action;
        private bool _cancelled;

        public PendingAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _cancelled = true;
        }

        public void Execute()
        {
            if (!_cancelled)
            {
                _action();
            }
        }

        public override string ToString()
        {
            return _action.ToString();
        }
    }
}
