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
        public FormFiber(ISynchronizeInvoke invoker, IBatchAndSingleExecutor executor)
            : base(new FormAdapter(invoker), executor)
        {
        }
    }

    internal class FormAdapter : IThreadAdapter
    {
        private readonly ISynchronizeInvoke _invoker;

        public FormAdapter(ISynchronizeInvoke invoker)
        {
            _invoker = invoker;
        }

        public void Invoke(Action act)
        {
            _invoker.BeginInvoke(act, null);
        }
    }
}