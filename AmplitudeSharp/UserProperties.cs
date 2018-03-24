using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace AmplitudeSharp
{
    public class UserProperties
    {
        [JsonProperty(propertyName: "user_id")]
        public string UserId { get; set; }

        [JsonProperty(propertyName: "app_version")]
        public string AppVersion { get; set; }

        [JsonIgnore]
        public Dictionary<string, object> ExtraProperties { get; private set; }

        public UserProperties()
        {
            ExtraProperties = new Dictionary<string, object>();

            // Infer app version (can be overriden later)
            AppVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();
        }
    }
}