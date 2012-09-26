using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections;
using System.Runtime.Serialization.Plists;
using System.IO;
using IPA_Web_Installer_Generator.Utilities;
using System.Drawing.Drawing2D;
using IPA_Web_Installer_Generator.Properties;
using System.Diagnostics;
using System.Security.Policy;

namespace IPA_Web_Installer_Generator
{
    public partial class MainForm : Form
    {
        enum DeviceFamily
        {
            Unknown,
            iPad,
            iPhone,
            Universal
        }

        class IPAInfo
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
            public Image Icon512 = null;
        }

        private string Title = string.Empty;
        private IPAInfo Info = null;
        private BackgroundWorker Worker = null;

        public MainForm()
        {
            InitializeComponent();

            Title = Text;

            HostName.Text = Settings.Default.HostUrl;
            BuildDir.Text = Settings.Default.BuildDir;
            if (string.IsNullOrEmpty(BuildDir.Text))
                BuildDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            CreateExtraDirCheckbox.Checked = Settings.Default.DedicatedBuildDir;

            if (!string.IsNullOrEmpty(Settings.Default.IPAPath) && File.Exists(Settings.Default.IPAPath))
                LoadIPA(Settings.Default.IPAPath);

            //const string ipaPath = @"C:\Users\thedmd\Desktop\G5_Ship_GameiPad-resigned.ipa";
            //const string ipaPath = @"d:\Projects\Sandbox\IPA-Web-Installer-Generator\Resources\Enigmatis_TheGhostsOfMapleCreek_iOS_20120925_0324.ipa";

            //IPAInfo info = GetIPAInfo(ipaPath);
            //SelectIPA(info);

        }

        private bool LoadIPA(string path)
        {
            try
            {
                IPAInfo info = GetIPAInfo(path);
                SelectIPA(info);
                return info != null;
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private IPAInfo GetIPAInfo(string path)
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
                    data.CopyTo(memoryStream);

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
                        pngStream.CopyTo(buffer);

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

            if (bestIcon == null)
                bestIcon = Resources.Icon;

            if (null == info.Icon57)
                info.Icon57 = ResizeImage(bestIcon, new Size(57, 57));
            if (null == info.Icon72)
                info.Icon72 = ResizeImage(bestIcon, new Size(72, 72));
            if (null == info.Icon512)
                info.Icon512 = ResizeImage(bestIcon, new Size(512, 512));

            ZipEntry infoPlistStrings = FindZipEntry(ipa, Path.Combine(bundleRoot, "InfoPlist.strings"));
            if (null != infoPlistStrings)
            {
                IDictionary strings = null;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Stream data = ipa.GetInputStream(infoPlistStrings))
                        data.CopyTo(memoryStream);

                    memoryStream.Position = 0;

                    BinaryPlistReader reader = new BinaryPlistReader();
                    strings = reader.ReadObject(memoryStream);
                }

                string displayName = GetDictionaryEntry(strings, "CFBundleDisplayName");
                if (!string.IsNullOrEmpty(displayName))
                    info.BundleDisplayName = displayName;
            }

            return info;
        }

        void SelectIPA(IPAInfo info)
        {
            bool enable = false;

            if (null != info)
            {
                IPADisplayName.Text = info.BundleDisplayName;
                IPAName.Text = info.BundleName;
                IPAPath.Text = info.BundlePath;
                IPAIdentifier.Text = info.BundleIdentifier;
                IPAVersion.Text = info.BundleVersion;
                IPAMinOS.Text = info.MinimumOSVersion;
                IPADate.Text = info.BuildDate.ToString();
                IPAIcon.Image = info.Icon72;

                Text = Title + " - " + info.BundleDisplayName;

                Settings.Default.IPAPath = info.BundlePath;
                Settings.Default.Save();

                enable = true;
            }
            else
            {
                IPADisplayName.Text = string.Empty;
                IPAName.Text = string.Empty;
                IPAPath.Text = string.Empty;
                IPAIdentifier.Text = string.Empty;
                IPAVersion.Text = string.Empty;
                IPAMinOS.Text = string.Empty;
                IPADate.Text = string.Empty;
                IPAIcon.Image = null;

                Text = Title;

                Settings.Default.IPAPath = string.Empty;
                Settings.Default.Save();
            }

            if (IPAIcon.Image == null)
                IPAIcon.Image = Resources.Icon;

            if (null != Worker)
                enable = false;

            ControlInput(enable, true);

            Info = info;
        }

        ZipEntry FindZipEntry(ZipFile file, string name)
        {
            name = name.Replace('\\', '/');

            foreach (ZipEntry entry in file)
            {
                string entryName = entry.Name.Replace('\\', '/');

                if (entryName.Equals(name))
                    return entry;
            }

            return null;
        }

        string GetDictionaryEntry(IDictionary dict, string key)
        {
            string result = dict[key] as string;
            if (string.IsNullOrEmpty(result))
                result = string.Empty;
            return result;
        }

        Image ResizeImage(Image image, Size size, bool preserveAspectRatio = true)
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

        string GetBuildDir()
        {
            string result = BuildDir.Text;
            if (CreateExtraDirCheckbox.Checked)
                result = Path.Combine(result, Path.GetFileNameWithoutExtension(Info.BundlePath));
            return result;
        }

        string GetHostUrl()
        {
            UriBuilder builder = new UriBuilder(HostName.Text);
            if (CreateExtraDirCheckbox.Checked)
                builder.Path = Path.Combine(builder.Path, Path.GetFileNameWithoutExtension(Info.BundlePath)).Replace(Path.DirectorySeparatorChar, '/');

            return builder.ToString();
        }

        private string GetAbsoluteUrl(string path)
        {
            UriBuilder builder = new UriBuilder(GetHostUrl());
            builder.Path = Path.Combine(builder.Path, path).Replace(Path.DirectorySeparatorChar, '/');
            return builder.ToString();
        }

        private void ControlInput(bool enable, bool buttonsOnly)
        {
            if (!buttonsOnly)
            {
                BuildDirBrowse.Enabled = enable;
                CreateExtraDirCheckbox.Enabled = enable;
                HostName.Enabled = enable;
                IPABrowse.Enabled = enable;
                BuildDir.Enabled = enable;
            }
            
            ShowOutputButton.Enabled = enable;
            OpenInBrowserButton.Enabled = enable;
            GenerateButton.Enabled = enable;
        }

        private void GenerateInstaller(IPAInfo info, string outputDir)
        {
            ControlInput(false, false);

            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string ipaName = Path.GetFileNameWithoutExtension(info.BundlePath);
            string ipaFileName = Path.GetFileName(info.BundlePath);
            string infoFileName = ipaName + ".plist";

            string contentDirName = "content";
            string contentDirPath = Path.Combine(outputDir, contentDirName);
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(contentDirPath);

            string icon57Path = Path.Combine(contentDirName, "Icon-57.png");
            string icon72Path = Path.Combine(contentDirName, "Icon-72.png");
            string icon512Path = Path.Combine(contentDirName, "Icon-512.png");
            string installPath = "Install.html";
            string buttonPath = Path.Combine(contentDirName, "button.png");
            string ipaPath = Path.Combine(contentDirName, ipaFileName);
            string infoPath = Path.Combine(contentDirName, infoFileName);

            dictionary.Add("[[BUNDLE-DISPLAY-NAME]]", info.BundleDisplayName);
            dictionary.Add("[[BUNDLE-ID]]", info.BundleIdentifier);
            dictionary.Add("[[BUNDLE-VERSION]]", info.BundleVersion);
            dictionary.Add("[[ICON-SMALL-URL]]", GetAbsoluteUrl(icon57Path));
            dictionary.Add("[[ICON-URL]]", GetAbsoluteUrl(icon72Path));
            dictionary.Add("[[ICON-LARGE-URL]]", GetAbsoluteUrl(icon512Path));
            dictionary.Add("[[IPA-URL]]", GetAbsoluteUrl(ipaPath));
            dictionary.Add("[[PLIST-URL]]", GetAbsoluteUrl(infoPath));
            dictionary.Add("[[BUTTON-URL]]", GetAbsoluteUrl(buttonPath));

            info.Icon57.Save(Path.Combine(outputDir, icon57Path));
            info.Icon72.Save(Path.Combine(outputDir, icon72Path));
            info.Icon512.Save(Path.Combine(outputDir, icon512Path));

            Resources.Button.Save(Path.Combine(outputDir, buttonPath));

            string installContent = Resources.Install;
            string infoContent = Resources.Template;

            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                installContent = installContent.Replace(entry.Key, entry.Value);
                infoContent = infoContent.Replace(entry.Key, entry.Value);
            }

            File.WriteAllText(Path.Combine(outputDir, infoPath), infoContent);
            File.WriteAllText(Path.Combine(outputDir, installPath), installContent);

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = false;
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            Worker.RunWorkerAsync(new string[] { info.BundlePath, Path.Combine(outputDir, ipaPath) });

            GenerateProgressBar.Value = 0;
            GenerateProgressBar.Minimum = 0;
            GenerateProgressBar.Maximum = 100;
            GenerateProgressBar.Visible = true;

            // IPA Keys:
            // [[IPA-URL]] [[ICON-URL]] [[ICON-LARGE-URL]] [[BUNDLE-ID]] [[BUNDLE-VERSION]] [[BUNDLE-DISPLAY-NAME]]

            // Install Keys:
            // [[BUNDLE-DISPLAY-NAME]] [[ICON-SMALL-URL]] [[PLIST-URL]] [[BUTTON-URL]]
        }

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string[] files = e.Argument as string[];

            XCopy.Copy(files[0], files[1], true, true, (o, pce) =>
            {
                worker.ReportProgress(pce.ProgressPercentage, files[0]);
            });

            e.Result = files[1];
        }

        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GenerateProgressBar.Invoke(new MethodInvoker(delegate()
            {
                int diffValue = e.ProgressPercentage - 1;
                if (diffValue < 0)
                    diffValue = 2;

                GenerateProgressBar.Value = e.ProgressPercentage;
                GenerateProgressBar.Value = diffValue;
                GenerateProgressBar.Value = e.ProgressPercentage;
            }));
        }

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GenerateProgressBar.Value = GenerateProgressBar.Maximum;
            GenerateProgressBar.Visible = false;
            ControlInput(true, false);
            Worker = null;

            string path = Path.GetDirectoryName(Path.GetDirectoryName(e.Result as string));

            MessageBoxEx.Show(this, "Installer was generated at: " + path, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void IPABrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = IPASelectDialog.ShowDialog(this);
            if (result == DialogResult.OK)
                LoadIPA(IPASelectDialog.FileName);
        }

        void IPAPath_Paint(object sender, PaintEventArgs e)
        {
            Label label = (Label)sender;
            using (SolidBrush b = new SolidBrush(label.BackColor))
                e.Graphics.FillRectangle(b, label.ClientRectangle);
            TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, label.ForeColor,
                TextFormatFlags.PathEllipsis);
        }

        void HostName_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.HostUrl = (sender as ComboBox).Text;
            Settings.Default.Save();
        }

        void OutputDir_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.BuildDir = (sender as ComboBox).Text;
            Settings.Default.Save();
        }

        void CreateExtraDirCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.DedicatedBuildDir = (sender as CheckBox).Checked;
            Settings.Default.Save();
        }

        void BuildDirBrowse_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(BuildDir.Text))
                BuildDirDialog.SelectedPath = BuildDir.Text;

            DialogResult result = BuildDirDialog.ShowDialog(this);
            if (result == DialogResult.OK)
                BuildDir.Text = BuildDirDialog.SelectedPath;
        }

        void ShowOutputButton_Click(object sender, EventArgs e)
        {
            try
            {
                string outputDir = GetBuildDir();

                if (!Directory.Exists(outputDir))
                    throw new Exception("Output directory doesn't not exists.");

                Process.Start(outputDir);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenInBrowserButton_Click(object sender, EventArgs e)
        {
            try
            {
                string hostUrl = GetAbsoluteUrl("Install.html");

                Process.Start(hostUrl);
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }            
        }

        void GenerateButton_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateInstaller(Info, GetBuildDir());
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }   
        }
    }
}
