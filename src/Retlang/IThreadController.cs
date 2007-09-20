namespace Retlang
{
    public interface IThreadController
    {
        void Start();
        void Stop();
        void Join();
    }
}