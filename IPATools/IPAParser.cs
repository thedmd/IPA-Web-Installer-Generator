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
                if (entry.IsFile)
                {
                    string[] components = entry.Name.Split(new char[] { '\\', '/' });
                    if (components.Length > 1)
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

            Image bestIcon = null;
            foreach (string bundleIconFile in bundleInfo["CFBundleIconFiles"] as object[])
            {
                ZipEntry iconEntry = FindZipEntry(ipa, Path.Combine(bundleRoot, bundleIconFile));

                Image icon = null;
                using (MemoryStream buffer = new MemoryStream())
                {
                    using (Stream zipStream = ipa.GetInputStream(iconEntry))
                    using (Stream pngStream = Decrunch.Process(zipStream))
                        IPATools.Utilities.Utils.CopyStream(pngStream, buffer);

                    buffer.Position = 0;

                    icon = Image.FromStream(buffer);
                }

                if (null == icon)
                    continue;

                if (icon.Width == 57 && icon.Height == 57)
                    info.Icon57 = icon;
                if (icon.Width == 72 && icon.Height == 72)
                    info.Icon72 = icon;
                if (icon.Width == 512 && icon.Height == 512)
                    info.Icon512 = icon;

                if (null == bestIcon || (icon.Width >= bestIcon.Width && icon.Height >= bestIcon.Height))
                    bestIcon = icon;
            }

            info.RawIcon = bestIcon;

            if (bestIcon == null)
                bestIcon = Resources.Icon;

            if (null == info.Icon57)
                info.Icon57 = ResizeImage(bestIcon, new Size(57, 57));
            if (null == info.Icon72)
                info.Icon72 = ResizeImage(bestIcon, new Size(72, 72));
            if (null == info.Icon512)
                info.Icon512 = ResizeImage(bestIcon, new Size(512, 512));
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
