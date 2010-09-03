namespace Retlang.Core
{
    ///<summary>
    /// Allows for the registration and deregistration of subscriptions
    ///</summary>
    public interface ISubscriptions
    {
        ///<summary>
        /// Register unsubscriber to be called when the IFiber is disposed
        ///</summary>
        ///<param name="toAdd"></param>
        void Register(IUnsubscriber toAdd);

        ///<summary>
        /// Deregister a subscription
        ///</summary>
        ///<param name="toRemove"></param>
        ///<returns></returns>
        bool Deregister(IUnsubscriber toRemove);
    }
}