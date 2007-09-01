using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IProcessContext: ICommandTimer
    {
        void Start();
        void Stop();
        void Join();

        void Enqueue(OnCommand command);
        void Publish(object topic, object msg);
        IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg);

        IRequestReply<T> SendRequest<T>(object topic, object msg);
    }

    public class ProcessContext: IProcessContext
    {
        private readonly IMessageBus _bus;
        private readonly IProcessThread _queue;

        public ProcessContext(IMessageBus messageBus, IProcessThread runner )
        {
            _bus = messageBus;
            _queue = runner;
        }

        public void Start()
        {
            _queue.Start();
        }

        public void Stop()
        {
            _queue.Stop();
        }

        public void Join()
        {
            _queue.Join();
        }

        public void Schedule(OnCommand command, int intervalInMs)
        {
            _queue.Schedule(command, intervalInMs);
        }

        public void ScheduleOnInterval(OnCommand command, int firstIntervalInMs, int regularIntervalInMs)
        {
            _queue.ScheduleOnInterval(command, firstIntervalInMs, regularIntervalInMs);
        }

        public void Enqueue(OnCommand command)
        {
            _queue.Enqueue(command);
        }

        public void Publish(object topic, object msg)
        {
            _bus.Publish(topic, msg);
        }

        public IUnsubscriber Subscribe<T>(ITopicMatcher topic, OnMessage<T> msg)
        {
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(topic, msg, _queue);
            _bus.Subscribe(subscriber);
            return new Unsubscriber(subscriber, _bus);
        }

        public IRequestReply<T> SendRequest<T>(object topic, object msg)
        {
            object requestTopic = new object();
            TopicRequestReply<T> req = new TopicRequestReply<T>();
            TopicSubscriber<T> subscriber = new TopicSubscriber<T>(new TopicMatcher(requestTopic), req.OnReply, _bus.Thread);
            _bus.Subscribe(subscriber);
            req.Unsubscriber = new Unsubscriber(subscriber, _bus);
            _bus.Publish(topic, msg, requestTopic);
            return req;
        }

    }
}
