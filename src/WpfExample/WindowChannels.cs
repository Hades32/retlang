using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Retlang.Channels;

namespace WpfExample
{
    public class WindowChannels
    {
        public readonly IChannel<DateTime> TimeUpdate = new Channel<DateTime>();
        public readonly IChannel<RoutedEventArgs> StartChannel = new Channel<RoutedEventArgs>();
    }
}
