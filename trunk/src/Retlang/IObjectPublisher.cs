namespace Retlang
{
    /// <summary>
    /// Methods for publishing messages.
    /// </summary>
    public interface IObjectPublisher
    {
        /// <summary>
        /// Publish a message with a reply Topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        /// <param name="replyToTopic"></param>
        void Publish(object topic, object msg, object replyToTopic);

        /// <summary>
        /// Publish message on the given topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="msg"></param>
        void Publish(object topic, object msg);
    }
}