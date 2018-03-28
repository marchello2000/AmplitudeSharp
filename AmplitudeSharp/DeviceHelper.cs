using System;
using System.Collections.Generic;
using System.Management;
using AmplitudeSharp.Utils;

namespace AmplitudeSharp
{
    internal class DeviceHelper
    {
        private static Dictionary<string, string> WindowsVersions = new Dictionary<string, string>()
        {
            { "5.1", "Windows XP" },
            { "6.0", "Windows Vista" },
            { "6.1", "Windows 7" },
            { "6.2", "Windows 8" },
            { "6.3", "Windows 8.1" },
            { "10.0", "Windows 10" },
        };

        public ulong RamMbs { get; }
        public string Manufacturer { get; }
        public string Model { get; }
        public bool Is64BitDevice { get; }
        public string OSName { get; }
        public string OSVersion { get; }

        internal DeviceHelper()
        {
            try
            {
                var mc = new ManagementClass("Win32_ComputerSystem");
                foreach (ManagementObject mo in mc.GetInstances())
                {
                    Manufacturer = mo["Manufacturer"].ToString();
                    Model = mo["Model"].ToString();
                    break;
                }
            }
            catch (Exception ex)
            {
                AmplitudeService.s_logger(LogLevel.Warning, $"Failed to get device make/model: {ex.ToString()}");
            }

            try
            {
                var memStatus = new NativeMethods.MEMORYSTATUSEX();
                if (NativeMethods.GlobalMemoryStatusEx(memStatus))
                {
                    // Round to nearest 0.5GB
                    RamMbs = (ulong)Math.Round((memStatus.ullTotalPhys >> 20) / 512.0) * 512;
                }
            }
            catch (Exception ex)
            {
                AmplitudeService.s_logger(LogLevel.Warning, $"Failed to get device RAM size: {ex.ToString()}");
            }

            Is64BitDevice = Environment.Is64BitOperatingSystem;
            string majorMinor = Environment.OSVersion.Version.ToString(2);
            OSName = WindowsVersions.TryGet(majorMinor, "Windows");
            OSVersion = Environment.OSVersion.Version.ToString();
        }
    }
}
