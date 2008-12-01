using System;
using System.Diagnostics;

namespace RetlangTests
{
    public class PerfTimer : IDisposable
    {
        private readonly int _count;
        private readonly Stopwatch _stopWatch;

        public PerfTimer(int count)
        {
            _count = count;
            _stopWatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            var elapsed = _stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Elapsed: " + elapsed + " Events: " + _count);
            Console.WriteLine("Avg/S: " + (_count/(elapsed/1000.00)));
        }
    }
}