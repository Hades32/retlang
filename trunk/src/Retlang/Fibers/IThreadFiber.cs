using System.Threading;

namespace Retlang.Fibers
{
    public interface IThreadFiber : IFiber
    {
        void Join();
        Thread Thread { get; }
    }
}
