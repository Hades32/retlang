using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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

    class FormAdapter: IThreadAdapter
    {
        private readonly ISynchronizeInvoke target;

        public FormAdapter(ISynchronizeInvoke invoker)
        {
            this.target = invoker;
        }

        public void Invoke(Action act)
        {
            target.BeginInvoke(act, null);
        }
    }
}