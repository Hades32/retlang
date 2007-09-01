using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class ProcessContextTests
    {
        [Test]
        public void ScheduleShutdown()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            OnCommand stopCommand = context.Stop;
            context.Schedule(stopCommand, 5);

            context.Join();

            factory.Stop();
            factory.Join();
        }

        [Test]
        public void ScheduleIntervalShutdown()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.Start();
            IProcessContext context = factory.Create();
            context.Start();

            int count = 0;
            OnCommand stopCommand = delegate
            {
                count++;
                if (count == 5)
                {
                    context.Stop();
                }
            };
            context.ScheduleOnInterval(stopCommand, 1,1);

            context.Join();

            factory.Stop();
            factory.Join();
            Assert.AreEqual(5, count);
        }
    }
}
