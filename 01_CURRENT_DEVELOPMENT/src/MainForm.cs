using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Ports;
using System.Media;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using com.advantech.nfc;
using com.advantech.nfc.api;
using com.advantech.nfc.cmd;
using J_RFID;

namespace AG_EPD_Tag
{
    public partial class MainForm : Form, NFCTagChangeListener, IDrawImageCallback
    {
        private AppSettings settings;
        private NFCManager manager;
        private D30Command nfc;
        private LeoD30EPDAPI api;
        private NFCTagState currentState = NFCTagState.NFC_TAG_STATE_TAG_OFF;
        private string currentUid = "--";
        private string currentFwVersion = "--";
        private string currentPlatform = "EPD-210--TC2";

        private System.Timers.Timer spendTimer;
        private float spendTimeSeconds = 0f;
        private bool isScanningPorts = false;

        public MainForm()
        {
            InitializeComponent();

            // Load settings
            settings = AppSettings.Load();

            // Set Application & Window Icon from logo.ico
            LoadApplicationIcon();

            // Initialize Localization
            Localization.CurrentLanguage = settings.Language;
            if (settings.Language == "zh-TW" || settings.Language == "zh")
            {
                cmbLanguage.SelectedIndex = 1;
            }
            else
            {
                cmbLanguage.SelectedIndex = 0;
            }

            // Restore style preferences
            if (settings.LastStyle == 0)
            {
                rbStyleA.Checked = true;
            }
            else
            {
                rbStyleB.Checked = true;
            }
            chkBorder.Checked = settings.ShowBorder;

            manager = NFCManager.getInstance();
            manager.TagChange = this;

            spendTimer = new System.Timers.Timer(100.0);
            spendTimer.Elapsed += SpendTimer_Elapsed;
        }

        private void LoadApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            ApplyLanguage();
            UpdatePreview();
            UpdateTagStatusDisplay(NFCTagState.NFC_TAG_STATE_TAG_OFF);
            await ScanAndPopulatePortsAsync();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentSettings();
            CloseCurrentConnection();
        }

        private void SaveCurrentSettings()
        {
            settings.Language = (cmbLanguage.SelectedIndex == 1) ? "zh-TW" : "en";
            settings.LastStyle = rbStyleA.Checked ? 0 : 1;
            settings.ShowBorder = chkBorder.Checked;
            if (cmbPorts.SelectedItem != null)
            {
                settings.LastComPort = cmbPorts.SelectedItem.ToString();
            }
            settings.Save();
        }

        #region Localization Management

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newLang = (cmbLanguage.SelectedIndex == 1) ? "zh-TW" : "en";
            if (Localization.CurrentLanguage != newLang)
            {
                Localization.CurrentLanguage = newLang;
                settings.Language = newLang;
                settings.Save();
                ApplyLanguage();
            }
        }

        private void ApplyLanguage()
        {
            this.Text = Localization.Get("AppTitle");
            lblPort.Text = Localization.Get("ComPort");
            btnRefreshPorts.Text = isScanningPorts ? Localization.Get("Scanning") : Localization.Get("Refresh");
            grpInputs.Text = Localization.Get("TagContentGroup");
            lblLine2.Text = Localization.Get("Line2Label");
            lblLine1.Text = Localization.Get("Line1Label");
            grpStyle.Text = Localization.Get("StyleGroup");
            rbStyleB.Text = Localization.Get("StyleB");
            rbStyleA.Text = Localization.Get("StyleA");
            chkBorder.Text = Localization.Get("ShowBorder");
            grpPreview.Text = Localization.Get("PreviewGroup");
            btnProgram.Text = Localization.Get("ProgramTag");
            lblProgress.Text = Localization.Get("Ready");
            lblSpendTime.Text = Localization.Get("Duration", spendTimeSeconds.ToString("F1"));

            UpdateTagStatusDisplay(currentState);
        }

        #endregion

        #region Asynchronous Non-Blocking Port Management

        private async void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            await ScanAndPopulatePortsAsync();
        }

        private async Task ScanAndPopulatePortsAsync()
        {
            if (isScanningPorts) return;

            isScanningPorts = true;
            btnRefreshPorts.Enabled = false;
            btnRefreshPorts.Text = Localization.Get("Scanning");
            lblStatusText.Text = Localization.Get("Scanning");

            CloseCurrentConnection();

            string lastPort = settings.LastComPort;

            // 1. Get system serial port names in background thread
            string[] ports = await Task.Run(() =>
            {
                try { return SerialPort.GetPortNames(); }
                catch { return new string[0]; }
            });

            // 2. Probe ports non-blockingly to find the Jogtek reader
            string verifiedReaderPort = await Task.Run(() =>
            {
                // First test the last successfully connected port if available
                if (!string.IsNullOrEmpty(lastPort) && Array.IndexOf(ports, lastPort) >= 0)
                {
                    if (QuickProbePort(lastPort))
                    {
                        return lastPort;
                    }
                }

                // Next test remaining ports
                foreach (string p in ports)
                {
                    if (p == lastPort) continue;
                    if (QuickProbePort(p))
                    {
                        return p;
                    }
                }
                return null;
            });

            cmbPorts.Items.Clear();
            foreach (string p in ports)
            {
                cmbPorts.Items.Add(p);
            }

            if (ports.Length == 0)
            {
                lblStatusDot.ForeColor = Color.Red;
                lblStatusText.Text = Localization.Get("NoPorts");
                lblTagInfo.Text = Localization.Get("PlugInReader");
            }
            else if (verifiedReaderPort != null)
            {
                cmbPorts.SelectedItem = verifiedReaderPort;
            }
            else if (!string.IsNullOrEmpty(lastPort) && Array.IndexOf(ports, lastPort) >= 0)
            {
                cmbPorts.SelectedItem = lastPort;
            }
            else
            {
                cmbPorts.SelectedIndex = 0;
            }

            isScanningPorts = false;
            btnRefreshPorts.Text = Localization.Get("Refresh");
            btnRefreshPorts.Enabled = true;
        }

        private static bool QuickProbePort(string portName)
        {
            RFIDAPI probe = new RFIDAPI();
            try
            {
                if (probe.RFID_OpenReader(portName) == 0)
                {
                    string fwVer = "";
                    byte res = probe.RFID_FWVersion(out fwVer);
                    probe.RFID_CloseReader(portName);
                    return (res == 0);
                }
            }
            catch
            {
                try { probe.RFID_CloseReader(portName); } catch { }
            }
            return false;
        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPorts.SelectedItem != null && !isScanningPorts)
            {
                string portName = cmbPorts.SelectedItem.ToString();
                BuildConnection(portName);
            }
        }

        private void BuildConnection(string portName)
        {
            CloseCurrentConnection();
            try
            {
                nfc = new D30Command(portName);
                if (nfc.openNFC())
                {
                    manager.setNFCCommand(nfc);
                    api = (LeoD30EPDAPI)manager.getNfcAPI();

                    // Remember this successfully opened port
                    settings.LastComPort = portName;
                    settings.Save();
                }
                else
                {
                    nfc = null;
                    lblStatusDot.ForeColor = Color.Red;
                    lblStatusText.Text = Localization.Get("OpenFailed", portName);
                }
            }
            catch (Exception ex)
            {
                nfc = null;
                lblStatusDot.ForeColor = Color.Red;
                lblStatusText.Text = "Error: " + ex.Message;
            }
        }

        private void CloseCurrentConnection()
        {
            if (nfc != null)
            {
                try
                {
                    UpdateTagStatusDisplay(NFCTagState.NFC_TAG_STATE_TAG_OFF);
                    api = null;
                    manager.setNFCCommand(null);
                    nfc.closeNFC();
                }
                catch { }
                nfc = null;
            }
        }

        #endregion

        #region Tag State Listener (NFCTagChangeListener)

        public void onTagStateChange(NFCTagState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<NFCTagState>(onTagStateChange), state);
                return;
            }

            currentState = state;
            UpdateTagStatusDisplay(state);
        }

        private void UpdateTagStatusDisplay(NFCTagState state)
        {
            switch (state)
            {
                case NFCTagState.NFC_TAG_STATE_TAG_OFF:
                    lblStatusDot.ForeColor = Color.Red;
                    lblStatusText.Text = Localization.Get("NoTag");
                    currentUid = "--";
                    currentFwVersion = "--";
                    lblTagInfo.Text = Localization.Get("TagInfo", "--", "--");
                    btnProgram.Enabled = false;
                    btnProgram.BackColor = Color.FromArgb(180, 180, 180);
                    break;

                case NFCTagState.NFC_TAG_STATE_TAG_ON:
                    lblStatusDot.ForeColor = Color.Goldenrod;
                    lblStatusText.Text = Localization.Get("TagConnecting");
                    btnProgram.Enabled = false;
                    btnProgram.BackColor = Color.FromArgb(180, 180, 180);
                    FetchTagUID();
                    break;

                case NFCTagState.NFC_TAG_STATE_COMM_ON:
                    lblStatusDot.ForeColor = Color.ForestGreen;
                    lblStatusText.Text = Localization.Get("TagReady");
                    FetchTagUID();
                    FetchTagFirmwareAndModel();
                    btnProgram.Enabled = true;
                    btnProgram.BackColor = Color.FromArgb(0, 122, 204);
                    break;
            }
        }

        private void FetchTagUID()
        {
            if (api != null)
            {
                try
                {
                    byte[] uidBytes = api.getTagID();
                    if (uidBytes != null && uidBytes.Length > 0)
                    {
                        currentUid = "";
                        foreach (byte b in uidBytes)
                        {
                            currentUid += b.ToString("X2");
                        }
                    }
                }
                catch { }
            }
            lblTagInfo.Text = Localization.Get("TagInfo", currentUid, currentFwVersion);
        }

        private void FetchTagFirmwareAndModel()
        {
            if (manager != null && manager._epd_api != null)
            {
                try
                {
                    string fw = manager._epd_api.GetVersion();
                    if (!string.IsNullOrEmpty(fw))
                    {
                        currentFwVersion = fw;
                    }

                    string model = manager._epd_api.GetPlatformName();
                    if (!string.IsNullOrEmpty(model))
                    {
                        currentPlatform = model.Trim();
                    }
                }
                catch { }
            }
            lblTagInfo.Text = Localization.Get("TagInfo", currentUid, currentFwVersion);
        }

        #endregion

        #region Live WYSIWYG Tag Preview & Style Selector

        private void txtLine_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void styleOption_Changed(object sender, EventArgs e)
        {
            settings.LastStyle = rbStyleA.Checked ? 0 : 1;
            settings.ShowBorder = chkBorder.Checked;
            settings.Save();
            UpdatePreview();
        }

        private TagStyle GetCurrentStyle()
        {
            return rbStyleA.Checked ? TagStyle.StyleA_BlackHeader : TagStyle.StyleB_CleanWhite;
        }

        private void UpdatePreview()
        {
            string line1 = txtLine1.Text;
            string line2 = txtLine2.Text;
            bool showBorder = chkBorder.Checked;
            TagStyle style = GetCurrentStyle();

            using (Bitmap rawTag = TagRenderer.RenderTag(line1, line2, showBorder, style))
            {
                int previewWidth = TagRenderer.TAG_WIDTH * 2;
                int previewHeight = TagRenderer.TAG_HEIGHT * 2;
                Bitmap displayBmp = new Bitmap(previewWidth, previewHeight);

                using (Graphics g = Graphics.FromImage(displayBmp))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.Clear(Color.White);
                    g.DrawImage(rawTag, new Rectangle(0, 0, previewWidth, previewHeight));
                }

                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                }
                picPreview.Image = displayBmp;
            }
        }

        #endregion

        #region Tag Programming / Flashing

        private void btnProgram_Click(object sender, EventArgs e)
        {
            if (currentState != NFCTagState.NFC_TAG_STATE_COMM_ON)
            {
                MessageBox.Show(Localization.Get("TagNotReadyMsg"), Localization.Get("TagNotReadyTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SetControlsActive(false);

                // 1. PIN Unlock (Default "0000")
                byte[] pin = new byte[] { 48, 48, 48, 48 };
                if (api != null && !api.UnlockPinCode(pin))
                {
                    // If already unlocked, continue
                }

                // 2. Query Model and FW Version
                string platformName = currentPlatform;
                string vername = currentFwVersion;
                if (manager != null && manager._epd_api != null)
                {
                    try
                    {
                        platformName = manager._epd_api.GetPlatformName();
                        vername = manager._epd_api.GetVersion();
                    }
                    catch { }
                }

                int num4 = ParseNumericVersion(vername);

                // 3. Compression & Packet Size Setup (DKE panel & firmware matrix)
                int lz4flag = 1;
                int lz4packsize = 1024;
                int pages = 1;

                if (platformName.Contains("EPD-210") || platformName.Contains("210"))
                {
                    if (num4 < 100)
                    {
                        lz4flag = 0;
                        lz4packsize = 0;
                    }
                    else if (num4 >= 400)
                    {
                        lz4flag = 1;
                        lz4packsize = 4096; // FW >= 4.0.0 DKE High-Speed Segment
                    }
                    else
                    {
                        lz4flag = 1;
                        lz4packsize = 1024;
                    }
                }
                else
                {
                    lz4flag = 0;
                    lz4packsize = 0;
                }

                // 4. Render raw 296x128 Tag Bitmap with user's selected style
                TagStyle style = GetCurrentStyle();
                Bitmap tagBitmap = TagRenderer.RenderTag(txtLine1.Text, txtLine2.Text, chkBorder.Checked, style);

                // 5. Create EinkImage with Linchun SDK auto-detection (routes to img_forDKEEPD_BW if num4 >= 400)
                EinkImage einkImage = new EinkImage(TagRenderer.TAG_WIDTH, TagRenderer.TAG_HEIGHT, pages, tagBitmap, lz4flag, lz4packsize, platformName, vername);

                // 6. Reset Timer & Start Flash
                spendTimeSeconds = 0f;
                lblSpendTime.Text = Localization.Get("Duration", "0.0");
                spendTimer.Start();

                lblProgress.Text = Localization.Get("InitiatingFlash");
                progressBar.Value = 5;

                api.DrawImage(einkImage, DrawImageMethod.DIMethod_Normal, this);
                tagBitmap.Dispose();
            }
            catch (Exception ex)
            {
                spendTimer.Stop();
                SetControlsActive(true);
                MessageBox.Show("Failed to program tag: " + ex.Message, Localization.Get("ProgrammingErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ParseNumericVersion(string ver)
        {
            try
            {
                if (string.IsNullOrEmpty(ver) || ver == "--") return 400; // Default to modern 4.0.0
                string[] parts = ver.Split('.');
                int maj = parts.Length > 0 ? int.Parse(parts[0]) : 0;
                int min = parts.Length > 1 ? int.Parse(parts[1]) : 0;
                int bld = parts.Length > 2 ? int.Parse(parts[2]) : 0;
                return (maj * 100) + (min * 10) + bld;
            }
            catch
            {
                return 400;
            }
        }

        private void SpendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateSpendTimerDisplay));
            }
            else
            {
                UpdateSpendTimerDisplay();
            }
        }

        private void UpdateSpendTimerDisplay()
        {
            spendTimeSeconds += 0.1f;
            lblSpendTime.Text = Localization.Get("Duration", spendTimeSeconds.ToString("F1"));
        }

        private void SetControlsActive(bool active)
        {
            btnProgram.Enabled = active && (currentState == NFCTagState.NFC_TAG_STATE_COMM_ON);
            cmbPorts.Enabled = active;
            btnRefreshPorts.Enabled = active;
            txtLine1.Enabled = active;
            txtLine2.Enabled = active;
            rbStyleA.Enabled = active;
            rbStyleB.Enabled = active;
            chkBorder.Enabled = active;
            cmbLanguage.Enabled = active;
        }

        #endregion

        #region DrawImage Progress Callback (IDrawImageCallback)

        public void onProgress(DrawImageState state, object data)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<DrawImageState, object>(onProgress), state, data);
                return;
            }

            int percent = (data is int) ? (int)data : 0;
            string statusInfo = "";

            switch (state)
            {
                case DrawImageState.DIState_Erase:
                    statusInfo = Localization.Get("ErasingFlash");
                    percent = 10;
                    break;

                case DrawImageState.DIState_SendData:
                    int mappedPercent = (int)(10.0 + (double)percent * 0.8);
                    if (mappedPercent > 90) mappedPercent = 90;
                    statusInfo = Localization.Get("UploadingImage", mappedPercent);
                    percent = mappedPercent;
                    break;

                case DrawImageState.DIState_WriteToEPD:
                    statusInfo = Localization.Get("RefreshingDisplay");
                    percent = 92;
                    break;

                case DrawImageState.DIState_Finish:
                    statusInfo = Localization.Get("Completed");
                    percent = 100;
                    break;

                case DrawImageState.DIState_Error:
                    statusInfo = Localization.Get("FlashError");
                    percent = 100;
                    break;
            }

            lblProgress.Text = statusInfo;
            progressBar.Value = Math.Min(100, Math.Max(0, percent));

            if (state == DrawImageState.DIState_Finish || state == DrawImageState.DIState_Error)
            {
                spendTimer.Stop();
                SetControlsActive(true);

                if (state == DrawImageState.DIState_Finish)
                {
                    try { SystemSounds.Beep.Play(); } catch { }
                    lblProgress.ForeColor = Color.ForestGreen;
                }
                else
                {
                    try { SystemSounds.Hand.Play(); } catch { }
                    lblProgress.ForeColor = Color.Red;
                    MessageBox.Show(Localization.Get("TransferFailedMsg"), Localization.Get("TransferFailedTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion
    }
}
