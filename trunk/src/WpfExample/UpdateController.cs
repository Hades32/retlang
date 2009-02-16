using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Retlang.Fibers;
using System.Windows;
using Retlang.Core;

namespace WpfExample
{
    public class UpdateController
    {
        private readonly IFiber fiber = new ThreadFiber();
        private ITimerControl timer;
        private readonly WindowChannels channels;

        public UpdateController(WindowChannels winChannels)
        {
            channels = winChannels;
            channels.StartChannel.Subscribe(fiber, OnStart);
            fiber.Start();
        }

        private void OnStart(RoutedEventArgs obj)
        {
            if (timer != null)
            {
                timer.Cancel();
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
