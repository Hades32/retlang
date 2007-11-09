
namespace Retlang
{
    public class DefaultCommandExecutor: ICommandExecutor
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
