namespace Retlang.Core
{
    /// <summary>
    /// Controller to cancel event timer.
    /// </summary>
    public interface ITimerControl
    {
        /// <summary>
        /// Cancels scheduled timer.
        /// </summary>
        void Cancel();
    }
}
