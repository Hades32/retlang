using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ICommandExceptionHandler
    {
        void AddExceptionHandler(OnException onExc);
        void RemoveExceptionHandler(OnException onExc);
    }
}
