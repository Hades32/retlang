using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface IObjectPublisher
    {
        void Publish(object topic, object msg, object replyToTopic);
        void Publish(object topic, object msg);
   
    }
}
