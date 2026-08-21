using AdvNFCWrap;
using BarcodeLib;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace NFCApp
{
	public class Form1 : Form, NFCWrap.TagState, NFCWrap.ProcessState
	{
		private NFCWrap oNFC = new NFCWrap();

		private IContainer components;

		private Button btnDetectPort;

		private TextBox txtPort;

		private Button btnDrawImage;

		private TextBox txtUnLock;

		private Button btnUnLock;

		private TextBox txtTagData;

		private Button btnWriteTagData;

		private Button btnGetTagData;

		private Button btnGetPinCodeStatus;

		private Button btnGetSN;

		private Button btnGetPlatformName;

		private Button btnGetVersion;

		private Button btnGetTagID;

		private Button btnCreateImage2;

		private TextBox txtPinCode;

		private Button btnSetPingCode;

		private GroupBox groupBox1;

		private GroupBox groupBox2;

		private GroupBox groupBox3;

		private GroupBox groupBox4;

		private Label lblProgress;

		private Button btnConnect;

		private CheckBox chkDithering;

		private TabControl tabControl1;

		private TabPage tabPage1;

		private TabPage tabPage2;

		private Label lblTagStatus;

		private Label label1;

		public Form1()
		{
			InitializeComponent();
			enableButtonButtons(NFCWrap.nTagState.NFC_TAG_STATE_TAG_OFF);
			oNFC.TagStateListener = this;
			oNFC.ProcessStateListener = this;
		}

		public void onTagState(NFCWrap.nTagState state)
		{
			Invoke((MethodInvoker)delegate
			{
				lblTagStatus.Text = state.ToString();
				enableButtonButtons(state);
			});
		}

		public void onProcessState(NFCWrap.nImageState state, object data)
		{
			Invoke((MethodInvoker)delegate
			{
				Console.WriteLine(data.ToString());
				lblProgress.Text = "Progress : " + data.ToString() + " %";
				if (state == NFCWrap.nImageState.DIState_Finish)
				{
					lblProgress.Visible = false;
					btnCreateImage2.Enabled = true;
					MessageBox.Show("Complete!");
				}
				else
				{
					lblProgress.Visible = true;
				}
			});
		}

		private void btnDetectPort_Click(object sender, EventArgs e)
		{
			string port = oNFC.GetPort();
			if (port != "")
			{
				txtPort.Text = port;
				oNFC = new NFCWrap(txtPort.Text);
				lblTagStatus.Text = "Port detected.";
				enableButtonButtons(NFCWrap.nTagState.NFC_TAG_STATE_TAG_OFF);
			}
			else
			{
				MessageBox.Show("No correct port detected.");
			}
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.ConnectTag());
		}

		private void btnGetTagID_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetTagID());
		}

		private void btnGetVersion_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetVersion());
		}

		private void btnGetPlatformName_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetPlatformName());
		}

		private void btnGetSN_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetSN());
		}

		private void btnGetPinCodeStatus_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetPinCodeStatus());
		}

		private void btnGetTagData_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.GetTagData());
		}

		private void btnWriteTagData_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.WriteTagData(txtTagData.Text));
		}

		private void btnUnLock_Click(object sender, EventArgs e)
		{
			MessageBox.Show(oNFC.UnlockPinCode(txtUnLock.Text));
		}

		private void btnDrawImage_Click(object sender, EventArgs e)
		{
			string text = oNFC.UnlockPinCode(txtUnLock.Text);
			if (text == "OK")
			{
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp";
				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					Bitmap oImage = new Bitmap(openFileDialog.FileName);
					text = oNFC.DrawImage(oImage, chkDithering.Checked);
				}
			}
			else
			{
				MessageBox.Show(text);
			}
		}

		private void enableButtonButtons(NFCWrap.nTagState state)
		{
			btnDetectPort.Enabled = (txtPort.Text == "");
			btnConnect.Enabled = (txtPort.Text != "");
			btnGetTagData.Enabled = (txtPort.Text != "");
			btnWriteTagData.Enabled = (txtPort.Text != "");
			txtTagData.Enabled = (txtPort.Text != "");
			btnGetTagID.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnGetVersion.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnGetPlatformName.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnGetSN.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnGetPinCodeStatus.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnUnLock.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			txtUnLock.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnSetPingCode.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			txtPinCode.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnDrawImage.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
			btnCreateImage2.Enabled = (state == NFCWrap.nTagState.NFC_TAG_STATE_COMM_ON);
		}

		private void btnCreateImage2_Click(object sender, EventArgs e)
		{
			string text = oNFC.UnlockPinCode(txtUnLock.Text);
			if (text == "OK")
			{
				Bitmap bitmap = new Bitmap(Convert.ToInt32(296), Convert.ToInt32(128), PixelFormat.Format32bppArgb);
				Graphics graphics = Graphics.FromImage(bitmap);
				graphics.Clear(Color.White);
				Font font = new Font("Arial", 12f);
				SolidBrush brush = new SolidBrush(Color.Black);
				StringFormat stringFormat = new StringFormat();
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				graphics.DrawString("CG4C001501", font, brush, 10f, 10f, stringFormat);
				graphics.DrawString("2521M99G14", font, brush, 10f, 30f, stringFormat);
				graphics.DrawString("SER CDACURUC", font, brush, 10f, 50f, stringFormat);
				Barcode barcode = new Barcode();
				barcode.IncludeLabel = true;
				barcode.LabelFont = new Font("Verdana", 4f);
				barcode.Width = 220;
				barcode.Height = 50;
				Image image = barcode.Encode(TYPE.CODE128, "710176121145", barcode.Width, barcode.Height);
				Point point = new Point(0, 70);
				graphics.DrawImage(image, point);
				QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
				QrCode qrCode = new QrCode();
				qrEncoder.TryEncode("ITEM0012345678", out qrCode);
				GraphicsRenderer graphicsRenderer = new GraphicsRenderer(new FixedModuleSize(3, QuietZoneModules.Two), Brushes.Black, Brushes.White);
				graphicsRenderer.SizeCalculator.GetSize(qrCode.Matrix.Width);
				graphicsRenderer.Draw(offset: new Point(210, 5), graphics: graphics, QrMatrix: qrCode.Matrix);
				text = oNFC.DrawImage(bitmap);
			}
			else
			{
				MessageBox.Show(text);
			}
		}

		private void btnSetPingCode_Click(object sender, EventArgs e)
		{
			if (oNFC.UnlockPinCode(txtUnLock.Text) == "OK")
			{
				MessageBox.Show(oNFC.SetPingCode(txtPinCode.Text));
			}
			else
			{
				MessageBox.Show("UnLock Error");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(NFCApp.Form1));
			btnDetectPort = new System.Windows.Forms.Button();
			txtPort = new System.Windows.Forms.TextBox();
			btnDrawImage = new System.Windows.Forms.Button();
			txtUnLock = new System.Windows.Forms.TextBox();
			btnUnLock = new System.Windows.Forms.Button();
			txtTagData = new System.Windows.Forms.TextBox();
			btnWriteTagData = new System.Windows.Forms.Button();
			btnGetTagData = new System.Windows.Forms.Button();
			btnGetPinCodeStatus = new System.Windows.Forms.Button();
			btnGetSN = new System.Windows.Forms.Button();
			btnGetPlatformName = new System.Windows.Forms.Button();
			btnGetVersion = new System.Windows.Forms.Button();
			btnGetTagID = new System.Windows.Forms.Button();
			btnCreateImage2 = new System.Windows.Forms.Button();
			txtPinCode = new System.Windows.Forms.TextBox();
			btnSetPingCode = new System.Windows.Forms.Button();
			groupBox1 = new System.Windows.Forms.GroupBox();
			btnConnect = new System.Windows.Forms.Button();
			groupBox2 = new System.Windows.Forms.GroupBox();
			groupBox3 = new System.Windows.Forms.GroupBox();
			chkDithering = new System.Windows.Forms.CheckBox();
			lblProgress = new System.Windows.Forms.Label();
			groupBox4 = new System.Windows.Forms.GroupBox();
			tabControl1 = new System.Windows.Forms.TabControl();
			tabPage1 = new System.Windows.Forms.TabPage();
			tabPage2 = new System.Windows.Forms.TabPage();
			lblTagStatus = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			groupBox1.SuspendLayout();
			groupBox2.SuspendLayout();
			groupBox3.SuspendLayout();
			groupBox4.SuspendLayout();
			tabControl1.SuspendLayout();
			tabPage1.SuspendLayout();
			tabPage2.SuspendLayout();
			SuspendLayout();
			btnDetectPort.BackColor = System.Drawing.SystemColors.Window;
			btnDetectPort.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnDetectPort.Location = new System.Drawing.Point(27, 25);
			btnDetectPort.Name = "btnDetectPort";
			btnDetectPort.Size = new System.Drawing.Size(133, 27);
			btnDetectPort.TabIndex = 11;
			btnDetectPort.Text = "DetectPort";
			btnDetectPort.UseVisualStyleBackColor = false;
			btnDetectPort.Click += new System.EventHandler(btnDetectPort_Click);
			txtPort.Enabled = false;
			txtPort.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			txtPort.Location = new System.Drawing.Point(166, 27);
			txtPort.Name = "txtPort";
			txtPort.ReadOnly = true;
			txtPort.Size = new System.Drawing.Size(201, 27);
			txtPort.TabIndex = 12;
			btnDrawImage.BackColor = System.Drawing.SystemColors.Window;
			btnDrawImage.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnDrawImage.Location = new System.Drawing.Point(14, 56);
			btnDrawImage.Name = "btnDrawImage";
			btnDrawImage.Size = new System.Drawing.Size(133, 27);
			btnDrawImage.TabIndex = 32;
			btnDrawImage.Text = "Draw Image (File)";
			btnDrawImage.UseVisualStyleBackColor = false;
			btnDrawImage.Click += new System.EventHandler(btnDrawImage_Click);
			txtUnLock.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			txtUnLock.Location = new System.Drawing.Point(155, 31);
			txtUnLock.Name = "txtUnLock";
			txtUnLock.Size = new System.Drawing.Size(165, 27);
			txtUnLock.TabIndex = 31;
			txtUnLock.Text = "0000";
			btnUnLock.BackColor = System.Drawing.SystemColors.Window;
			btnUnLock.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnUnLock.Location = new System.Drawing.Point(14, 29);
			btnUnLock.Name = "btnUnLock";
			btnUnLock.Size = new System.Drawing.Size(133, 27);
			btnUnLock.TabIndex = 30;
			btnUnLock.Text = "UnLock";
			btnUnLock.UseVisualStyleBackColor = false;
			btnUnLock.Click += new System.EventHandler(btnUnLock_Click);
			txtTagData.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			txtTagData.Location = new System.Drawing.Point(155, 65);
			txtTagData.Name = "txtTagData";
			txtTagData.Size = new System.Drawing.Size(165, 27);
			txtTagData.TabIndex = 29;
			txtTagData.Text = "Hi ETag!";
			btnWriteTagData.BackColor = System.Drawing.SystemColors.Window;
			btnWriteTagData.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnWriteTagData.Location = new System.Drawing.Point(14, 65);
			btnWriteTagData.Name = "btnWriteTagData";
			btnWriteTagData.Size = new System.Drawing.Size(133, 27);
			btnWriteTagData.TabIndex = 28;
			btnWriteTagData.Text = "WriteTagData";
			btnWriteTagData.UseVisualStyleBackColor = false;
			btnWriteTagData.Click += new System.EventHandler(btnWriteTagData_Click);
			btnGetTagData.BackColor = System.Drawing.SystemColors.Window;
			btnGetTagData.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetTagData.Location = new System.Drawing.Point(14, 31);
			btnGetTagData.Name = "btnGetTagData";
			btnGetTagData.Size = new System.Drawing.Size(133, 27);
			btnGetTagData.TabIndex = 27;
			btnGetTagData.Text = "GetTagData";
			btnGetTagData.UseVisualStyleBackColor = false;
			btnGetTagData.Click += new System.EventHandler(btnGetTagData_Click);
			btnGetPinCodeStatus.BackColor = System.Drawing.SystemColors.Window;
			btnGetPinCodeStatus.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetPinCodeStatus.Location = new System.Drawing.Point(14, 109);
			btnGetPinCodeStatus.Name = "btnGetPinCodeStatus";
			btnGetPinCodeStatus.Size = new System.Drawing.Size(133, 27);
			btnGetPinCodeStatus.TabIndex = 26;
			btnGetPinCodeStatus.Text = "GetPinCodeStatus";
			btnGetPinCodeStatus.UseVisualStyleBackColor = false;
			btnGetPinCodeStatus.Click += new System.EventHandler(btnGetPinCodeStatus_Click);
			btnGetSN.BackColor = System.Drawing.SystemColors.Window;
			btnGetSN.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetSN.Location = new System.Drawing.Point(14, 68);
			btnGetSN.Name = "btnGetSN";
			btnGetSN.Size = new System.Drawing.Size(133, 27);
			btnGetSN.TabIndex = 25;
			btnGetSN.Text = "GetSN";
			btnGetSN.UseVisualStyleBackColor = false;
			btnGetSN.Click += new System.EventHandler(btnGetSN_Click);
			btnGetPlatformName.BackColor = System.Drawing.SystemColors.Window;
			btnGetPlatformName.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetPlatformName.Location = new System.Drawing.Point(153, 109);
			btnGetPlatformName.Name = "btnGetPlatformName";
			btnGetPlatformName.Size = new System.Drawing.Size(133, 27);
			btnGetPlatformName.TabIndex = 24;
			btnGetPlatformName.Text = "GetPlatformName";
			btnGetPlatformName.UseVisualStyleBackColor = false;
			btnGetPlatformName.Click += new System.EventHandler(btnGetPlatformName_Click);
			btnGetVersion.BackColor = System.Drawing.SystemColors.Window;
			btnGetVersion.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetVersion.Location = new System.Drawing.Point(153, 70);
			btnGetVersion.Name = "btnGetVersion";
			btnGetVersion.Size = new System.Drawing.Size(133, 27);
			btnGetVersion.TabIndex = 23;
			btnGetVersion.Text = "GetVersion";
			btnGetVersion.UseVisualStyleBackColor = false;
			btnGetVersion.Click += new System.EventHandler(btnGetVersion_Click);
			btnGetTagID.BackColor = System.Drawing.SystemColors.Window;
			btnGetTagID.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnGetTagID.Location = new System.Drawing.Point(153, 31);
			btnGetTagID.Name = "btnGetTagID";
			btnGetTagID.Size = new System.Drawing.Size(133, 27);
			btnGetTagID.TabIndex = 22;
			btnGetTagID.Text = "GetTagID";
			btnGetTagID.UseVisualStyleBackColor = false;
			btnGetTagID.Click += new System.EventHandler(btnGetTagID_Click);
			btnCreateImage2.BackColor = System.Drawing.SystemColors.Window;
			btnCreateImage2.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnCreateImage2.Location = new System.Drawing.Point(153, 56);
			btnCreateImage2.Name = "btnCreateImage2";
			btnCreateImage2.Size = new System.Drawing.Size(133, 27);
			btnCreateImage2.TabIndex = 39;
			btnCreateImage2.Text = "Draw Image (Code)";
			btnCreateImage2.UseVisualStyleBackColor = false;
			btnCreateImage2.Click += new System.EventHandler(btnCreateImage2_Click);
			txtPinCode.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			txtPinCode.Location = new System.Drawing.Point(155, 70);
			txtPinCode.Name = "txtPinCode";
			txtPinCode.Size = new System.Drawing.Size(165, 27);
			txtPinCode.TabIndex = 41;
			txtPinCode.Text = "0000";
			btnSetPingCode.BackColor = System.Drawing.SystemColors.Window;
			btnSetPingCode.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnSetPingCode.Location = new System.Drawing.Point(14, 68);
			btnSetPingCode.Name = "btnSetPingCode";
			btnSetPingCode.Size = new System.Drawing.Size(133, 27);
			btnSetPingCode.TabIndex = 40;
			btnSetPingCode.Text = "Set Ping Code";
			btnSetPingCode.UseVisualStyleBackColor = false;
			btnSetPingCode.Click += new System.EventHandler(btnSetPingCode_Click);
			groupBox1.Controls.Add(btnConnect);
			groupBox1.Controls.Add(btnGetTagID);
			groupBox1.Controls.Add(btnGetVersion);
			groupBox1.Controls.Add(btnGetPlatformName);
			groupBox1.Controls.Add(btnGetSN);
			groupBox1.Controls.Add(btnGetPinCodeStatus);
			groupBox1.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			groupBox1.Location = new System.Drawing.Point(47, 27);
			groupBox1.Name = "groupBox1";
			groupBox1.Size = new System.Drawing.Size(340, 160);
			groupBox1.TabIndex = 42;
			groupBox1.TabStop = false;
			groupBox1.Text = "Tag Information";
			btnConnect.BackColor = System.Drawing.SystemColors.Window;
			btnConnect.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnConnect.Location = new System.Drawing.Point(14, 31);
			btnConnect.Name = "btnConnect";
			btnConnect.Size = new System.Drawing.Size(133, 27);
			btnConnect.TabIndex = 27;
			btnConnect.Text = "Connect";
			btnConnect.UseVisualStyleBackColor = false;
			btnConnect.Click += new System.EventHandler(btnConnect_Click);
			groupBox2.Controls.Add(txtUnLock);
			groupBox2.Controls.Add(btnUnLock);
			groupBox2.Controls.Add(txtPinCode);
			groupBox2.Controls.Add(btnSetPingCode);
			groupBox2.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			groupBox2.Location = new System.Drawing.Point(431, 27);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new System.Drawing.Size(340, 118);
			groupBox2.TabIndex = 43;
			groupBox2.TabStop = false;
			groupBox2.Text = "Ping Code";
			groupBox3.Controls.Add(chkDithering);
			groupBox3.Controls.Add(lblProgress);
			groupBox3.Controls.Add(btnDrawImage);
			groupBox3.Controls.Add(btnCreateImage2);
			groupBox3.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			groupBox3.Location = new System.Drawing.Point(47, 211);
			groupBox3.Name = "groupBox3";
			groupBox3.Size = new System.Drawing.Size(340, 129);
			groupBox3.TabIndex = 44;
			groupBox3.TabStop = false;
			groupBox3.Text = "Draw";
			chkDithering.AutoSize = true;
			chkDithering.Location = new System.Drawing.Point(18, 30);
			chkDithering.Name = "chkDithering";
			chkDithering.Size = new System.Drawing.Size(86, 20);
			chkDithering.TabIndex = 54;
			chkDithering.Text = "Dithering";
			chkDithering.UseVisualStyleBackColor = true;
			lblProgress.AutoSize = true;
			lblProgress.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			lblProgress.Location = new System.Drawing.Point(15, 98);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(98, 16);
			lblProgress.TabIndex = 51;
			lblProgress.Text = "Progress : 0% ";
			lblProgress.Visible = false;
			groupBox4.Controls.Add(txtTagData);
			groupBox4.Controls.Add(btnGetTagData);
			groupBox4.Controls.Add(btnWriteTagData);
			groupBox4.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			groupBox4.Location = new System.Drawing.Point(47, 27);
			groupBox4.Name = "groupBox4";
			groupBox4.Size = new System.Drawing.Size(340, 102);
			groupBox4.TabIndex = 45;
			groupBox4.TabStop = false;
			groupBox4.Text = "Read/Write Data";
			tabControl1.Controls.Add(tabPage1);
			tabControl1.Controls.Add(tabPage2);
			tabControl1.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			tabControl1.Location = new System.Drawing.Point(27, 81);
			tabControl1.Name = "tabControl1";
			tabControl1.SelectedIndex = 0;
			tabControl1.Size = new System.Drawing.Size(827, 376);
			tabControl1.TabIndex = 54;
			tabPage1.Controls.Add(groupBox1);
			tabPage1.Controls.Add(groupBox3);
			tabPage1.Controls.Add(groupBox2);
			tabPage1.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			tabPage1.Location = new System.Drawing.Point(4, 26);
			tabPage1.Name = "tabPage1";
			tabPage1.Padding = new System.Windows.Forms.Padding(3);
			tabPage1.Size = new System.Drawing.Size(819, 346);
			tabPage1.TabIndex = 0;
			tabPage1.Text = "Image";
			tabPage1.UseVisualStyleBackColor = true;
			tabPage2.Controls.Add(groupBox4);
			tabPage2.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			tabPage2.Location = new System.Drawing.Point(4, 26);
			tabPage2.Name = "tabPage2";
			tabPage2.Padding = new System.Windows.Forms.Padding(3);
			tabPage2.Size = new System.Drawing.Size(819, 346);
			tabPage2.TabIndex = 1;
			tabPage2.Text = "Data";
			tabPage2.UseVisualStyleBackColor = true;
			lblTagStatus.AutoSize = true;
			lblTagStatus.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			lblTagStatus.Location = new System.Drawing.Point(449, 30);
			lblTagStatus.Name = "lblTagStatus";
			lblTagStatus.Size = new System.Drawing.Size(67, 16);
			lblTagStatus.TabIndex = 55;
			lblTagStatus.Text = "unknown";
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label1.Location = new System.Drawing.Point(393, 30);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(50, 16);
			label1.TabIndex = 56;
			label1.Text = "State : ";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.SystemColors.Window;
			base.ClientSize = new System.Drawing.Size(904, 496);
			base.Controls.Add(lblTagStatus);
			base.Controls.Add(label1);
			base.Controls.Add(tabControl1);
			base.Controls.Add(txtPort);
			base.Controls.Add(btnDetectPort);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "Form1";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "EPD210 NFC - Advantech";
			groupBox1.ResumeLayout(false);
			groupBox2.ResumeLayout(false);
			groupBox2.PerformLayout();
			groupBox3.ResumeLayout(false);
			groupBox3.PerformLayout();
			groupBox4.ResumeLayout(false);
			groupBox4.PerformLayout();
			tabControl1.ResumeLayout(false);
			tabPage1.ResumeLayout(false);
			tabPage2.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
