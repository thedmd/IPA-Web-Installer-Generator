using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IPATools.Properties;
using System.ComponentModel;
using IPATools.Utilities;
using System.Runtime.Serialization.Plists;

namespace IPATools
{
    public class IPAInstallerGenerator
    {
        public IPAInfo Info
        {
            get { return m_Info; }
            private set { m_Info = value; }
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

        public bool Async
        {
            get;
            set;
        }

        public event ProgressChangedEventHandler ProgressChanged;
        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        public IPAInstallerGenerator()
        {
            Async = false;
        }

        public IPAInstallerGenerator(IPAInfo info, string outputDir, string baseUrl)
        {
            Async = false;
            Info = info;
            OutputDir = outputDir;
            BaseUrl = baseUrl;
        }

        public void Run()
        {
            if (m_Running)
                throw new InvalidOperationException("Generator is already running.");

            if (null == Info)
                throw new ArgumentNullException("Info");

            if (string.IsNullOrEmpty(OutputDir))
                throw new ArgumentNullException("OutputDir");

            if (string.IsNullOrEmpty(BaseUrl))
                throw new ArgumentNullException("BaseUrl");

            m_Running = true;

            DoGenerate();
        }

        private void DoGenerate()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            string ipaName = Path.GetFileNameWithoutExtension(m_Info.BundlePath);
            string ipaFileName = Path.GetFileName(m_Info.BundlePath);
            string infoFileName = ipaName + ".plist";

            string contentDirName = "content";
            string contentDirPath = Path.Combine(m_OutputDir, contentDirName);
            Directory.CreateDirectory(m_OutputDir);
            Directory.CreateDirectory(contentDirPath);

            string icon57Path = Path.Combine(contentDirName, "Icon-57.png");
            string icon72Path = Path.Combine(contentDirName, "Icon-72.png");
            string icon512Path = Path.Combine(contentDirName, "Icon-512.png");
            string installPath = "Install.html";
            string buttonPath = Path.Combine(contentDirName, "button.png");
            string ipaPath = Path.Combine(contentDirName, ipaFileName);
            string infoPath = Path.Combine(contentDirName, infoFileName);

            dictionary.Add("[[BUNDLE-DISPLAY-NAME]]", m_Info.BundleDisplayName);
            dictionary.Add("[[BUNDLE-ID]]", m_Info.BundleIdentifier);
            dictionary.Add("[[BUNDLE-VERSION]]", m_Info.BundleVersion);
            dictionary.Add("[[ICON-SMALL-URL]]", GetAbsoluteUrl(icon57Path));
            dictionary.Add("[[ICON-URL]]", GetAbsoluteUrl(icon72Path));
            dictionary.Add("[[ICON-LARGE-URL]]", GetAbsoluteUrl(icon512Path));
            dictionary.Add("[[IPA-URL]]", GetAbsoluteUrl(ipaPath));
            dictionary.Add("[[PLIST-URL]]", GetAbsoluteUrl(infoPath));
            dictionary.Add("[[BUTTON-URL]]", GetAbsoluteUrl(buttonPath));

            m_Info.Icon57.Save(Path.Combine(m_OutputDir, icon57Path));
            m_Info.Icon72.Save(Path.Combine(m_OutputDir, icon72Path));
            m_Info.Icon512.Save(Path.Combine(m_OutputDir, icon512Path));

            Resources.Button.Save(Path.Combine(m_OutputDir, buttonPath));

            string installContent = Resources.Install;
            string infoContent = Resources.Template;

            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                installContent = installContent.Replace(entry.Key, entry.Value);
                infoContent = infoContent.Replace(entry.Key, entry.Value);
            }

            File.WriteAllText(Path.Combine(m_OutputDir, infoPath), infoContent);
            File.WriteAllText(Path.Combine(m_OutputDir, installPath), installContent);

            CopyFile(m_Info.BundlePath, Path.Combine(m_OutputDir, ipaPath));

            // IPA Keys:
            // [[IPA-URL]] [[ICON-URL]] [[ICON-LARGE-URL]] [[BUNDLE-ID]] [[BUNDLE-VERSION]] [[BUNDLE-DISPLAY-NAME]]

            // Install Keys:
            // [[BUNDLE-DISPLAY-NAME]] [[ICON-SMALL-URL]] [[PLIST-URL]] [[BUTTON-URL]]
        }

        private string GetAbsoluteUrl(string path)
        {
            UriBuilder builder = new UriBuilder(m_BaseUrl);
            builder.Path = Path.Combine(builder.Path, path).Replace(Path.DirectorySeparatorChar, '/');
            return builder.ToString();
        }

        void CopyFile(string source, string destination)
        {
#if WIN32
            if (Async)
            {
                m_Worker = new BackgroundWorker();
                m_Worker.WorkerReportsProgress = true;
                m_Worker.WorkerSupportsCancellation = false;
                m_Worker.DoWork += new DoWorkEventHandler(XCopy_DoWork);
                m_Worker.ProgressChanged += new ProgressChangedEventHandler(XCopy_ProgressChanged);
                m_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(XCopy_RunWorkerCompleted);
                m_Worker.RunWorkerAsync(new string[] { source, destination });
            }
            else
#endif
            {
                XCopy_ProgressChanged(this, new ProgressChangedEventArgs(0, destination));
                File.Copy(source, destination);
                XCopy_ProgressChanged(this, new ProgressChangedEventArgs(100, destination));
                XCopy_RunWorkerCompleted(this, new RunWorkerCompletedEventArgs(destination, null, false));
            }
        }

#if WIN32
        void XCopy_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string[] files = e.Argument as string[];

            XCopy.Copy(files[0], files[1], true, true, (o, pce) =>
            {
                worker.ReportProgress(pce.ProgressPercentage, files[0]);
            });

            e.Result = files[1];
        }
#endif

        void XCopy_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (null != ProgressChanged)
                ProgressChanged(sender, e);
        }

        void XCopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_Running = false;

            if (null != RunWorkerCompleted)
                RunWorkerCompleted(sender, e);
        }

        private IPAInfo m_Info = null;
        private string m_OutputDir = string.Empty;
        private string m_BaseUrl = string.Empty;
        private bool m_Running = false;
        private BackgroundWorker m_Worker = null;
    }
}
