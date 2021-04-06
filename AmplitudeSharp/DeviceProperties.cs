using System.Collections.Generic;
using System.Threading;
using AmplitudeSharp.Utils;
using Newtonsoft.Json;

namespace AmplitudeSharp
{
    public class DeviceProperties
    {
        [JsonProperty(propertyName:"device_id")]
        public string DeviceId { get; set; }

        [JsonProperty(propertyName: "platform")]
        public string Platform { get; set; }

        [JsonProperty(propertyName: "os_version")]
        public string OSVersion { get; set; }

        [JsonProperty(propertyName: "os_name")]
        public string OSName { get; set; }

        [JsonProperty(propertyName: "device_model")]
        public string DeviceModel { get; set; }

        [JsonProperty(propertyName: "device_manufacturer")]
        public string DeviceManufacturer { get; set; }

        [JsonProperty(propertyName: "language")]
        public string Language { get; set; }

        [JsonIgnore]
        public bool Is64BitDevice
        {
            get
            {
                return (bool)ExtraProperties.TryGet("64bit_device");
            }
            set
            {
                ExtraProperties["64bit_device"] = value;
            }
        }

        [JsonIgnore]
        public ulong RamMbs
        {
            get
            {
                return (ulong)ExtraProperties.TryGet("ram_mbs");
            }
            set
            {
                ExtraProperties["ram_mbs"] = value;
            }
        }

        [JsonIgnore]
        public string NetFrameworkVersion
        {
            get
            {
                return ExtraProperties.TryGet("netfx_version") as string;
            }
            set
            {
                ExtraProperties["netfx_version"] = value;
            }
        }

        [JsonIgnore]
        public Dictionary<string, object> ExtraProperties { get; private set; }

        public DeviceProperties()
        {
            DeviceHelper deviceHelper = new DeviceHelper();
            ExtraProperties = new Dictionary<string, object>();

            Platform = OSName = "Windows";
            OSVersion = deviceHelper.OSVersion;
            DeviceModel = deviceHelper.Model;
            DeviceManufacturer = deviceHelper.Manufacturer;
            // TODO(revive): Revive this once .NET Core 3.0 is released
//            NetFrameworkVersion = NetFxHelper.GetNetFxVersion().ToString();
            RamMbs = deviceHelper.RamMbs;
            Is64BitDevice = deviceHelper.Is64BitDevice;
            Language = Thread.CurrentThread.CurrentUICulture.EnglishName;
        }
    }
}
