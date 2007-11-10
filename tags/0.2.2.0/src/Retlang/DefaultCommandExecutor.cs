namespace Retlang
{
    public class CommandExecutor : ICommandExecutor
    {
        public void ExecuteAll(Command[] toExecute)
        {
            foreach (Command command in toExecute)
            {
                command();
            }
        }
    }
}