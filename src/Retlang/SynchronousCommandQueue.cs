namespace Retlang
{
    public class SynchronousCommandQueue : ICommandQueue
    {
        public void Enqueue(Command command)
        {
            command();
        }
    }
}