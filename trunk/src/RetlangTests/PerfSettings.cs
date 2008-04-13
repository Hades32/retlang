using System;
using System.Runtime.InteropServices;

namespace RetlangTests
{
    public static class PerfSettings
    {
        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint period);

        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint period);
    }
}
