namespace IPA_Web_Installer_Generator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.IPABrowse = new System.Windows.Forms.Button();
            this.IPADisplayName = new System.Windows.Forms.Label();
            this.IPAPath = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.IPASelectDialog = new System.Windows.Forms.OpenFileDialog();
            this.IPAIcon = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.IPAIdentifier = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.IPAVersion = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.IPAMinOS = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.IPADate = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.IPAName = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.HostName = new System.Windows.Forms.ComboBox();
            this.BuildDir = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.BuildDirBrowse = new System.Windows.Forms.Button();
            this.BuildDirDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.ShowOutputButton = new System.Windows.Forms.Button();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.OpenInBrowserButton = new System.Windows.Forms.Button();
            this.CreateExtraDirCheckbox = new System.Windows.Forms.CheckBox();
            this.GenerateProgressBar = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.IPAIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // IPABrowse
            // 
            this.IPABrowse.Location = new System.Drawing.Point(12, 12);
            this.IPABrowse.Name = "IPABrowse";
            this.IPABrowse.Size = new System.Drawing.Size(75, 41);
            this.IPABrowse.TabIndex = 0;
            this.IPABrowse.Text = "Browse...";
            this.IPABrowse.UseVisualStyleBackColor = true;
            this.IPABrowse.Click += new System.EventHandler(this.IPABrowse_Click);
            // 
            // IPADisplayName
            // 
            this.IPADisplayName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPADisplayName.AutoEllipsis = true;
            this.IPADisplayName.Location = new System.Drawing.Point(182, 17);
            this.IPADisplayName.Name = "IPADisplayName";
            this.IPADisplayName.Size = new System.Drawing.Size(244, 18);
            this.IPADisplayName.TabIndex = 1;
            // 
            // IPAPath
            // 
            this.IPAPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAPath.AutoEllipsis = true;
            this.IPAPath.Location = new System.Drawing.Point(182, 53);
            this.IPAPath.Name = "IPAPath";
            this.IPAPath.Size = new System.Drawing.Size(244, 18);
            this.IPAPath.TabIndex = 2;
            this.IPAPath.Paint += new System.Windows.Forms.PaintEventHandler(this.IPAPath_Paint);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(93, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "Display Name:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(93, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Path:";
            // 
            // IPASelectDialog
            // 
            this.IPASelectDialog.DefaultExt = "ipa";
            this.IPASelectDialog.Filter = "IPA Files|*.ipa|All files|*.*";
            this.IPASelectDialog.Title = "Choose IPA File";
            // 
            // IPAIcon
            // 
            this.IPAIcon.Image = global::IPA_Web_Installer_Generator.Properties.Resources.Icon;
            this.IPAIcon.Location = new System.Drawing.Point(12, 59);
            this.IPAIcon.Name = "IPAIcon";
            this.IPAIcon.Size = new System.Drawing.Size(75, 75);
            this.IPAIcon.TabIndex = 5;
            this.IPAIcon.TabStop = false;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(93, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 18);
            this.label4.TabIndex = 7;
            this.label4.Text = "Identifier:";
            // 
            // IPAIdentifier
            // 
            this.IPAIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAIdentifier.AutoEllipsis = true;
            this.IPAIdentifier.Location = new System.Drawing.Point(182, 71);
            this.IPAIdentifier.Name = "IPAIdentifier";
            this.IPAIdentifier.Size = new System.Drawing.Size(244, 18);
            this.IPAIdentifier.TabIndex = 6;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(93, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 18);
            this.label6.TabIndex = 9;
            this.label6.Text = "Version:";
            // 
            // IPAVersion
            // 
            this.IPAVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAVersion.AutoEllipsis = true;
            this.IPAVersion.Location = new System.Drawing.Point(182, 89);
            this.IPAVersion.Name = "IPAVersion";
            this.IPAVersion.Size = new System.Drawing.Size(244, 18);
            this.IPAVersion.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(93, 107);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 18);
            this.label8.TabIndex = 11;
            this.label8.Text = "Minimum OS:";
            // 
            // IPAMinOS
            // 
            this.IPAMinOS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAMinOS.AutoEllipsis = true;
            this.IPAMinOS.Location = new System.Drawing.Point(182, 107);
            this.IPAMinOS.Name = "IPAMinOS";
            this.IPAMinOS.Size = new System.Drawing.Size(244, 18);
            this.IPAMinOS.TabIndex = 10;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(93, 125);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(83, 18);
            this.label10.TabIndex = 13;
            this.label10.Text = "Date:";
            // 
            // IPADate
            // 
            this.IPADate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPADate.AutoEllipsis = true;
            this.IPADate.Location = new System.Drawing.Point(182, 125);
            this.IPADate.Name = "IPADate";
            this.IPADate.Size = new System.Drawing.Size(244, 18);
            this.IPADate.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(93, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 18);
            this.label1.TabIndex = 15;
            this.label1.Text = "Name:";
            // 
            // IPAName
            // 
            this.IPAName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IPAName.AutoEllipsis = true;
            this.IPAName.Location = new System.Drawing.Point(182, 35);
            this.IPAName.Name = "IPAName";
            this.IPAName.Size = new System.Drawing.Size(244, 18);
            this.IPAName.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Host";
            // 
            // HostName
            // 
            this.HostName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HostName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.HostName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
            this.HostName.FormattingEnabled = true;
            this.HostName.Location = new System.Drawing.Point(12, 164);
            this.HostName.Name = "HostName";
            this.HostName.Size = new System.Drawing.Size(413, 21);
            this.HostName.TabIndex = 17;
            this.HostName.TextChanged += new System.EventHandler(this.HostName_TextChanged);
            // 
            // BuildDir
            // 
            this.BuildDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildDir.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.BuildDir.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.BuildDir.FormattingEnabled = true;
            this.BuildDir.Location = new System.Drawing.Point(12, 204);
            this.BuildDir.Name = "BuildDir";
            this.BuildDir.Size = new System.Drawing.Size(372, 21);
            this.BuildDir.TabIndex = 19;
            this.BuildDir.TextChanged += new System.EventHandler(this.OutputDir_TextChanged);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 188);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Output Directory";
            // 
            // BuildDirBrowse
            // 
            this.BuildDirBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BuildDirBrowse.Location = new System.Drawing.Point(390, 204);
            this.BuildDirBrowse.Name = "BuildDirBrowse";
            this.BuildDirBrowse.Size = new System.Drawing.Size(35, 21);
            this.BuildDirBrowse.TabIndex = 20;
            this.BuildDirBrowse.Text = "...";
            this.BuildDirBrowse.UseVisualStyleBackColor = true;
            this.BuildDirBrowse.Click += new System.EventHandler(this.BuildDirBrowse_Click);
            // 
            // ShowOutputButton
            // 
            this.ShowOutputButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ShowOutputButton.Location = new System.Drawing.Point(12, 259);
            this.ShowOutputButton.Name = "ShowOutputButton";
            this.ShowOutputButton.Size = new System.Drawing.Size(114, 41);
            this.ShowOutputButton.TabIndex = 21;
            this.ShowOutputButton.Text = "Show Output";
            this.ShowOutputButton.UseVisualStyleBackColor = true;
            this.ShowOutputButton.Click += new System.EventHandler(this.ShowOutputButton_Click);
            // 
            // GenerateButton
            // 
            this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GenerateButton.Location = new System.Drawing.Point(312, 259);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(114, 41);
            this.GenerateButton.TabIndex = 22;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // OpenInBrowserButton
            // 
            this.OpenInBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OpenInBrowserButton.Location = new System.Drawing.Point(132, 259);
            this.OpenInBrowserButton.Name = "OpenInBrowserButton";
            this.OpenInBrowserButton.Size = new System.Drawing.Size(114, 41);
            this.OpenInBrowserButton.TabIndex = 23;
            this.OpenInBrowserButton.Text = "Open In Browser";
            this.OpenInBrowserButton.UseVisualStyleBackColor = true;
            this.OpenInBrowserButton.Click += new System.EventHandler(this.OpenInBrowserButton_Click);
            // 
            // CreateExtraDirCheckbox
            // 
            this.CreateExtraDirCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreateExtraDirCheckbox.AutoSize = true;
            this.CreateExtraDirCheckbox.Checked = true;
            this.CreateExtraDirCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.CreateExtraDirCheckbox.Location = new System.Drawing.Point(17, 231);
            this.CreateExtraDirCheckbox.Name = "CreateExtraDirCheckbox";
            this.CreateExtraDirCheckbox.Size = new System.Drawing.Size(154, 17);
            this.CreateExtraDirCheckbox.TabIndex = 24;
            this.CreateExtraDirCheckbox.Text = "Create Dedicated Directory";
            this.CreateExtraDirCheckbox.UseVisualStyleBackColor = true;
            this.CreateExtraDirCheckbox.CheckedChanged += new System.EventHandler(this.CreateExtraDirCheckbox_CheckedChanged);
            // 
            // GenerateProgressBar
            // 
            this.GenerateProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GenerateProgressBar.Location = new System.Drawing.Point(12, 260);
            this.GenerateProgressBar.Name = "GenerateProgressBar";
            this.GenerateProgressBar.Size = new System.Drawing.Size(413, 41);
            this.GenerateProgressBar.TabIndex = 25;
            this.GenerateProgressBar.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 313);
            this.Controls.Add(this.GenerateProgressBar);
            this.Controls.Add(this.CreateExtraDirCheckbox);
            this.Controls.Add(this.OpenInBrowserButton);
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.ShowOutputButton);
            this.Controls.Add(this.BuildDirBrowse);
            this.Controls.Add(this.BuildDir);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.HostName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.IPAName);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.IPADate);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.IPAMinOS);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.IPAVersion);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.IPAIdentifier);
            this.Controls.Add(this.IPAIcon);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.IPAPath);
            this.Controls.Add(this.IPADisplayName);
            this.Controls.Add(this.IPABrowse);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(454, 351);
            this.Name = "MainForm";
            this.Text = "IPA Web Installer Generator";
            ((System.ComponentModel.ISupportInitialize)(this.IPAIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button IPABrowse;
        private System.Windows.Forms.Label IPADisplayName;
        private System.Windows.Forms.Label IPAPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog IPASelectDialog;
        private System.Windows.Forms.PictureBox IPAIcon;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label IPAIdentifier;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label IPAVersion;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label IPAMinOS;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label IPADate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label IPAName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox HostName;
        private System.Windows.Forms.ComboBox BuildDir;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button BuildDirBrowse;
        private System.Windows.Forms.FolderBrowserDialog BuildDirDialog;
        private System.Windows.Forms.Button ShowOutputButton;
        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.Button OpenInBrowserButton;
        private System.Windows.Forms.CheckBox CreateExtraDirCheckbox;
        private System.Windows.Forms.ProgressBar GenerateProgressBar;
    }
}

