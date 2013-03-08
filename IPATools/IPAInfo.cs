using System;
using System.Drawing;

namespace IPATools
{
    public enum DeviceFamily
    {
        Unknown,
        iPad,
        iPhone,
        Universal
    }

    public class IPAInfo
    {
        public DeviceFamily DeviceFamily = DeviceFamily.Unknown; // UIDeviceFamily
        public string BundlePath = string.Empty;
        public string BundleDisplayName = string.Empty;
        public string BundleName = string.Empty;
        public string BundleIdentifier = string.Empty;
        public string BundleVersion = string.Empty;
        public string MinimumOSVersion = string.Empty;
        public string PlatformName = string.Empty; // DTPlatformName
        public string PlatformVersion = string.Empty; // DTPlatformVersion
        public DateTime BuildDate = DateTime.MinValue;
        public Image Icon57 = null;
        public Image Icon72 = null;
        public Image Icon256 = null;
        public Image Icon512 = null;
        public Image RawIcon = null;
    }
}
