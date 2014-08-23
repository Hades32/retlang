using Retlang.Channels;
using System;
using Windows.UI.Xaml;

namespace WpfExample
{
    public class WindowChannels
    {
        public readonly IChannel<DateTime> TimeUpdate = new Channel<DateTime>();
        public readonly IChannel<RoutedEventArgs> StartChannel = new Channel<RoutedEventArgs>();
    }
}