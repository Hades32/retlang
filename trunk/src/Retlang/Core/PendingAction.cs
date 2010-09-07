using System;

namespace Retlang.Core
{
    internal class PendingAction : IDisposable
    {
        private readonly Action _toExecute;
        private bool _cancelled;

        public PendingAction(Action toExecute)
        {
            _toExecute = toExecute;
        }

        public void Dispose()
        {
            _cancelled = true;
        }

        public void Execute()
        {
            if (!_cancelled)
            {
                _toExecute();
            }
        }

        public override string ToString()
        {
            return _toExecute.ToString();
        }
    }
}
