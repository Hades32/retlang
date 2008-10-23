namespace Retlang.Channels
{
    /// <summary>
    /// Base implementation for subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSubscription<T> : ISubscribable<T>
    {
        private Filter<T> _filterOnProducerThread;

        /// <summary>
        /// <see cref="ISubscribable{T}.FilterOnProducerThread"/>
        /// </summary>
        public Filter<T> FilterOnProducerThread
        {
            get { return _filterOnProducerThread; }
            set { _filterOnProducerThread = value; }
        }

        private bool PassesProducerThreadFilter(T msg)
        {
            return _filterOnProducerThread == null || _filterOnProducerThread(msg);
        }

        /// <summary>
        /// <see cref="IProducerThreadSubscriber{T}.ReceiveOnProducerThread"/>
        /// </summary>
        /// <param name="msg"></param>
        public void ReceiveOnProducerThread(T msg)
        {
            if (PassesProducerThreadFilter(msg))
            {
                OnMessageOnProducerThread(msg);
            }
        }

        /// <summary>
        /// Called after message has been filtered.
        /// </summary>
        /// <param name="msg"></param>
        protected abstract void OnMessageOnProducerThread(T msg);
    }
}
