using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Plists;
using ICSharpCode.SharpZipLib.Zip;
using IPATools.Properties;
using IPATools.Utilities;
using System.Text;
using System.Text.RegularExpressions;

namespace IPATools
{
    public class IPAParser
    {
        public static IPAInfo Parse(string path)
        {
            ZipFile ipa = new ZipFile(path);

            IPAInfo info = new IPAInfo();
            info.BundlePath = path;

            string bundleRoot = string.Empty;
            foreach (ZipEntry entry in ipa)
            {
                if (entry.IsDirectory)
                {
                    string[] components = entry.Name.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (components.Length > 1 && components[0] == "Payload" && components[1].EndsWith(".app"))
                    {
                        bundleRoot = Path.Combine(components[0], components[1]);
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(bundleRoot))
                throw new Exception("IPA archive is empty or damaged.");

            IDictionary bundleInfo = null;
            ZipEntry infoPlistEntry = FindZipEntry(ipa, Path.Combine(bundleRoot, "Info.plist"));
            if (null == infoPlistEntry)
                throw new Exception("Failed to find Info.plist in IPA archive.");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Stream data = ipa.GetInputStream(infoPlistEntry))
                    IPATools.Utilities.Utils.CopyStream(data, memoryStream);

                memoryStream.Position = 0;

                BinaryPlistReader reader = new BinaryPlistReader();
                bundleInfo = reader.ReadObject(memoryStream);
            }

            info.BundleDisplayName = GetDictionaryEntry(bundleInfo, "CFBundleDisplayName");
            info.BundleName = GetDictionaryEntry(bundleInfo, "CFBundleName");
            info.BundleIdentifier = GetDictionaryEntry(bundleInfo, "CFBundleIdentifier");
            info.BundleVersion = GetDictionaryEntry(bundleInfo, "CFBundleVersion");
            info.MinimumOSVersion = GetDictionaryEntry(bundleInfo, "MinimumOSVersion");
            info.PlatformName = GetDictionaryEntry(bundleInfo, "DTPlatformName");
            info.PlatformVersion = GetDictionaryEntry(bundleInfo, "DTPlatformVersion");
            info.BuildDate = infoPlistEntry.DateTime;

            Dictionary<int, bool> deviceFamilies = new Dictionary<int, bool>();
            if (bundleInfo.Contains("UIDeviceFamily"))
                foreach (short deviceFamily in bundleInfo["UIDeviceFamily"] as object[])
                    deviceFamilies.Add(deviceFamily, true);

            if (deviceFamilies.ContainsKey(1) && deviceFamilies.ContainsKey(2))
                info.DeviceFamily = DeviceFamily.Universal;
            else if (deviceFamilies.ContainsKey(1))
                info.DeviceFamily = DeviceFamily.iPhone;
            else if (deviceFamilies.ContainsKey(2))
                info.DeviceFamily = DeviceFamily.iPad;
            else if (deviceFamilies.ContainsKey(3))
                info.DeviceFamily = DeviceFamily.AppleTV;

            List<string> iconNames = new List<string>();

            object[] bundleIconFiles = null;

            // Find device specific icon bundle
            IDictionary bundleIcons = null;
            if (info.DeviceFamily == DeviceFamily.iPad)
                bundleIcons = bundleInfo["CFBundleIcons~ipad"] as IDictionary;
            else if (info.DeviceFamily == DeviceFamily.iPhone)
                bundleIcons = bundleInfo["CFBundleIcons~iphone"] as IDictionary;
            if (null == bundleIcons)
                bundleIcons = bundleInfo["CFBundleIcons"] as IDictionary;
            if (null != bundleIcons)
            {
                IDictionary primaryIcon = bundleIcons["CFBundlePrimaryIcon"] as IDictionary;
                if (null != primaryIcon)
                    bundleIconFiles = primaryIcon["CFBundleIconFiles"] as object[];
            }

            if (null == bundleIconFiles)
                bundleIconFiles = bundleInfo["CFBundleIconFiles"] as object[];

            if (null != bundleIconFiles)
                foreach (string bundleIconFile in bundleIconFiles as object[])
                    iconNames.Add(Path.Combine(bundleRoot, bundleIconFile));

            List<IPAIcon> icons = new List<IPAIcon>();
            foreach (string iconName in iconNames)
            {
                var iconEntries = FindZipEntries(ipa, iconName);
                if (iconEntries == null || iconEntries.Count == 0)
                    continue;

                foreach (var iconEntry in iconEntries)
                {
                    Image iconImage = null;
                    using (MemoryStream buffer = new MemoryStream())
                    {
                        try
                        {
                            using (Stream zipStream = ipa.GetInputStream(iconEntry))
                            using (Stream pngStream = Decrunch.Process(zipStream))
                                IPATools.Utilities.Utils.CopyStream(pngStream, buffer);
                        }
                        catch (Exception)
                        {
                            buffer.Position = 0;
                            buffer.SetLength(0);
                        }

                        if (buffer.Position == 0)
                        {
                            try
                            {
                                using (Stream zipStream = ipa.GetInputStream(iconEntry))
                                    IPATools.Utilities.Utils.CopyStream(zipStream, buffer);
                            }
                            catch (Exception)
                            {
                                buffer.Position = 0;
                                buffer.SetLength(0);
                            }
                        }

                        buffer.Position = 0;

                        if (buffer.Length > 0)
                            iconImage = Image.FromStream(buffer);
                    }

                    if (null == iconImage)
                        continue;

                    var suffix = Path.GetFileNameWithoutExtension(iconEntry.Name).Substring(Path.GetFileNameWithoutExtension(iconName).Length);

                    IPAIcon icon = new IPAIcon(iconImage);
                    Match match = Regex.Match(suffix, @"(@(?<scale>\d+)x)?((?<ipad>~ipad)|(?<iphone>~iphone))?");
                    if (match.Success)
                    {
                        if (match.Groups["scale"].Success)
                            icon.Scale = int.Parse(match.Groups["scale"].Value);
                        if (match.Groups["ipad"].Success)
                            icon.Familiy = DeviceFamily.iPad;
                        if (match.Groups["iphone"].Success)
                            icon.Familiy = DeviceFamily.iPhone;
                    }

                    icons.Add(icon);
                }
            }

            info.Icons    = icons.ToArray();
            info.BestIcon = GetBestIcon(icons, info.DeviceFamily);

            info.Icon57 = GetIconOfSize(info, 57);
            info.Icon72 = GetIconOfSize(info, 72);
            info.Icon256 = GetIconOfSize(info, 256);
            info.Icon512 = GetIconOfSize(info, 512);

            ZipEntry infoPlistStrings = FindZipEntry(ipa, Path.Combine(bundleRoot, "InfoPlist.strings"));
            if (null == infoPlistStrings)
                infoPlistStrings = FindZipEntry(ipa, "*/en.lproj/InfoPlist.strings");
            if (null == infoPlistStrings)
                infoPlistStrings = FindZipEntry(ipa, "*.lproj/InfoPlist.strings");
            if (null != infoPlistStrings)
            {
                IDictionary strings = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Stream data = ipa.GetInputStream(infoPlistStrings))
                        IPATools.Utilities.Utils.CopyStream(data, memoryStream);

                    memoryStream.Position = 0;

                    try
                    {
                        UTF8Encoding encoder = new UTF8Encoding(false, true);
                        string entry = encoder.GetString(memoryStream.GetBuffer());
                        var match = Regex.Match(entry, @"CFBundleDisplayName\s*=\s*""([^""\\]*(?:\\.[^""\\]*)*)""");

                        if (match.Success)
                        {
                            Dictionary<string, string> result = new Dictionary<string, string>();
                            result.Add("CFBundleDisplayName", match.Groups[1].Value);
                            strings = result;
                        }
                        else
                            throw new Exception("Failed to parse InfoPlist.strings as plain text.");
                    }
                    catch (Exception)
                    {
                        BinaryPlistReader reader = new BinaryPlistReader();
                        strings = reader.ReadObject(memoryStream);
                    }
                }

                string displayName = GetDictionaryEntry(strings, "CFBundleDisplayName");
                if (!string.IsNullOrEmpty(displayName))
                    info.BundleDisplayName = displayName;
            }

            return info;
        }

        static ZipEntry FindZipEntry(ZipFile file, string name)
        {
            bool tail = name.StartsWith("*");

            name = name.Replace('\\', '/');
            if (tail)
                name = name.Substring(1);

            foreach (ZipEntry entry in file)
            {
                string entryName = entry.Name.Replace('\\', '/');

                if (tail && entryName.EndsWith(name))
                    return entry;
                else if (!tail && entryName.Equals(name))
                    return entry;
            }

            return null;
        }

        static List<ZipEntry> FindZipEntries(ZipFile file, string name)
        {
            name = name.Replace('\\', '/');

            string dir = Path.GetDirectoryName(name).Replace('\\', '/');
            if (!string.IsNullOrEmpty(dir))
                dir += '/';

            List<ZipEntry> result = new List<ZipEntry>();
            foreach (ZipEntry entry in file)
            {
                string entryName = entry.Name.Replace('\\', '/');

                if (!entryName.StartsWith(name))
                    continue;

                var shortPath = entryName.Substring(dir.Length);

                if (string.IsNullOrEmpty(Path.GetDirectoryName(shortPath)))
                    result.Add(entry);
            }

            return result;
        }

        // Search for best icon or return default one.
        static IPAIcon GetBestIcon(List<IPAIcon> icons, DeviceFamily deviceFamily)
        {
            IPAIcon bestIcon = null;
            int bestIconScore = 0;
            foreach (var icon in icons)
            {
                if (deviceFamily != DeviceFamily.Unknown && (deviceFamily != icon.Familiy))
                    continue;

                int iconScore = icon.Icon.Width * icon.Icon.Height * icon.Scale;

                if (bestIcon == null || iconScore > bestIconScore)
                {
                    bestIcon = icon;
                    bestIconScore = iconScore;
                }
            }

            if (null != bestIcon)
                return bestIcon;
            else if (deviceFamily != DeviceFamily.Unknown)
                return GetBestIcon(icons, DeviceFamily.Unknown);
            else if (icons.Count > 0)
                return icons[icons.Count - 1];
            else
                return new IPAIcon(Resources.Icon);
        }

        static IPAIcon GetIconOfSize(IPAInfo info, int size)
        {
            foreach (var icon in info.Icons)
                if (icon.Icon.Width == size)
                    return icon;

            Image resizedIcon = ResizeImage(info.BestIcon.Icon, new Size(size, size));

            return new IPAIcon(resizedIcon);
        }

        static string GetDictionaryEntry(IDictionary dict, string key)
        {
            string result = dict[key] as string;
            if (string.IsNullOrEmpty(result))
                result = string.Empty;
            return result;
        }

        static Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        private IPAParser()
        {
        }
    }
}
