namespace Retlang
{
    public class AsyncRequestSubscriber<T>
    {
        private IUnsubscriber _replyTopic;
        private ITimerControl _timeoutControl;
        private readonly OnMessage<T> _onMsg;
        private readonly Command _onTimeout;

        public AsyncRequestSubscriber(OnMessage<T> onMsg, Command onTimeout)
        {
            _onMsg = onMsg;
            _onTimeout = onTimeout;
        }

        public IUnsubscriber Unsubscriber
        {
            get { return _replyTopic; }
            set { _replyTopic = value; }
        }

        public ITimerControl TimeoutControl
        {
            get { return _timeoutControl; }
            set { _timeoutControl = value; }
        }

        public void OnTimeout()
        {
            _replyTopic.Unsubscribe();
            if(_onTimeout != null)
                _onTimeout();
        }

        public void OnReceive(IMessageHeader header, T msg)
        {
            if (_timeoutControl != null)
            {
                _timeoutControl.Cancel();
            }
            _replyTopic.Unsubscribe();
            _onMsg(header, msg);
        }

    }
}
