using AdvNFCWrap;
using BarcodeLib;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Windows.Forms;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("NFCApp")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("NFCApp")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("7e426902-e03f-451d-b8a9-e0314a78494d")]
[assembly: AssemblyFileVersion("1.0.1.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.6.1", FrameworkDisplayName = ".NET Framework 4.6.1")]
[assembly: AssemblyVersion("1.0.1.0")]
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
	public class Form2 : Form, NFCWrap.TagState, NFCWrap.ProcessState
	{
		private NFCWrap oNFC = new NFCWrap();

		private Bitmap mBitmap;

		private int mRow;

		private IContainer components;

		private Label lblProgress;

		private Button btnImport;

		private DataGridView dataGridView1;

		private CheckBox chkAutoRefresh;

		private Panel panel1;

		private Label lblTagStatus;

		private Label lblPort;

		private Label label1;

		private Label label2;

		private PictureBox picPreview;

		private Panel panel2;

		private Label label6;

		private Button btnRetry;

		private Label lblWarning;

		private Label lblDeviceID;

		private Label label3;

		private DataGridViewTextBoxColumn Type;

		private DataGridViewTextBoxColumn Content;

		private Button btnClear;

		private Label labelVersion;

		public Form2()
		{
			InitializeComponent();
			oNFC.TagStateListener = this;
			oNFC.ProcessStateListener = this;
			string path = "sample2.csv";
			if (File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				for (int i = 1; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(';');
					DataGridViewRowCollection rows = dataGridView1.Rows;
					object[] values = array2;
					rows.Add(values);
				}
			}
			DetectPort();
		}

		private void btnRetry_Click(object sender, EventArgs e)
		{
			DetectPort();
		}

		public void onTagState(NFCWrap.nTagState state)
		{
			Invoke((MethodInvoker)delegate
			{
				switch (state.ToString())
				{
				case "NFC_TAG_STATE_TAG_OFF":
					lblTagStatus.Text = "EPD off reader";
					break;
				case "NFC_TAG_STATE_TAG_ON":
					lblTagStatus.Text = "EPD on reader";
					break;
				case "NFC_TAG_STATE_COMM_ON":
					lblTagStatus.Text = "EPD ready";
					break;
				default:
					lblTagStatus.Text = state.ToString();
					break;
				}
				if (state.ToString() == "NFC_TAG_STATE_COMM_ON")
				{
					new Thread((ThreadStart)delegate
					{
						Thread.Sleep(100);
						string strDeviceID = oNFC.GetTagID();
						lblDeviceID.InvokeIfRequired(delegate
						{
							lblDeviceID.Text = strDeviceID;
						});
					}).Start();
					if (chkAutoRefresh.Checked)
					{
						new Thread((ThreadStart)delegate
						{
							Thread.Sleep(100);
							DrawImage2(0);
						}).Start();
					}
				}
				else
				{
					lblDeviceID.Text = "unknown";
				}
			});
		}

		public void onProcessState(NFCWrap.nImageState state, object data)
		{
			Invoke((MethodInvoker)delegate
			{
				lblProgress.Text = "Progress : " + data.ToString() + " %";
				if (state == NFCWrap.nImageState.DIState_Finish)
				{
					lblWarning.Visible = false;
					lblProgress.Visible = false;
					if (dataGridView1.Rows.Count > mRow)
					{
						dataGridView1.Rows.RemoveAt(mRow);
					}
					SystemSounds.Beep.Play();
				}
				else
				{
					if (!lblWarning.Visible)
					{
						SystemSounds.Hand.Play();
					}
					lblWarning.Visible = true;
					lblProgress.Visible = true;
				}
			});
		}

		private void btnImport_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "CSV Files(*.csv;)|*.csv";
			openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				string[] array = File.ReadAllLines(openFileDialog.FileName);
				for (int i = 1; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(';');
					DataGridViewRowCollection rows = dataGridView1.Rows;
					object[] values = array2;
					rows.Add(values);
				}
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			dataGridView1.Rows.Clear();
		}

		private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			mRow = e.RowIndex;
			DrawImage2(mRow);
		}

		private void DrawImage2(int dRow = 0)
		{
			Bitmap bitmap = new Bitmap(Convert.ToInt32(296), Convert.ToInt32(128), PixelFormat.Format32bppArgb);
			Graphics.FromImage(bitmap).Clear(Color.White);
			if (dataGridView1.Rows.Count > 0)
			{
				string a = dataGridView1.Rows[dRow].Cells[0].Value.ToString();
				if (!(a == "PRODUCT"))
				{
					if (a == "STORGE")
					{
						bitmap = DrawImage_STORGE(dRow);
					}
					else
					{
						Console.WriteLine("Nothing");
					}
				}
				else
				{
					bitmap = DrawImage_PRODUCT(dRow);
				}
			}
			picPreview.Image = bitmap;
			string text = oNFC.UnlockPinCode("0000");
			if (text == "0000")
			{
				text = oNFC.DrawImage(bitmap);
				Console.WriteLine(new NFCError(text).Content + "(at DrawImage)");
			}
			else
			{
				MessageBox.Show(new NFCError(text).Content + "(at UnlockPinCode)");
			}
		}

		private Bitmap DrawImage_PRODUCT(int dRow = 0)
		{
			int value = 296;
			int num = 128;
			mRow = dRow;
			mBitmap = new Bitmap(Convert.ToInt32(value), Convert.ToInt32(num), PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(mBitmap);
			graphics.Clear(Color.White);
			Font font = new Font("Arial", 8f);
			SolidBrush solidBrush = new SolidBrush(Color.Black);
			SolidBrush brush = new SolidBrush(Color.White);
			new StringFormat().FormatFlags = StringFormatFlags.NoWrap;
			new Pen(Color.Black, 1f);
			Point point = new Point(10, 10);
			Size size = new Size(100, 20);
			Rectangle rect = new Rectangle(point.X, point.Y, size.Width, size.Height);
			graphics.FillRectangle(solidBrush, rect);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			font = new Font("Arial", 10f, FontStyle.Bold);
			rect = new Rectangle(10, 10, size.Width, size.Height);
			graphics.DrawString("Advantech", font, brush, rect, stringFormat);
			DataGridViewRow dataGridViewRow = dataGridView1.Rows[dRow];
			dataGridViewRow.Cells[0].Value.ToString();
			dynamic val = JsonConvert.DeserializeObject<object>(dataGridViewRow.Cells[1].Value.ToString());
			Rectangle rectangle = new Rectangle(2, 2, 118, 16);
			rectangle = new Rectangle(10, 40, 160, 40);
			StringFormat stringFormat2 = new StringFormat();
			stringFormat2.FormatFlags = StringFormatFlags.LineLimit;
			graphics.DrawString(val["PROD NAME"].ToString(), font, solidBrush, rectangle, stringFormat2);
			Barcode barcode = new Barcode();
			barcode.IncludeLabel = true;
			barcode.LabelFont = new Font("Verdana", 4f);
			barcode.Width = 160;
			barcode.Height = 30;
			barcode.Alignment = AlignmentPositions.LEFT;
			Image image = (Image)barcode.Encode(TYPE.CODE128, val["BARCODE"].ToString(), barcode.Width, barcode.Height);
			Point point2 = new Point(point.X, num - barcode.Height);
			graphics.DrawImage(image, point2);
			font = new Font("Arial", 8f, FontStyle.Bold);
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Far;
			rectangle = new Rectangle(145, point.Y, 140, 20);
			graphics.DrawString(val["SPECUFUCATION"].ToString(), font, solidBrush, rectangle, stringFormat);
			font = new Font("Arial", 22f, FontStyle.Bold);
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Far;
			rectangle = new Rectangle(145, 60, 140, 50);
			graphics.DrawString(val["PRICE"].ToString(), font, solidBrush, rectangle, stringFormat);
			font = new Font("Arial", 8f, FontStyle.Bold);
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Far;
			rectangle = new Rectangle(145, 40, 140, 20);
			graphics.DrawString(val["RATE"].ToString(), font, solidBrush, rectangle, stringFormat);
			return mBitmap;
		}

		private Bitmap DrawImage_STORGE(int dRow = 0)
		{
			int num = 296;
			int num2 = 128;
			mRow = dRow;
			mBitmap = new Bitmap(Convert.ToInt32(num), Convert.ToInt32(num2), PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(mBitmap);
			graphics.Clear(Color.White);
			Font font = new Font("Arial", 8f);
			SolidBrush solidBrush = new SolidBrush(Color.Black);
			SolidBrush solidBrush2 = new SolidBrush(Color.White);
			new StringFormat().FormatFlags = StringFormatFlags.NoWrap;
			Pen pen = new Pen(Color.Black, 1f);
			Point point = new Point(10, 10);
			Size size = new Size(100, 20);
			Rectangle rect = new Rectangle(point.X, point.Y, size.Width, size.Height);
			graphics.FillRectangle(solidBrush, rect);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			font = new Font("Arial", 10f, FontStyle.Bold);
			rect = new Rectangle(10, 10, size.Width, size.Height);
			graphics.DrawString("Advantech", font, solidBrush2, rect, stringFormat);
			point = new Point(10, 40);
			size = new Size(num - 20, num2 - 50);
			rect = new Rectangle(point.X, point.Y, size.Width, size.Height);
			graphics.DrawRectangle(pen, rect);
			graphics.DrawLine(pen, point.X, point.Y + size.Height / 3, point.X + size.Width, point.Y + size.Height / 3);
			graphics.DrawLine(pen, point.X, point.Y + size.Height / 3 * 2, point.X + 180, point.Y + size.Height / 3 * 2);
			graphics.DrawLine(pen, point.X + 80, point.Y, point.X + 80, point.Y + size.Height);
			graphics.DrawLine(pen, point.X + 180, point.Y, point.X + 180, point.Y + size.Height);
			graphics.DrawLine(pen, point.X + 240, point.Y, point.X + 240, point.Y + size.Height);
			DataGridViewRow dataGridViewRow = dataGridView1.Rows[dRow];
			dataGridViewRow.Cells[0].Value.ToString();
			dynamic val = JsonConvert.DeserializeObject<object>(dataGridViewRow.Cells[1].Value.ToString());
			font = new Font("Arial", 10f, FontStyle.Bold);
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Far;
			Rectangle rectangle = new Rectangle(140, 10, num - 140 - 10, 20);
			graphics.DrawString(val["PART NO"].ToString(), font, solidBrush, rectangle, stringFormat);
			font = new Font("Arial", 8f, FontStyle.Bold);
			stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Center;
			stringFormat.LineAlignment = StringAlignment.Center;
			rectangle = new Rectangle(point.X, point.Y, 80, size.Height / 3);
			graphics.DrawString("Product", font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X, point.Y + size.Height / 3, 80, size.Height / 3);
			graphics.DrawString("Location", font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X, point.Y + size.Height / 3 * 2, 80, size.Height / 3);
			graphics.DrawString("Spec", font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X + 180, point.Y, 60, size.Height / 3);
			graphics.DrawString("Unit", font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X + 180, point.Y + size.Height / 3, 60, size.Height / 3 * 2);
			graphics.DrawString("Stock", font, solidBrush, rectangle, stringFormat);
			font = new Font("Arial", 10f, FontStyle.Bold);
			rectangle = new Rectangle(point.X + 80, point.Y, 100, size.Height / 3);
			graphics.DrawString(val["PART NAME"].ToString(), font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X + 80, point.Y + size.Height / 3, 100, size.Height / 3);
			graphics.DrawString(val["LOCATION"].ToString(), font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X + 80, point.Y + size.Height / 3 * 2, 100, size.Height / 3);
			graphics.DrawString(val["SPECUFUCATION"].ToString(), font, solidBrush, rectangle, stringFormat);
			rectangle = new Rectangle(point.X + 240, point.Y, 40, size.Height / 3);
			graphics.DrawString(val["UNIT"].ToString(), font, solidBrush, rectangle, stringFormat);
			font = new Font("Arial", 14f, FontStyle.Bold);
			rectangle = new Rectangle(point.X + 240, point.Y + size.Height / 3, 36, size.Height / 3 * 2);
			graphics.FillRectangle(solidBrush, rectangle);
			graphics.DrawString(val["INVENTORY"].ToString(), font, solidBrush2, rectangle, stringFormat);
			return mBitmap;
		}

		private void DetectPort()
		{
			lblPort.Text = "unknown";
			string port = oNFC.GetPort();
			if (port != "" && port != "0101")
			{
				lblPort.Text = port;
				oNFC = new NFCWrap(lblPort.Text);
				lblTagStatus.Text = "Port detected.";
				oNFC.ConnectTag();
			}
			else
			{
				MessageBox.Show("No correct port detected.");
			}
		}

		private void chkAutoRefresh_CheckedChanged(object sender, EventArgs e)
		{
			if (chkAutoRefresh.Checked)
			{
				DrawImage2(0);
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
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(NFCApp.Form2));
			lblProgress = new System.Windows.Forms.Label();
			btnImport = new System.Windows.Forms.Button();
			dataGridView1 = new System.Windows.Forms.DataGridView();
			Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Content = new System.Windows.Forms.DataGridViewTextBoxColumn();
			chkAutoRefresh = new System.Windows.Forms.CheckBox();
			panel1 = new System.Windows.Forms.Panel();
			lblDeviceID = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			btnRetry = new System.Windows.Forms.Button();
			lblTagStatus = new System.Windows.Forms.Label();
			lblPort = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			picPreview = new System.Windows.Forms.PictureBox();
			panel2 = new System.Windows.Forms.Panel();
			label6 = new System.Windows.Forms.Label();
			lblWarning = new System.Windows.Forms.Label();
			btnClear = new System.Windows.Forms.Button();
			labelVersion = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
			panel2.SuspendLayout();
			SuspendLayout();
			lblProgress.AutoSize = true;
			lblProgress.Font = new System.Drawing.Font("新細明體", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblProgress.ForeColor = System.Drawing.Color.RoyalBlue;
			lblProgress.Location = new System.Drawing.Point(674, 82);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(140, 21);
			lblProgress.TabIndex = 66;
			lblProgress.Text = "Progress : 0% ";
			lblProgress.Visible = false;
			btnImport.BackColor = System.Drawing.SystemColors.Window;
			btnImport.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnImport.Location = new System.Drawing.Point(197, 273);
			btnImport.Name = "btnImport";
			btnImport.Size = new System.Drawing.Size(133, 27);
			btnImport.TabIndex = 65;
			btnImport.Text = "Import Data";
			btnImport.UseVisualStyleBackColor = false;
			btnImport.Click += new System.EventHandler(btnImport_Click);
			dataGridView1.AllowUserToAddRows = false;
			dataGridView1.AllowUserToDeleteRows = false;
			dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Columns.AddRange(Type, Content);
			dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			dataGridView1.Location = new System.Drawing.Point(29, 316);
			dataGridView1.MultiSelect = false;
			dataGridView1.Name = "dataGridView1";
			dataGridView1.ReadOnly = true;
			dataGridView1.RowTemplate.Height = 24;
			dataGridView1.Size = new System.Drawing.Size(1043, 290);
			dataGridView1.TabIndex = 64;
			dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
			Type.HeaderText = "Type";
			Type.Name = "Type";
			Type.ReadOnly = true;
			Type.Width = 120;
			Content.HeaderText = "Content";
			Content.Name = "Content";
			Content.ReadOnly = true;
			Content.Width = 2000;
			chkAutoRefresh.AutoSize = true;
			chkAutoRefresh.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			chkAutoRefresh.Location = new System.Drawing.Point(29, 277);
			chkAutoRefresh.Name = "chkAutoRefresh";
			chkAutoRefresh.Size = new System.Drawing.Size(147, 20);
			chkAutoRefresh.TabIndex = 63;
			chkAutoRefresh.Text = "Auto refresh image";
			chkAutoRefresh.UseVisualStyleBackColor = true;
			chkAutoRefresh.CheckedChanged += new System.EventHandler(chkAutoRefresh_CheckedChanged);
			panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel1.Controls.Add(lblDeviceID);
			panel1.Controls.Add(label3);
			panel1.Controls.Add(btnRetry);
			panel1.Controls.Add(lblTagStatus);
			panel1.Controls.Add(lblPort);
			panel1.Controls.Add(label1);
			panel1.Controls.Add(label2);
			panel1.Location = new System.Drawing.Point(29, 28);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(1043, 47);
			panel1.TabIndex = 62;
			lblDeviceID.AutoSize = true;
			lblDeviceID.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblDeviceID.ForeColor = System.Drawing.Color.RoyalBlue;
			lblDeviceID.Location = new System.Drawing.Point(866, 14);
			lblDeviceID.Name = "lblDeviceID";
			lblDeviceID.Size = new System.Drawing.Size(74, 16);
			lblDeviceID.TabIndex = 69;
			lblDeviceID.Text = "unknown";
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label3.Location = new System.Drawing.Point(796, 14);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(64, 16);
			label3.TabIndex = 68;
			label3.Text = "EPD ID: ";
			btnRetry.BackColor = System.Drawing.SystemColors.Window;
			btnRetry.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnRetry.Location = new System.Drawing.Point(175, 9);
			btnRetry.Name = "btnRetry";
			btnRetry.Size = new System.Drawing.Size(50, 27);
			btnRetry.TabIndex = 67;
			btnRetry.Text = "Retry";
			btnRetry.UseVisualStyleBackColor = false;
			btnRetry.Click += new System.EventHandler(btnRetry_Click);
			lblTagStatus.AutoSize = true;
			lblTagStatus.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblTagStatus.ForeColor = System.Drawing.Color.RoyalBlue;
			lblTagStatus.Location = new System.Drawing.Point(363, 14);
			lblTagStatus.Name = "lblTagStatus";
			lblTagStatus.Size = new System.Drawing.Size(74, 16);
			lblTagStatus.TabIndex = 52;
			lblTagStatus.Text = "unknown";
			lblPort.AutoSize = true;
			lblPort.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			lblPort.Location = new System.Drawing.Point(102, 14);
			lblPort.Name = "lblPort";
			lblPort.Size = new System.Drawing.Size(67, 16);
			lblPort.TabIndex = 56;
			lblPort.Text = "unknown";
			label1.AutoSize = true;
			label1.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label1.Location = new System.Drawing.Point(307, 14);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(50, 16);
			label1.TabIndex = 53;
			label1.Text = "State : ";
			label2.AutoSize = true;
			label2.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label2.Location = new System.Drawing.Point(18, 14);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(78, 16);
			label2.TabIndex = 55;
			label2.Text = "USB Port : ";
			picPreview.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreview.Location = new System.Drawing.Point(356, 24);
			picPreview.Name = "picPreview";
			picPreview.Size = new System.Drawing.Size(296, 128);
			picPreview.TabIndex = 67;
			picPreview.TabStop = false;
			panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel2.Controls.Add(lblProgress);
			panel2.Controls.Add(label6);
			panel2.Controls.Add(picPreview);
			panel2.Location = new System.Drawing.Point(29, 95);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(1043, 168);
			panel2.TabIndex = 63;
			label6.AutoSize = true;
			label6.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label6.Location = new System.Drawing.Point(227, 82);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(113, 16);
			label6.TabIndex = 53;
			label6.Text = "Image Preview : ";
			lblWarning.AutoSize = true;
			lblWarning.Font = new System.Drawing.Font("新細明體", 36f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblWarning.ForeColor = System.Drawing.Color.Red;
			lblWarning.Location = new System.Drawing.Point(189, 266);
			lblWarning.Name = "lblWarning";
			lblWarning.Size = new System.Drawing.Size(743, 48);
			lblWarning.TabIndex = 67;
			lblWarning.Text = "Do not remove EPD until finished !!!";
			lblWarning.Visible = false;
			btnClear.BackColor = System.Drawing.SystemColors.Window;
			btnClear.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnClear.Location = new System.Drawing.Point(340, 273);
			btnClear.Name = "btnClear";
			btnClear.Size = new System.Drawing.Size(133, 27);
			btnClear.TabIndex = 68;
			btnClear.Text = "Clear Data";
			btnClear.UseVisualStyleBackColor = false;
			btnClear.Click += new System.EventHandler(btnClear_Click);
			labelVersion.AutoSize = true;
			labelVersion.Location = new System.Drawing.Point(384, 623);
			labelVersion.Name = "labelVersion";
			labelVersion.Size = new System.Drawing.Size(272, 12);
			labelVersion.TabIndex = 74;
			labelVersion.Text = "Version 1.3.2 ©2023 Advantech corp All rights reserved.";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			BackColor = System.Drawing.SystemColors.Window;
			base.ClientSize = new System.Drawing.Size(1100, 644);
			base.Controls.Add(labelVersion);
			base.Controls.Add(lblWarning);
			base.Controls.Add(btnClear);
			base.Controls.Add(panel2);
			base.Controls.Add(btnImport);
			base.Controls.Add(dataGridView1);
			base.Controls.Add(chkAutoRefresh);
			base.Controls.Add(panel1);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "Form2";
			base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "EPD210 NFC - Advantech";
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
	public static class Extension
	{
		public static void InvokeIfRequired(this Control control, MethodInvoker action)
		{
			if (control != null)
			{
				if (control.InvokeRequired)
				{
					control.Invoke(action);
				}
				else
				{
					action();
				}
			}
		}
	}
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form2());
		}
	}
}
namespace NFCApp.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("NFCApp.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal Resources()
		{
		}
	}
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;
	}
}
