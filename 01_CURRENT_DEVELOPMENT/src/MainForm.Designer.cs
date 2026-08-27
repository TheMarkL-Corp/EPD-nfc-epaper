namespace AG_EPD_Tag
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblPort = new System.Windows.Forms.Label();
            this.cmbPorts = new System.Windows.Forms.ComboBox();
            this.btnRefreshPorts = new System.Windows.Forms.Button();
            this.lblStatusDot = new System.Windows.Forms.Label();
            this.lblStatusText = new System.Windows.Forms.Label();
            this.lblTagInfo = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.grpInputs = new System.Windows.Forms.GroupBox();
            this.lblLine2 = new System.Windows.Forms.Label();
            this.txtLine2 = new System.Windows.Forms.TextBox();
            this.lblLine1 = new System.Windows.Forms.Label();
            this.txtLine1 = new System.Windows.Forms.TextBox();
            this.grpStyle = new System.Windows.Forms.GroupBox();
            this.rbStyleB = new System.Windows.Forms.RadioButton();
            this.rbStyleA = new System.Windows.Forms.RadioButton();
            this.chkBorder = new System.Windows.Forms.CheckBox();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnProgram = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblSpendTime = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelCenter.SuspendLayout();
            this.grpInputs.SuspendLayout();
            this.grpStyle.SuspendLayout();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(251)))));
            this.panelTop.Controls.Add(this.lblPort);
            this.panelTop.Controls.Add(this.cmbPorts);
            this.panelTop.Controls.Add(this.btnRefreshPorts);
            this.panelTop.Controls.Add(this.lblStatusDot);
            this.panelTop.Controls.Add(this.lblStatusText);
            this.panelTop.Controls.Add(this.lblTagInfo);
            this.panelTop.Controls.Add(this.cmbLanguage);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(784, 52);
            this.panelTop.TabIndex = 0;
            // 
            // lblPort
            // 
            this.lblPort.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPort.Location = new System.Drawing.Point(10, 16);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(95, 20);
            this.lblPort.TabIndex = 0;
            this.lblPort.Text = "COM Port:";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbPorts
            // 
            this.cmbPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPorts.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.cmbPorts.FormattingEnabled = true;
            this.cmbPorts.Location = new System.Drawing.Point(108, 14);
            this.cmbPorts.Name = "cmbPorts";
            this.cmbPorts.Size = new System.Drawing.Size(106, 23);
            this.cmbPorts.TabIndex = 1;
            this.cmbPorts.SelectedIndexChanged += new System.EventHandler(this.cmbPorts_SelectedIndexChanged);
            // 
            // btnRefreshPorts
            // 
            this.btnRefreshPorts.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.btnRefreshPorts.Location = new System.Drawing.Point(216, 13);
            this.btnRefreshPorts.Name = "btnRefreshPorts";
            this.btnRefreshPorts.Size = new System.Drawing.Size(78, 25);
            this.btnRefreshPorts.TabIndex = 2;
            this.btnRefreshPorts.Text = "Refresh";
            this.btnRefreshPorts.UseVisualStyleBackColor = true;
            this.btnRefreshPorts.Click += new System.EventHandler(this.btnRefreshPorts_Click);
            // 
            // lblStatusDot
            // 
            this.lblStatusDot.AutoSize = true;
            this.lblStatusDot.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblStatusDot.ForeColor = System.Drawing.Color.Red;
            this.lblStatusDot.Location = new System.Drawing.Point(302, 15);
            this.lblStatusDot.Name = "lblStatusDot";
            this.lblStatusDot.Size = new System.Drawing.Size(20, 20);
            this.lblStatusDot.TabIndex = 3;
            this.lblStatusDot.Text = "●";
            // 
            // lblStatusText
            // 
            this.lblStatusText.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatusText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.lblStatusText.Location = new System.Drawing.Point(324, 16);
            this.lblStatusText.Name = "lblStatusText";
            this.lblStatusText.Size = new System.Drawing.Size(170, 18);
            this.lblStatusText.TabIndex = 4;
            this.lblStatusText.Text = "No Tag Detected";
            this.lblStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTagInfo
            // 
            this.lblTagInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTagInfo.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblTagInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblTagInfo.Location = new System.Drawing.Point(430, 16);
            this.lblTagInfo.Name = "lblTagInfo";
            this.lblTagInfo.Size = new System.Drawing.Size(242, 18);
            this.lblTagInfo.TabIndex = 5;
            this.lblTagInfo.Text = "UID: -- | FW: --";
            this.lblTagInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8.5F);
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English",
            "繁體中文"});
            this.cmbLanguage.Location = new System.Drawing.Point(682, 14);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(92, 23);
            this.cmbLanguage.TabIndex = 6;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // panelCenter
            // 
            this.panelCenter.Controls.Add(this.grpInputs);
            this.panelCenter.Controls.Add(this.grpStyle);
            this.panelCenter.Controls.Add(this.grpPreview);
            this.panelCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCenter.Location = new System.Drawing.Point(0, 52);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Padding = new System.Windows.Forms.Padding(15);
            this.panelCenter.Size = new System.Drawing.Size(784, 478);
            this.panelCenter.TabIndex = 1;
            // 
            // grpInputs
            // 
            this.grpInputs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInputs.Controls.Add(this.lblLine2);
            this.grpInputs.Controls.Add(this.txtLine2);
            this.grpInputs.Controls.Add(this.lblLine1);
            this.grpInputs.Controls.Add(this.txtLine1);
            this.grpInputs.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpInputs.Location = new System.Drawing.Point(15, 345);
            this.grpInputs.Name = "grpInputs";
            this.grpInputs.Size = new System.Drawing.Size(465, 118);
            this.grpInputs.TabIndex = 1;
            this.grpInputs.TabStop = false;
            this.grpInputs.Text = " Tag Text Settings ";
            // 
            // lblLine2
            // 
            this.lblLine2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblLine2.Location = new System.Drawing.Point(8, 26);
            this.lblLine2.Name = "lblLine2";
            this.lblLine2.Size = new System.Drawing.Size(150, 22);
            this.lblLine2.TabIndex = 0;
            this.lblLine2.Text = "Header Text:";
            this.lblLine2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLine2
            // 
            this.txtLine2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLine2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9.75F);
            this.txtLine2.Location = new System.Drawing.Point(164, 24);
            this.txtLine2.Name = "txtLine2";
            this.txtLine2.Size = new System.Drawing.Size(288, 24);
            this.txtLine2.TabIndex = 1;
            this.txtLine2.Text = "";
            this.txtLine2.TextChanged += new System.EventHandler(this.txtLine_TextChanged);
            // 
            // lblLine1
            // 
            this.lblLine1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblLine1.Location = new System.Drawing.Point(8, 68);
            this.lblLine1.Name = "lblLine1";
            this.lblLine1.Size = new System.Drawing.Size(150, 22);
            this.lblLine1.TabIndex = 2;
            this.lblLine1.Text = "Main Body Text:";
            this.lblLine1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtLine1
            // 
            this.txtLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLine1.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.txtLine1.Location = new System.Drawing.Point(164, 66);
            this.txtLine1.Name = "txtLine1";
            this.txtLine1.Size = new System.Drawing.Size(288, 24);
            this.txtLine1.TabIndex = 3;
            this.txtLine1.Text = "";
            this.txtLine1.TextChanged += new System.EventHandler(this.txtLine_TextChanged);
            // 
            // grpStyle
            // 
            this.grpStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStyle.Controls.Add(this.rbStyleB);
            this.grpStyle.Controls.Add(this.rbStyleA);
            this.grpStyle.Controls.Add(this.chkBorder);
            this.grpStyle.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpStyle.Location = new System.Drawing.Point(490, 345);
            this.grpStyle.Name = "grpStyle";
            this.grpStyle.Size = new System.Drawing.Size(279, 118);
            this.grpStyle.TabIndex = 2;
            this.grpStyle.TabStop = false;
            this.grpStyle.Text = " Display Layout Style ";
            // 
            // rbStyleB
            // 
            this.rbStyleB.AutoSize = true;
            this.rbStyleB.Checked = true;
            this.rbStyleB.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.rbStyleB.Location = new System.Drawing.Point(14, 25);
            this.rbStyleB.Name = "rbStyleB";
            this.rbStyleB.Size = new System.Drawing.Size(189, 19);
            this.rbStyleB.TabIndex = 0;
            this.rbStyleB.TabStop = true;
            this.rbStyleB.Text = "Style B: Clean White (Default)";
            this.rbStyleB.UseVisualStyleBackColor = true;
            this.rbStyleB.CheckedChanged += new System.EventHandler(this.styleOption_Changed);
            // 
            // rbStyleA
            // 
            this.rbStyleA.AutoSize = true;
            this.rbStyleA.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.rbStyleA.Location = new System.Drawing.Point(14, 53);
            this.rbStyleA.Name = "rbStyleA";
            this.rbStyleA.Size = new System.Drawing.Size(187, 19);
            this.rbStyleA.TabIndex = 1;
            this.rbStyleA.Text = "Style A: Black Header Banner";
            this.rbStyleA.UseVisualStyleBackColor = true;
            this.rbStyleA.CheckedChanged += new System.EventHandler(this.styleOption_Changed);
            // 
            // chkBorder
            // 
            this.chkBorder.AutoSize = true;
            this.chkBorder.Checked = false;
            this.chkBorder.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.chkBorder.Location = new System.Drawing.Point(14, 82);
            this.chkBorder.Name = "chkBorder";
            this.chkBorder.Size = new System.Drawing.Size(188, 19);
            this.chkBorder.TabIndex = 2;
            this.chkBorder.Text = "Show Rounded Outer Border";
            this.chkBorder.UseVisualStyleBackColor = true;
            this.chkBorder.CheckedChanged += new System.EventHandler(this.styleOption_Changed);
            // 
            // grpPreview
            // 
            this.grpPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreview.Controls.Add(this.picPreview);
            this.grpPreview.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPreview.Location = new System.Drawing.Point(15, 12);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(754, 323);
            this.grpPreview.TabIndex = 0;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = " Live Tag Preview (2.13\" 296×128) ";
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.White;
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(3, 19);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(748, 301);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(251)))));
            this.panelBottom.Controls.Add(this.btnProgram);
            this.panelBottom.Controls.Add(this.progressBar);
            this.panelBottom.Controls.Add(this.lblProgress);
            this.panelBottom.Controls.Add(this.lblSpendTime);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 530);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(784, 65);
            this.panelBottom.TabIndex = 2;
            // 
            // btnProgram
            // 
            this.btnProgram.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProgram.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnProgram.Enabled = false;
            this.btnProgram.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProgram.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10.5F, System.Drawing.FontStyle.Bold);
            this.btnProgram.ForeColor = System.Drawing.Color.White;
            this.btnProgram.Location = new System.Drawing.Point(605, 12);
            this.btnProgram.Name = "btnProgram";
            this.btnProgram.Size = new System.Drawing.Size(164, 41);
            this.btnProgram.TabIndex = 0;
            this.btnProgram.Text = "Write Tag";
            this.btnProgram.UseVisualStyleBackColor = false;
            this.btnProgram.Click += new System.EventHandler(this.btnProgram_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(15, 15);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(575, 18);
            this.progressBar.TabIndex = 1;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8.5F);
            this.lblProgress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblProgress.Location = new System.Drawing.Point(14, 39);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(40, 15);
            this.lblProgress.TabIndex = 2;
            this.lblProgress.Text = "Ready";
            // 
            // lblSpendTime
            // 
            this.lblSpendTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSpendTime.Font = new System.Drawing.Font("Microsoft JhengHei UI", 8.5F);
            this.lblSpendTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblSpendTime.Location = new System.Drawing.Point(440, 39);
            this.lblSpendTime.Name = "lblSpendTime";
            this.lblSpendTime.Size = new System.Drawing.Size(150, 15);
            this.lblSpendTime.TabIndex = 3;
            this.lblSpendTime.Text = "Duration: 0.0s";
            this.lblSpendTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(784, 595);
            this.Controls.Add(this.panelCenter);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.MinimumSize = new System.Drawing.Size(760, 570);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MedTRx EPD v1.0.2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelCenter.ResumeLayout(false);
            this.grpInputs.ResumeLayout(false);
            this.grpInputs.PerformLayout();
            this.grpStyle.ResumeLayout(false);
            this.grpStyle.PerformLayout();
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cmbPorts;
        private System.Windows.Forms.Button btnRefreshPorts;
        private System.Windows.Forms.Label lblStatusDot;
        private System.Windows.Forms.Label lblStatusText;
        private System.Windows.Forms.Label lblTagInfo;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.GroupBox grpInputs;
        private System.Windows.Forms.Label lblLine2;
        private System.Windows.Forms.TextBox txtLine2;
        private System.Windows.Forms.Label lblLine1;
        private System.Windows.Forms.TextBox txtLine1;
        private System.Windows.Forms.GroupBox grpStyle;
        private System.Windows.Forms.RadioButton rbStyleB;
        private System.Windows.Forms.RadioButton rbStyleA;
        private System.Windows.Forms.CheckBox chkBorder;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button btnProgram;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblSpendTime;
    }
}



