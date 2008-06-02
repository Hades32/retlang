namespace Retlang
{
    /// <summary>
    /// Default command executor.
    /// </summary>
    public class CommandExecutor : ICommandExecutor
    {
        private bool _running = true;

        /// <summary>
        /// <see cref="ICommandExecutor.ExecuteAll(Command[])"/>
        /// </summary>
        /// <param name="toExecute"></param>
        public void ExecuteAll(Command[] toExecute)
        {
            foreach (Command command in toExecute)
            {
                if (_running)
                {
                    command();
                }
            }
        }

        /// <summary>
        /// When disabled, commands will be ignored by executor. The executor is typically disabled at shutdown
        /// to prevent any pending commands from being executed. 
        /// </summary>
        public bool IsEnabled
        {
            get { return _running; }
            set { _running = value; }
        }
    }
}