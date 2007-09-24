using System;
using NUnit.Framework;
using Retlang;

namespace RetlangTests
{
    [TestFixture]
    public class FibonacciDemonstration
    {
        // Simple immutable class that serves as a message
        // to be passed between services.
        private class IntPair
        {
            private readonly int _first;
            private readonly int _second;

            public IntPair(int first, int second)
            {
                _first = first;
                _second = second;
            }

            public int First
            {
                get { return _first; }
            }

            public int Second
            {
                get { return _second; }
            }
        }

        // This class calculates the next value in a Fibonacci sequence.
        // It listens for the previous pair on one topic, and then publishes
        // a new pair with the latest value onto the reply topic.
        // When a specified limit is reached, it stops processing.
        private class FibonacciCalculator
        {
            private readonly IProcessContext _processContext;
            private readonly string _name;
            private readonly int _limit;

            public FibonacciCalculator(IProcessContext processContext, string name, int limit)
            {
                _processContext = processContext;
                _name = name;
                _processContext.Subscribe<IntPair>(new TopicEquals(name), CalculateNext);
                _limit = limit;
            }

            public void Begin(object listener, IntPair pair)
            {
                Console.WriteLine(_name + " " + pair.Second);
                _processContext.Publish(listener, pair, _name);
            }

            private void CalculateNext(IMessageHeader header, IntPair receivedPair)
            {
                int next = receivedPair.First + receivedPair.Second;
                IntPair pairToPublish = new IntPair(receivedPair.Second, next);
                _processContext.Publish(header.ReplyTo, pairToPublish, _name);
                if (next > _limit)
                {
                    Console.WriteLine("Stopping " + _name);
                    _processContext.Stop();
                    return;
                }
                Console.WriteLine(_name + " " + next);
            }
        }

        [Test]
        public void TestCalculations()
        {
            ProcessContextFactory factory = new ProcessContextFactory();
            factory.TransferEnvelopeFactory = new ObjectTransferEnvelopeFactory();
            factory.Start();

            // Two instances of the calculator are created.  One is named "Odd" 
            // (it calculates the 1st, 3rd, 5th... values in the sequence) the
            // other is named "Even".  They message each other back and forth
            // with the latest two values and successively build the sequence.
            IProcessContext contextOne = factory.Create();
            FibonacciCalculator oddCalculator = new FibonacciCalculator(contextOne, "Odd", 1000);

            IProcessContext contextTwo = factory.Create();
            new FibonacciCalculator(contextTwo, "Even", 1000);

            contextOne.Start();
            contextTwo.Start();

            IntPair start = new IntPair(0, 1);

            oddCalculator.Begin("Even", start);

            contextOne.Join();
            contextTwo.Join();
            factory.Stop();
            factory.Join();
        }
    }
}