namespace Retlang
{
    public interface ICommandExceptionHandler
    {
        void AddExceptionHandler(OnException onExc);
        void RemoveExceptionHandler(OnException onExc);
    }
}