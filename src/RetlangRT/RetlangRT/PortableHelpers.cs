
using System.Collections.Generic;
namespace System
{
    public delegate K Converter<T, K>(T t);
}

namespace System.Threading
{
    public delegate void WaitCallback(Object state);
}

namespace System.Linq
{
    public static class ExtensionMethods
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}