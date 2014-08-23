using Retlang.Fibers;
using System;
using Windows.UI.Xaml;

namespace WpfExample
{
    public class UpdateController
    {
        private readonly IFiber fiber = new ThreadFiber();
        private IDisposable timer;
        private readonly WindowChannels channels;

        public UpdateController(WindowChannels winChannels)
        {
            channels = winChannels;
            channels.StartChannel.Subscribe(fiber, OnStart);
            fiber.Start();
        }

        private void OnStart(RoutedEventArgs msg)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
            else
            {
                timer = fiber.ScheduleOnInterval(OnTimer, 1000, 1000);
            }
        }

        private void OnTimer()
        {
            channels.TimeUpdate.Publish(DateTime.Now);
        }
    }
}