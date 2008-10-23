using System;

namespace Retlang.Core
{
    /// <summary>
    /// Thrown when a queue is full.
    /// </summary>
    public class QueueFullException : Exception
    {
        private readonly int _depth;

        /// <summary>
        /// Construct the execution with the depth of the queue.
        /// </summary>
        /// <param name="depth"></param>
        public QueueFullException(int depth)
            : base("Attempted to enqueue item into full queue: " + depth)
        {
            _depth = depth;
        }

        /// <summary>
        /// Construct with a custom message.
        /// </summary>
        /// <param name="msg"></param>
        public QueueFullException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Depth of queue.
        /// </summary>
        public int Depth
        {
            get { return _depth; }
        }
    }
}