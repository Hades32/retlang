using System;
using System.Collections.Generic;

namespace Retlang.Core
{
    public static class Lists
    {
        public static void Swap(ref List<Action> a, ref List<Action> b)
        {
            List<Action> tmp = a;
            a = b;
            b = tmp;
        }
    }
}