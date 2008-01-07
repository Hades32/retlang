namespace Retlang
{
    public class ProcessContext : ProcessBus, IProcessContext
    {
        private readonly IProcessThread _processThread;

        public ProcessContext(IMessageBus messageBus, IProcessThread runner, ITransferEnvelopeFactory factory)
            : base(messageBus, runner, factory)
        {
            _processThread = runner;
        }

        public void Join()
        {
            _processThread.Join();
        }

        public bool Join(int milliseconds)
        {
            return _processThread.Thread.Join(milliseconds);
        }
    }
}