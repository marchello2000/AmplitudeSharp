using System;
// TODO(revive): Revive this once .NET Core 3.0 is released
//using Microsoft.Win32;

namespace AmplitudeSharp
{
    class NetFxHelper
    {
        // TODO(revive): Revive this once .NET Core 3.0 is released
        /// <summary>
        /// Get the installed .net version as recommended here:
        /// https://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        /// </summary>
//        public static Version GetNetFxVersion()
//        {
//            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
//
//            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
//            {
//                if (ndpKey != null && ndpKey.GetValue("Release") != null)
//                {
//                    Version installedVersion = GetNet45Version((int)ndpKey.GetValue("Release"));
//
//                    return installedVersion;
//                }
//            }
//
//            // If we hit this we have .NET framework version < 4.5, return 4.0 for now
//            return new Version("4.0");
//        }

        // Checking the version using >= will enable forward compatibility.
        private static Version GetNet45Version(int releaseKey)
        {
            if (releaseKey >= 461308)
            {
                // "4.7.1 or later";
                return new Version("4.7.1");
            }
            if (releaseKey >= 460798)
            {
                return new Version(4, 7);
            }
            if (releaseKey >= 394802)
            {
                return new Version(4, 6, 2);
            }
            if (releaseKey >= 394254)
            {
                return new Version(4, 6, 1);
            }
            if (releaseKey >= 393295)
            {
                return new Version(4, 6);
            }
            if ((releaseKey >= 379893))
            {
                return new Version(4, 5, 2);
            }
            if ((releaseKey >= 378675))
            {
                return new Version(4, 5, 2);
            }
            if ((releaseKey >= 378389))
            {
                return new Version(4, 5);
            }

            // This code should never execute because for now this assembly targets 4.5 or higher.
            // But return 4.0 for now
            return new Version(4, 0);
        }
    }
}
