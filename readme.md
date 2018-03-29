# AmplitudeSharp
A simple to use Amplitude analytics logging library for C#

[![Build status](https://ci.appveyor.com/api/projects/status/4qsr9ida4dmy9fji?svg=true)](https://ci.appveyor.com/project/marchello2000/amplitudesharp) [![NuGet](https://img.shields.io/nuget/v/AmplitudeSharp.svg)](https://www.nuget.org/packages/AmplitudeSharp/)



**NOTE:** this library is early development stages, many changes are planned (including better documentation) use at your own risk

## Features
* Simple setup
* Native Amplitude API (instead of going through some other service, such as segment.io)
* Automatic identification of hardware
* Support for offline scenarios (events are queued and stored offline to be sent later)

## Setup
1. If you don't have an Amplitude account already, head over to [Amplitude](https://amplitude.com/signup) and create an account.
2. Once created, note your project API key, you can find this information in [Settings > Projects](https://analytics.amplitude.com/settings/projects) section, note each project will have a different API key. I also recommend that you have `dev` and `prod` environments/projects.
3. Install the [nuget package](https://www.nuget.org/packages/AmplitudeSharp/)
4. Initialize the library on app start and uninitialize the library when the app quits (here is a WPF app example):

```cs
using AmplitudeSharp;

public partial class App : Application
{
    private AmplitudeService analytics;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Only call this once per lifetime of the object
        analytics = AmplitudeService.Initialize("<YOUR API KEY>");

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        // Terminate the analytics upload thread
        analytics.Dispose();
    }
}
```

5. Identify your user:
```cs
// Setup our user properties:
UserProperties userProps = new UserProperties();
userProps.UserId = "testuser_2";
userProps.ExtraProperties.Add("email", "test2@example.com");
// Note that userProps.AppVersion is automatically inferred from the
// version of the calling assembly, but can be overriden here, e.g:
// userProps.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

// Setup our device properties:
DeviceProperties devProps = new DeviceProperties();
// It's a good idea to set a device ID, that way you can tell how many devices
// a give user uses. Best way is to generate a device ID on first start and stash it
// in the settings of the app
devProps.DeviceId = "<SOME PERSISTED GUID>";
AmplitudeService.Instance.Identify(userProps, devProps);
```

6. Track events  
There are two types of events you can track: with and without parameters. Events without parameters require only a name to be logged, like so:  
`AmplitudeService.Instance.Track("simple_event");`  
Events with parameters can be logged as follows:  
`AmplitudeService.Instance.Track("event_with_properties", new { count = 4, category = "some value" });`

7. Session management  
A session is automatically created when the library is initialized, however you may want to create a new session without restarting the app. Let's say you want to create a new session when a user opens a new project/file/etc in your application, to do that, simple call:  
`AmplitudeService.Instance.NewSession();`  
This will allow you track session length on Amplitude and measure engagement of your users over time
