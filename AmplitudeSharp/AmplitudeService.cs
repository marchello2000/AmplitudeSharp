using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AmplitudeSharp.Api;
using AmplitudeSharp.Utils;
using Newtonsoft.Json;

namespace AmplitudeSharp
{
    public class AmplitudeService : IDisposable
    {
        public static AmplitudeService s_instance;
        public static AmplitudeService Instance
        {
            get
            {
                return s_instance;
            }
        }

        private object lockObject;
        private List<IAmplitudeEvent> eventQueue;
        private CancellationTokenSource cancellationToken;
        private Thread sendThread;
        private AmplitudeApi api;
        private AmplitudeIdentify identification;
        private SemaphoreSlim eventsReady;
        private long sessionId;

        /// <summary>
        /// Sets Offline mode, which means the events are never sent to actual amplitude service
        /// This is meant for testing
        /// </summary>
        public bool OfflineMode => api.OfflineMode;

        private AmplitudeService(string apiKey)
        {
            lockObject = new object();
            api = new AmplitudeApi(apiKey);
            eventQueue = new List<IAmplitudeEvent>();
            cancellationToken = new CancellationTokenSource();
            eventsReady = new SemaphoreSlim(0);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        public void Dispose()
        { 
            Uninitialize();
            s_instance = null;
        }

        /// <summary>
        /// Initialize AmplitudeSharp
        /// Takes an API key for the project and, optionally, 
        /// a stream where offline/past events are stored
        /// </summary>
        /// <param name="apiKey">api key for the project to stream data to</param>
        /// <param name="persistenceStream">optinal, stream with saved event data <seealso cref="Uninitialize(Stream)"/></param>
        /// <returns></returns>
        public static AmplitudeService Initialize(string apiKey, Stream persistenceStream = null)
        {
            if (apiKey == "<YOUR_API_KEY>")
            {
                throw new ArgumentOutOfRangeException(nameof(apiKey), "Please specify Amplitude API key");
            }

            AmplitudeService instance = new AmplitudeService(apiKey);
            instance.NewSession();

            if (Interlocked.CompareExchange(ref s_instance, instance, null) == null)
            {
                instance.StartSendThread();

                if (persistenceStream != null)
                {
                    instance.LoadPastEvents(persistenceStream);
                }
            }

            return Instance;
        }

        public void Uninitialize(Stream persistenceStore = null)
        {
            cancellationToken.Cancel();
            SaveEvents(persistenceStore);
        }

        /// <summary>
        /// Configure proxy settings.
        /// Many corporate users will be behind a proxy server <seealso cref="System.Net.HttpStatusCode.ProxyAuthenticationRequired"/>
        /// Supply the proxy credentials to use
        /// </summary>
        /// <param name="proxyUserName">proxy server username</param>
        /// <param name="proxyPassword">proxy server password</param>
        public void ConfigureProxy(string proxyUserName = null, string proxyPassword = null)
        {
            api.ConfigureProxy(proxyUserName, proxyPassword);
        }

        /// <summary>
        /// Set user and device identification parameters.
        /// </summary>
        public void Identify(UserProperties user, DeviceProperties device)
        {
            AmplitudeIdentify identify = new AmplitudeIdentify();

            foreach (var extraProps in new[] { user.ExtraProperties, device.ExtraProperties })
            {
                foreach (var value in extraProps)
                {
                    identify.UserProperties[value.Key] = value.Value;
                }
            }

            foreach (var obj in new object[] { user, device })
            {
                foreach (PropertyInfo property in obj.GetType().GetRuntimeProperties())
                {
                    object value = property.GetValue(obj);
                    if ((value != null) &&
                        (property.GetCustomAttribute<JsonIgnoreAttribute>() == null))
                    {
                        var jsonProp = property.GetCustomAttribute<JsonPropertyAttribute>();
                        string name = jsonProp?.PropertyName ?? property.Name;

                        identify[name] = value;
                    }
                }
            }

            identification = identify;
            QueueEvent(identify);
        }

        /// <summary>
        /// Begin new user session.
        /// Amplitude groups events into a single session if they have the same session_id
        /// The session_id is just the unix time stamp when the session began.
        /// Normally, you don't have to call it, but if you want to identify a specific new
        /// session (e.g. you are building a plugin an not an app)
        /// </summary>
        public void NewSession()
        {
            sessionId = DateTime.UtcNow.ToUnixEpoch();
        }

        /// <summary>
        /// Log an event without any parameters
        /// </summary>
        /// <param name="eventName">the name of the event</param>
        public void Track(string eventName)
        {
            Track(eventName, null);
        }

        /// <summary>
        /// Log an event with parameters
        /// </summary>
        /// <param name="eventName">the name of the event</param>
        /// <param name="properties">parameters for the event (this can just be a dynamic class)</param>
        public void Track(string eventName, object properties)
        {
            AmplitudeEvent e = new AmplitudeEvent(eventName, properties)
            {
                SessionId = sessionId
            };

            var identification = this.identification;
            e["user_id"] = identification.UserId;
            e["device_id"] = identification.DeviceId;

            QueueEvent(e);
        }

        private void QueueEvent(IAmplitudeEvent e)
        {
            lock (lockObject)
            {
                eventQueue.Add(e);
            }

            eventsReady.Release();
        }

        private void SaveEvents(Stream persistenceStore)
        {
            if (persistenceStore != null)
            {
                try
                {
                    string persistedData = JsonConvert.SerializeObject(eventQueue, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
                    using (var writer = new StreamWriter(persistenceStore))
                    {
                        writer.Write(persistedData);
                    }
                }
                catch
                {
                }
            }
        }

        private void LoadPastEvents(Stream persistenceStore)
        {
            try
            {
                using (var reader = new StreamReader(persistenceStore))
                {
                    string persistedData = reader.ReadLine();
                    var data = JsonConvert.DeserializeObject<List<IAmplitudeEvent>>(persistedData, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });

                    eventQueue.InsertRange(0, data);
                    eventsReady.Release();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Configure and kick-off the background upload thread
        /// </summary>
        private void StartSendThread()
        {
            sendThread = new Thread(UploadThread);
            sendThread.Name = $"{nameof(AmplitudeSharp)} Upload Thread";
            sendThread.Priority = ThreadPriority.BelowNormal;
            sendThread.Start();
        }

        /// <summary>
        /// The background thread for uploading events
        /// </summary>
        private async void UploadThread()
        {
            const int MaxEventBatch = 10;

            try
            {
                while (true)
                {
                    await eventsReady.WaitAsync(cancellationToken.Token);
                    List<AmplitudeEvent> eventsToSend = new List<AmplitudeEvent>();
                    AmplitudeIdentify identification = null;
                    bool backOff = false;

                    lock (lockObject)
                    {
                        foreach (IAmplitudeEvent e in eventQueue)
                        {
                            identification = e as AmplitudeIdentify;

                            if ((identification != null) || (eventsToSend.Count >= MaxEventBatch))
                            {
                                break;
                            }

                            eventsToSend.Add((AmplitudeEvent)e);
                        }
                    }

                    if (eventsToSend.Count > 0)
                    {
                        AmplitudeApi.SendResult result = await api.SendEvents(eventsToSend);

                        if (result == AmplitudeApi.SendResult.Success)
                        {
                            // Remove these events from the list:
                            lock (lockObject)
                            {
                                eventQueue.RemoveRange(0, eventsToSend.Count);
                            }
                        }
                        else
                        {
                            // If we failed to send events don't sent the identification:
                            identification = null;
                            backOff = true;
                        }
                    }

                    if (identification != null)
                    {
                        AmplitudeApi.SendResult result = await api.Identify(identification);

                        if (result == AmplitudeApi.SendResult.Success)
                        {
                            // Remove this identify call from the list
                            lock (lockObject)
                            {
                                eventQueue.RemoveRange(0, 1);
                            }
                        }
                        else
                        {
                            backOff = true;
                        }
                    }

                    if (backOff)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken.Token);
                    }
                }
            }
            catch
            {
                // No matter what exception happens, we just quit
            }
        }
    }
}
