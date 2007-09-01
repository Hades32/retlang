using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IThreadController
    {
        void Start();
        void Stop();
        void Join();
    }
}
