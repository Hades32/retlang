namespace Retlang.Core
{
    internal class PendingCommand : ITimerControl
    {
        private readonly Command _toExecute;
        private bool _cancelled;

        public PendingCommand(Command toExecute)
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
