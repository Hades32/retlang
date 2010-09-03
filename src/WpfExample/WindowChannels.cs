using System;
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