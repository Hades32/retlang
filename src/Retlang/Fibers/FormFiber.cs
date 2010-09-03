using System;
using System.ComponentModel;
using Retlang.Core;

namespace Retlang.Fibers
{
    ///<summary>
    /// Allows interaction with Windows Forms.  Transparently moves actions onto the Form's thread.
    ///</summary>
    public class FormFiber : BaseFiber
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        public FormFiber(ISynchronizeInvoke invoker, IExecutor executor)
            : base(new FormAdapter(invoker), executor)
        {
        }
    }

    internal class FormAdapter : IContext
    {
        private readonly ISynchronizeInvoke _invoker;

        public FormAdapter(ISynchronizeInvoke invoker)
        {
            _invoker = invoker;
        }

        public void Enqueue(Action action)
        {
            _invoker.BeginInvoke(action, null);
        }
    }
}