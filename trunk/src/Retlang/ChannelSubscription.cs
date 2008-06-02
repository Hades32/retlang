using System;

namespace Retlang
{
    public delegate bool Filter<T>(T msg);

    public interface IProducerThreadSubscriber<T>
    {
        void ReceiveOnProducerThread(T msg);
    }
    

    public interface IChannelSubscription<T>: IProducerThreadSubscriber<T>
    {
        Filter<T> FilterOnProducerThread
        {
            get;
            set;
        }

    }

    public abstract class BaseSubscription<T>
    {
        private Filter<T> _filterOnProducerThread;

        public Filter<T> FilterOnProducerThread
        {
            get { return _filterOnProducerThread; }
            set { _filterOnProducerThread = value; }
        }

        private bool PassesProducerThreadFilter(T msg)
        {
            return _filterOnProducerThread == null || _filterOnProducerThread(msg);
        }

        public void ReceiveOnProducerThread(T msg)
        {
            if (PassesProducerThreadFilter(msg))
            {
                OnMessageOnProducerThread(msg);
            }
        }

        protected abstract void OnMessageOnProducerThread(T msg);
    }

    public class ChannelSubscription<T>: BaseSubscription<T>, IChannelSubscription<T>
    {
        private Action<T> _receiveMethod;
        private ICommandQueue _targetQueue;

        public ChannelSubscription(ICommandQueue queue, Action<T> receiveMethod)
        {
            _receiveMethod = receiveMethod;
            _targetQueue = queue;
        }

        protected override void OnMessageOnProducerThread(T msg)
        {
                Command asyncExec = delegate { _receiveMethod(msg); };
                _targetQueue.Enqueue(asyncExec);
         }
    }
}