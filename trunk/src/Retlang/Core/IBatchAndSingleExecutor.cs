using System;

namespace Retlang.Core
{
    public interface IBatchAndSingleExecutor : IBatchExecutor
    {
        void Execute(Action action);
    }
}