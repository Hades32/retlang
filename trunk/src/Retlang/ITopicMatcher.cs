using System;
using System.Collections.Generic;
using System.Text;

namespace Retlang
{
    public interface ITopicMatcher
    {
        bool Matches(object topic);
    }
}
