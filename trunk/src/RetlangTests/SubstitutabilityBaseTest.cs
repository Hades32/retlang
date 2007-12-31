using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    public abstract class SubstitutabilityBaseTest
    {
        private IProcessContextFactory _contextFactory;
        public abstract IProcessBus CreateBus(IProcessContextFactory factory);


        private IProcessBus _bus;

        [SetUp]
        public void Setup()
        {
            _contextFactory = new ProcessContextFactory();
            _contextFactory.Start();
            _bus = CreateBus(_contextFactory);
        }

        [TearDown]
        public void TearDown()
        {
            if (_bus != null)
            {
                _bus.Stop();
            }
            _contextFactory.Stop();
        }


        [Test]
        public void ScheduleBeforeStart()
        {
            ManualResetEvent reset = new ManualResetEvent(false);

            Command onReset = delegate { reset.Set(); };
            _bus.Schedule(onReset, 1);
            _bus.Start();

            Assert.IsTrue(reset.WaitOne(5000, false));
        }
    }


    [TestFixture]
    public class ThreadedContextTests : SubstitutabilityBaseTest
    {
        public override IProcessBus CreateBus(IProcessContextFactory factory)
        {
            return factory.Create();
        }
    }

    [TestFixture]
    public class ThreadPoolContextTests : SubstitutabilityBaseTest
    {
        public override IProcessBus CreateBus(IProcessContextFactory factory)
        {
            return factory.CreatePooled();
        }
    }
}