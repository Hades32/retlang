using System;

namespace Retlang
{
    public class QueueFullException : Exception
    {
        private readonly int _depth;

        public QueueFullException(int depth)
            : base("Attempted to enqueue item into full queue: " + depth)
        {
            _depth = depth;
        }

        public int Depth
        {
            get { return _depth; }
        }
    }
}