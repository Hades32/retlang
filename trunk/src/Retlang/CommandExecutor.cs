namespace Retlang
{
    public interface ICommandExecutor
    {
        void ExecuteAll(Command[] toExecute);
    }
}