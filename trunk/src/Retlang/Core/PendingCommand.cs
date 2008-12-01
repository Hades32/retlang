using System;

namespace Retlang.Core
{
    internal class PendingCommand : ITimerControl
    {
        private readonly Action _toExecute;
        private bool _cancelled;

        public PendingCommand(Action toExecute)
        {
            _toExecute = toExecute;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        public void ExecuteCommand()
        {
            if (!_cancelled)
            {
                _toExecute();
            }
        }
    }
}
