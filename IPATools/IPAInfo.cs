using System;
using System.Drawing;

namespace IPATools
{
    public enum DeviceFamily
    {
        Unknown,
        iPad,
        iPhone,
        Universal,
        AppleTV
    }

    public class IPAIcon
    {
        public Image Icon = null;
        public int Scale = 1;
        public DeviceFamily Familiy = DeviceFamily.Unknown;

        public IPAIcon()
        {
        }

        public IPAIcon(Image icon)
        {
            Icon = icon;
        }

        public IPAIcon(Image icon, int scale)
        {
            Icon = icon;
            Scale = scale;
        }
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
        public IPAIcon[] Icons = null;
        public IPAIcon Icon57 = null;
        public IPAIcon Icon72 = null;
        public IPAIcon Icon256 = null;
        public IPAIcon Icon512 = null;
        public IPAIcon BestIcon = null;
        public string[] ProvisionedDevices = null;
    }
}
