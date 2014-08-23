using Retlang.Fibers;
using System;
using System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WpfExample;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WPTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WindowChannels channels = new WindowChannels();
        private IFiber fiber;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (fiber == null)
            {
                fiber = new DispatcherFiber(SynchronizationContext.Current);
                fiber.Start();
                channels.TimeUpdate.SubscribeToLast(fiber, OnTimeUpdate, 0);
                new UpdateController(channels);
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            channels.StartChannel.Publish(e);
        }

        private void OnTimeUpdate(DateTime time)
        {
            tbTime.Text = time.ToString();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
