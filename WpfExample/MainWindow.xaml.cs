using System.Windows;
using AmplitudeSharp;

namespace WpfExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Identify_Click(object sender, RoutedEventArgs e)
        {
            // Setup our user properties:
            UserProperties userProps = new UserProperties();
            userProps.UserId = "testuser_2";
            userProps.ExtraProperties.Add("email", "test2@example.com");
            // Note that userProps.AppVersion has been automatically inferred from
            // the version of the calling assembly, but can be overriden here, e.g:
            // userProps.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Setup our device properties:
            DeviceProperties devProps = new DeviceProperties();
            // It's a good idea to set a device ID, that way you can tell how many devices
            // a give user uses. Best way is to generate a device ID on first start and stash it
            // in the settings of the app
            devProps.DeviceId = "9818C32E-00E3-493D-837C-AD0C0D77748C";
            AmplitudeService.Instance.Identify(userProps, devProps);
        }

        private void LogSimgple_Click(object sender, RoutedEventArgs e)
        {
            AmplitudeService.Instance.Track("simple_event");
        }

        private void LogWithParams_Click(object sender, RoutedEventArgs e)
        {
            AmplitudeService.Instance.Track("event_with_properties", new { count = 4, category = "some value" });
        }

        private void NewSession_Click(object sender, RoutedEventArgs e)
        {
            AmplitudeService.Instance.NewSession();
        }
    }
}
