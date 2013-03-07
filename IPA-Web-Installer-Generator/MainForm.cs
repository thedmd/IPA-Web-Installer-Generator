using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using IPA_Web_Installer_Generator.Properties;
using IPA_Web_Installer_Generator.Utilities;
using IPATools;

namespace IPA_Web_Installer_Generator
{
    public partial class MainForm : Form
    {
        private string Title = string.Empty;
        private IPAInfo Info = null;
        private bool Working = false;

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
                IPAInfo info = IPAParser.Parse(path);
                SelectIPA(info);
                return info != null;
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
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

            if (Working)
                enable = false;

            ControlInput(enable, true);

            Info = info;
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

            Working = true;

            GenerateProgressBar.Value = 0;
            GenerateProgressBar.Minimum = 0;
            GenerateProgressBar.Maximum = 100;
            GenerateProgressBar.Visible = true;

            IPAInstallerGenerator generator = new IPAInstallerGenerator(info, outputDir, GetHostUrl());
            generator.Async = true;
            generator.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
            generator.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            generator.Run();
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
            Working = false;
            ControlInput(true, false);

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
