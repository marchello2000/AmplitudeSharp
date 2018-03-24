using System.IO;
using System.Windows;
using AmplitudeSharp;

namespace WpfExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AmplitudeService analytics;
        private const string AmplitudeApiKey = "<YOUR_API_KEY>";

        protected override void OnStartup(StartupEventArgs e)
        {
            if (File.Exists("event.store"))
            {
                using (var s = File.OpenRead("event.store"))
                {
                    analytics = AmplitudeService.Initialize(AmplitudeApiKey, s);
                }
            }
            else
            {
                analytics = AmplitudeService.Initialize(AmplitudeApiKey);
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            using (var s = File.OpenWrite("event.store"))
            {
                analytics.Uninitialize(s);
            }

            analytics.Dispose();
        }
    }
}
