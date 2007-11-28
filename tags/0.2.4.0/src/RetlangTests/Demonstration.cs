using System.Threading;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class Demonstration
    {
        // This class is a producer or source.  It is given a topic on
        // construction and will publish a sequence of ints starting 
        // at one and continuing to the specified limit to that topic.
        private class NumberSource
        {
            // The process (thread) that will run the code.
            private readonly IProcessContext _processContext;
            // The topic we wish to publish to.
            private readonly object _topic;
            // The highest number this number source will publish.
            private readonly int _limit;

            public NumberSource(IProcessContext processContext, object topic, int limit)
            {
                _processContext = processContext;
                _limit = limit;
                _topic = topic;
            }

            // This method will send the numbers 1 to limit to the topic.
            public void SendNumbers()
            {
                for (int idx = 1; idx <= _limit; idx++)
                {
                    _processContext.Publish(_topic, idx);
                }
            }
        }

        // This class is a consumer or sink.  It listens for ints to be
        // published on a specified topic and adds them to its internal sum.
        private class SummationSink
        {
            // The process (thread) that will listen for messages.
            private readonly IProcessContext _processContext;
            private readonly AutoResetEvent _completionEvent;
            private readonly int _expectedSum;
            private int _sum = 0;

            public SummationSink(IProcessContext processContext, object topic, AutoResetEvent completionEvent,
                                 int expectedSum)
            {
                _processContext = processContext;
                _completionEvent = completionEvent;
                _expectedSum = expectedSum;
                // We create a selector that will allow this class
                // to subscribe to the topic.
                ITopicMatcher topicSelector = new TopicEquals(topic);
                // The subscription demands a delegate to call upon
                // receiving a message.
                _processContext.Subscribe<int>(topicSelector, Add);
            }

            // Our summation method must implement this signature 
            // as it functions as a delegate.  The header is 
            // ignored in this simple example.
            private void Add(IMessageHeader header, int amount)
            {
                _sum += amount;
                if (_sum == _expectedSum)
                {
                    _completionEvent.Set();
                }
            }

            public int Sum
            {
                get { return _sum; }
            }
        }

        [Test]
        public void TestSummation()
        {
            // A factory that will create some threads.
            ProcessContextFactory pcf = new ProcessContextFactory();
            pcf.Start();

            // We create three sources, each running on its own thread
            // all publishing to the same topic.  We variously publish
            // the numbers 1-100, 1-50 and 1-200.
            IProcessContext processContextOne = pcf.Create();
            NumberSource numberSourceOne = new NumberSource(processContextOne, "Summation Channel", 100);

            IProcessContext processContextTwo = pcf.Create();
            NumberSource numberSourceTwo = new NumberSource(processContextTwo, "Summation Channel", 50);

            IProcessContext processContextThree = pcf.Create();
            NumberSource numberSourceThree = new NumberSource(processContextThree, "Summation Channel", 200);

            // We create one sink to listen to all the publishing threads.
            IProcessContext processContextFour = pcf.Create();
            int expectedTotal = 26425;
            AutoResetEvent completionEvent = new AutoResetEvent(false);
            SummationSink summationSink =
                new SummationSink(processContextFour, "Summation Channel", completionEvent, expectedTotal);

            // Start all the threads.
            processContextOne.Start();
            processContextTwo.Start();
            processContextThree.Start();
            processContextFour.Start();

            // send all the sequences from each source
            numberSourceOne.SendNumbers();
            numberSourceTwo.SendNumbers();
            numberSourceThree.SendNumbers();

            // Pause to allow all the threads to complete.
            completionEvent.WaitOne(10000, false);

            // And assert that the total is correct.
            Assert.AreEqual(26425, summationSink.Sum);

            // Notice that despite having three threads all contending to
            // send messages to one place, our code has no locks and still
            // preserves a single mutable total correctly.
        }
    }
}