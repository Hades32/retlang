namespace Retlang
{
    /// <summary>
    /// ProcessBus backed by a dedicated thread.
    /// </summary>
    public class ProcessContext : ProcessBus, IProcessContext
    {
        private readonly IProcessThread _processThread;

        /// <summary>
        /// Construct new intance.
        /// </summary>
        /// <param name="messageBus"></param>
        /// <param name="runner"></param>
        /// <param name="factory"></param>
        public ProcessContext(IMessageBus messageBus, IProcessThread runner, ITransferEnvelopeFactory factory)
            : base(messageBus, runner, factory)
        {
            _processThread = runner;
        }

        /// <summary>
        /// Wait for backing thread to finish.
        /// </summary>
        public void Join()
        {
            _processThread.Join();
        }

        /// <summary>
        /// Wait with timeout for backing thread to finish.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public bool Join(int milliseconds)
        {
            return _processThread.Thread.Join(milliseconds);
        }
    }
}