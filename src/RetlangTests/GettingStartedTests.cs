using System;
using System.Collections.Generic;
using NUnit.Framework;
using Retlang;
using System.Threading;

namespace RetlangTests
{

    public class MultiplyController
    {
        private readonly object _uniqueTopic;
        private readonly IProcessContext _process;

        public MultiplyController(IProcessContext processContext)
        {
            _process = processContext;
            // create a unique exclusive topic to be used as an inbox
            _uniqueTopic = new object();
            _process.Subscribe<int>(new TopicEquals(_uniqueTopic), OnReply);
        }

        private void OnReply(IMessageHeader header, int num)
        {
            Console.WriteLine("Received Reply: " + num);
        }

        public void Send(int num)
        {
            _process.Publish("multiply.service", num, _uniqueTopic);
            Console.WriteLine("Sent: " + num);
        }


        internal void StopContextAfterNumReplies(int num)
        {
            int count = 0;
            OnMessage<int> shutdown = delegate
            {
                count++;
                if (count == num)
                {
                    _process.Stop();
                }
            };
            _process.Subscribe<int>(new TopicEquals(_uniqueTopic), shutdown);
        }
    }

    public class MultiplyService
    {
        private readonly IProcessContext _process;
        private readonly int _multiplyBy;

        public MultiplyService(IProcessContext processContext, int multiplyBy)
        {
            _multiplyBy = multiplyBy;
            _process = processContext;
            _process.Subscribe<int>(new TopicEquals("multiply.service"), OnMultiply);
        }

        private void OnMultiply(IMessageHeader header, int num)
        {
            object replyTopic = header.ReplyTo;
            int result = num * _multiplyBy;
            _process.Publish(replyTopic, result);
            Console.WriteLine("Published Reply: " + result);
        }
    }

    [TestFixture]
    public class GettingStartedTests
    {

        [Test]
        public void Example()
        {
            // all process contexts created from this factory will be able to exchange messages
            ProcessContextFactory factory = new ProcessContextFactory();
            // start the processing thread.
            factory.Start();

            IProcessContext controllerContext = factory.Create();
            // start the process thread
            controllerContext.Start();
            MultiplyController controller = new MultiplyController(controllerContext);

            IProcessContext firstContext = factory.Create();
            firstContext.Start();
            MultiplyService firstService = new MultiplyService(firstContext, 10);

            IProcessContext secondContext = factory.Create();
            secondContext.Start();
            MultiplyService secondService = new MultiplyService(secondContext, -1);

            //expect 2 replies to each request - one for each service
            controller.StopContextAfterNumReplies(2*2);
            controller.Send(5);
            controller.Send(100);

            // wait for requests to complete
            controllerContext.Join();
            firstContext.Stop();
            secondContext.Stop();
            firstContext.Join();
            secondContext.Join();
            factory.Stop();
            factory.Join();
        }
    }
}
