using System.Collections.Generic;
using AmplitudeSharp.Utils;
using Newtonsoft.Json;

namespace AmplitudeSharp.Api
{
    public class AmplitudeIdentify : Dictionary<string, object>, IAmplitudeEvent
    {
        [JsonIgnore]
        public string UserId
        {
            get
            {
                return (string)this.TryGet("user_id");
            }
        }

        [JsonIgnore]
        public string DeviceId
        {
            get
            {
                return (string)this.TryGet("device_id");
            }
        }

        [JsonIgnore]
        public Dictionary<string, object> UserProperties { get; set; }

        public AmplitudeIdentify()
        {
            UserProperties = new Dictionary<string, object>();
            this["user_properties"] = UserProperties;
        }
    }
}
