using System;
using System.Collections.Generic;
using System.IO;
using IPATools.Properties;
using System.Text;

namespace IPATools
{
    public class IPAInstallerGenerator
    {
        public List<IPAInfo> InfoList
        {
            get { return m_InfoList; }
        }

        public string OutputDir
        {
            get { return m_OutputDir; }
            set { m_OutputDir = value; }
        }

        public string BaseUrl
        {
            get { return m_BaseUrl; }
            set { m_BaseUrl = value; }
        }

        public CustomFileCopyHandler CustomFileCopy
        {
            get { return m_CustomFileCopy; }
            set { m_CustomFileCopy = value; }
        }

        public bool SkipCopying
        {
            set;
            get;
        }

        public delegate void CustomFileCopyHandler(string source, string destination);

        public IPAInstallerGenerator()
        {
            SkipCopying = false;
        }

        public IPAInstallerGenerator(IPAInfo info, string outputDir, string baseUrl, CustomFileCopyHandler fileCopyHandler):
            this(new IPAInfo[] { info }, outputDir, baseUrl, fileCopyHandler)
        {
        }

        public IPAInstallerGenerator(IPAInfo[] infos, string outputDir, string baseUrl, CustomFileCopyHandler fileCopyHandler):
            this()
        {
            InfoList.Clear();
            InfoList.AddRange(infos);
            OutputDir = outputDir;
            BaseUrl = baseUrl;
            CustomFileCopy = fileCopyHandler;
        }

        public void Run()
        {
            if (m_Running)
                throw new InvalidOperationException("Generator is already running.");

            if (InfoList.Count == 0)
                throw new ArgumentNullException("InfoList");

            if (string.IsNullOrEmpty(OutputDir))
                throw new ArgumentNullException("OutputDir");

            if (string.IsNullOrEmpty(BaseUrl))
                throw new ArgumentNullException("BaseUrl");

            m_Running = true;

            DoGenerate();
        }

        private void SplitTemplate(out string header, out string footer, out string row, out string rowSeparator)
        {
            string rowStart = "<!-- ROW START -->";
            string rowEnd   = "<!-- ROW END -->";
            string rowSeparatorStart = "<!-- ROW SEPARATOR START -->";
            string rowSeparatorEnd = "<!-- ROW SEPARATOR END -->";

            var content = Resources.Install.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            List<string> headerLines = new List<string>();
            List<string> footerLines = new List<string>();
            List<string> rowLines = new List<string>();
            List<string> rowSeparatorLines = new List<string>();

            List<string> output = headerLines;
            foreach (var line in content)
            {
                if (line.Contains(rowStart))
                {
                    output = rowLines;
                    continue;
                }
                else if (line.Contains(rowEnd))
                {
                    output = footerLines;
                    continue;
                }
                else if (line.Contains(rowSeparatorStart))
                {
                    output = rowSeparatorLines;
                    continue;
                }
                else if (line.Contains(rowSeparatorEnd))
                {
                    output = footerLines;
                    continue;
                }

                output.Add(line);
            }

            header = string.Join("\n", headerLines.ToArray()) + "\n";
            footer = string.Join("\n", footerLines.ToArray()) + "\n";
            row = string.Join("\n", rowLines.ToArray()) + "\n";
            rowSeparator = string.Join("\n", rowSeparatorLines.ToArray()) + "\n";
        }

        private string ReplaceTokens(string template, Dictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> entry in values)
                template = template.Replace(entry.Key, entry.Value);

            return template;
        }

        private void DoGenerate()
        {
            string headerTemplate, footerTemplate, rowTemplate, rowSeparatorTemplate;
            SplitTemplate(out headerTemplate, out footerTemplate, out rowTemplate, out rowSeparatorTemplate);

            string commonDirName = InfoList.Count > 1 ? "common" : "content";
            string commonDirPath = Path.Combine(m_OutputDir, commonDirName);
            Directory.CreateDirectory(commonDirPath);

            string buttonPath = Path.Combine(commonDirName, "button.png");

            Resources.Button.Save(Path.Combine(m_OutputDir, buttonPath));

            Dictionary<string, string> commonDictionary = new Dictionary<string, string>();
            commonDictionary.Add("[[BUTTON-URL]]", GetAbsoluteUrl(buttonPath));
            commonDictionary.Add("[[BUNDLE-DISPLAY-NAME]]", "Ad-Hoc Installers");

            string installPath = "Install.html";
            StringBuilder installer = new StringBuilder();
            installer.Append(ReplaceTokens(headerTemplate, commonDictionary));

            foreach (var info in InfoList)
            {
                if (InfoList.IndexOf(info) > 0)
                    installer.Append(ReplaceTokens(rowSeparatorTemplate, commonDictionary));

                Dictionary<string, string> dictionary = new Dictionary<string, string>(commonDictionary);

                string ipaName = Path.GetFileNameWithoutExtension(info.BundlePath);
                string ipaFileName = Path.GetFileName(info.BundlePath);
                string infoFileName = ipaName + ".plist";

                string contentDirName = "content" + (InfoList.Count > 1 ? "_" + InfoList.IndexOf(info) : "");
                string contentDirPath = Path.Combine(m_OutputDir, contentDirName);
                Directory.CreateDirectory(m_OutputDir);
                Directory.CreateDirectory(contentDirPath);

                string icon57Path = Path.Combine(contentDirName, "Icon-57.png");
                string icon72Path = Path.Combine(contentDirName, "Icon-72.png");
                string icon256Path = Path.Combine(contentDirName, "Icon-256.png");
                string icon512Path = Path.Combine(contentDirName, "Icon-512.png");
                string ipaPath = Path.Combine(contentDirName, ipaFileName);
                string infoPath = Path.Combine(contentDirName, infoFileName);

                dictionary["[[BUNDLE-DISPLAY-NAME]]"] = info.BundleDisplayName;
                dictionary["[[BUNDLE-ID]]"] = info.BundleIdentifier;
                dictionary["[[BUNDLE-VERSION]]"] = info.BundleVersion;
                dictionary["[[BUILD-PLATFORM]]"] = info.DeviceFamily.ToString();
                dictionary["[[BUILD-DATE]]"] = info.BuildDate.ToString();
                dictionary["[[ICON-SMALL-URL]]"] = GetAbsoluteUrl(icon57Path);
                dictionary["[[ICON-URL]]"] = GetAbsoluteUrl(icon72Path);
                dictionary["[[ICON-LARGE-URL]]"] = GetAbsoluteUrl(icon256Path);
                dictionary["[[ICON-XLARGE-URL]]"] = GetAbsoluteUrl(icon512Path);
                dictionary["[[IPA-URL]]"] = GetAbsoluteUrl(ipaPath);
                dictionary["[[PLIST-URL]]"] = GetAbsoluteUrl(infoPath);

                info.Icon57.Icon.Save(Path.Combine(m_OutputDir, icon57Path));
                info.Icon72.Icon.Save(Path.Combine(m_OutputDir, icon72Path));
                info.Icon256.Icon.Save(Path.Combine(m_OutputDir, icon256Path));
                info.Icon512.Icon.Save(Path.Combine(m_OutputDir, icon512Path));

                string installRow = ReplaceTokens(rowTemplate, dictionary);
                string infoContent = ReplaceTokens(Resources.Template, dictionary);

                File.WriteAllText(Path.Combine(m_OutputDir, infoPath), infoContent);

                installer.Append(installRow);

                if (!SkipCopying)
                    CopyFile(info.BundlePath, Path.Combine(m_OutputDir, ipaPath));
            }

            installer.Append(ReplaceTokens(footerTemplate, commonDictionary));

            File.WriteAllText(Path.Combine(m_OutputDir, installPath), installer.ToString());

            // IPA Keys:
            // [[IPA-URL]] [[ICON-URL]] [[ICON-LARGE-URL]] [[BUNDLE-ID]] [[BUNDLE-VERSION]] [[BUNDLE-DISPLAY-NAME]] [[BUILD-PLATFORM]] [[BUILD-DATE]]

            // Install Keys:
            // [[BUNDLE-DISPLAY-NAME]] [[ICON-LARGE-URL]] [[PLIST-URL]] [[BUTTON-URL]]
        }

        private string GetAbsoluteUrl(string path)
        {
            UriBuilder builder = new UriBuilder(m_BaseUrl);
            builder.Path = Path.Combine(builder.Path, path).Replace(Path.DirectorySeparatorChar, '/');
            return builder.ToString();
        }

        void CopyFile(string source, string destination)
        {
            if (null != m_CustomFileCopy)
                m_CustomFileCopy(source, destination);
            else
                File.Copy(source, destination, true);
        }

        private List<IPAInfo> m_InfoList = new List<IPAInfo>();
        private string m_OutputDir = string.Empty;
        private string m_BaseUrl = string.Empty;
        private bool m_Running = false;
        private CustomFileCopyHandler m_CustomFileCopy = null;
    }
}
