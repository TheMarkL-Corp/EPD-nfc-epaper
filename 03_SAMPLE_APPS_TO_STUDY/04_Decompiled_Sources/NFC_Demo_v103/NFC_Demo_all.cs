using AdvNFCWrap;
using Newtonsoft.Json;
using QRCoder;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using WebSocketSharp;
using WebSocketSharp.Server;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("NFC_Demo")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("NFC_Demo")]
[assembly: AssemblyCopyright("Copyright ©  2024")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("57182e55-a13f-4c0c-825c-9fefb758e7a8")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.6.1", FrameworkDisplayName = ".NET Framework 4.6.1")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace NFC_Demo
{
	public class Form1 : Form, NFCWrap.TagState, NFCWrap.ProcessState
	{
		private class NotifyBehavior : WebSocketBehavior
		{
			private readonly Action<string> handleMessageReceived;

			public NotifyBehavior(Action<string> handleMessageReceived)
			{
				this.handleMessageReceived = handleMessageReceived;
			}

			protected override void OnOpen()
			{
				Console.WriteLine("WebSocket OnOpen ");
				Send(JsonConvert.SerializeObject(new
				{
					message = "Open"
				}));
			}

			protected override void OnMessage(MessageEventArgs e)
			{
				Console.WriteLine("WebSocket OnMessage " + e.Data);
				handleMessageReceived?.Invoke(e.Data);
				if (e.Data.Equals("reset"))
				{
					Send(JsonConvert.SerializeObject(new
					{
						message = "OK"
					}));
				}
			}

			protected override void OnClose(CloseEventArgs e)
			{
				Console.WriteLine("WebSocket OnClose ");
			}
		}

		private NFCWrap oNFC = new NFCWrap();

		private Bitmap mBitmap;

		private int mRow;

		private bool isTransmiting = false;

		private string localIP;

		private Configuration config = null;

		private WebSocket ws = null;

		public Form1 _FORM;

		private IContainer components = null;

		private Panel panel1;

		private Label lblDeviceID;

		private Label label3;

		private Button btnRetry;

		private Label lblTagStatus;

		private Label lblPort;

		private Label label1;

		private Label label2;

		private Panel panel2;

		private Label lblProgress;

		private Label label6;

		private PictureBox picPreview;

		private DataGridView dataGridView2;

		private Label lblWarning;

		private Button btnImport;

		private DataGridViewTextBoxColumn Part_Number;

		private DataGridViewTextBoxColumn Storage_Bin;

		private DataGridViewTextBoxColumn Description;

		private DataGridViewTextBoxColumn Cost;

		private DataGridViewTextBoxColumn Storage;

		private DataGridViewTextBoxColumn Request_Qty;

		private DataGridViewTextBoxColumn Storage_Qty;

		private DataGridViewTextBoxColumn Applicant;

		private DataGridViewTextBoxColumn Updated_Time;

		private Button btnUpImage;

		private Button btnAddRow;

		private Button btnUpload;

		private Button btnReset;

		private CheckBox chkDithering;

		private Label label20;

		public WebSocketServer webSocketServer
		{
			get;
			private set;
		}

		public Form1()
		{
			InitializeComponent();
			_FORM = this;
			string str = Directory.GetCurrentDirectory() + "/";
			using (StreamReader streamReader = new StreamReader(str + "config.json"))
			{
				string value = streamReader.ReadToEnd();
				config = JsonConvert.DeserializeObject<Configuration>(value);
			}
			oNFC.TagStateListener = this;
			oNFC.ProcessStateListener = this;
			dataGridView2.EditMode = DataGridViewEditMode.EditOnEnter;
			dataGridView2.ReadOnly = false;
			string path = "sample.csv";
			if (File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				for (int i = 1; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(',');
					DataGridViewRowCollection rows = dataGridView2.Rows;
					object[] values = array2;
					rows.Add(values);
				}
			}
			DetectPort();
			localIP = GetIP();
			newWebSocket();
		}

		private string GetIP()
		{
			string hostName = Dns.GetHostName();
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
			IPAddress iPAddress = hostAddresses.FirstOrDefault((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork);
			if (iPAddress == null)
			{
				Console.WriteLine("找不到本地 IPv4 位址");
				return null;
			}
			Console.WriteLine("本地 IPv4 位址: " + iPAddress.ToString());
			return iPAddress.ToString();
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
					Thread thread = new Thread((ThreadStart)delegate
					{
						Thread.Sleep(100);
						string strDeviceID = oNFC.GetTagID();
						lblDeviceID.InvokeIfRequired(delegate
						{
							lblDeviceID.Text = strDeviceID;
						});
					});
					thread.Start();
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
					isTransmiting = false;
					lblWarning.Visible = false;
					lblProgress.Visible = false;
					btnUpImage.Enabled = true;
					btnImport.Enabled = true;
					btnAddRow.Enabled = true;
					btnReset.Enabled = true;
					btnUpload.Enabled = true;
					if (dataGridView2.Rows.Count > mRow && mRow > 0)
					{
						dataGridView2.Rows.RemoveAt(mRow);
					}
					SystemSounds.Beep.Play();
				}
				else
				{
					if (!lblWarning.Visible)
					{
						SystemSounds.Hand.Play();
					}
					btnUpImage.Enabled = false;
					btnImport.Enabled = false;
					btnAddRow.Enabled = false;
					btnReset.Enabled = false;
					btnUpload.Enabled = false;
					lblWarning.Visible = true;
					lblProgress.Visible = true;
				}
			});
		}

		public async void ResetImage()
		{
			mRow = -1;
			Bitmap mBitmap = new Bitmap(Convert.ToInt32(416), Convert.ToInt32(240), PixelFormat.Format32bppArgb);
			Graphics g = Graphics.FromImage(mBitmap);
			g.Clear(Color.White);
			picPreview.Image = mBitmap;
			UpdateTextBox("Picking finish!!");
			if (isTransmiting)
			{
				MessageBox.Show("EPD Transmiting, Please wati!");
			}
			else
			{
				string strReturn = oNFC.UnlockPinCode("0000");
				if (strReturn == NFCError.NFC_MSG_SUCCESS)
				{
					isTransmiting = true;
					Console.WriteLine(new NFCError(await oNFC.DrawImageAsync(mBitmap)).Content + "(at DrawImage)");
				}
				else
				{
					MessageBox.Show(new NFCError(strReturn).Content + "(at UnlockPinCode)");
				}
			}
		}

		private void UpdateTextBox(string message)
		{
			if (lblWarning.InvokeRequired)
			{
				lblWarning.Invoke(new Action<string>(UpdateTextBox), message);
			}
			else
			{
				Console.WriteLine("Test log: " + message);
				lblWarning.Text = message;
			}
		}

		private async void PreviewImage(int dRow = 0)
		{
			if (isTransmiting)
			{
				MessageBox.Show("EPD Transmiting, Please wati!");
			}
			else
			{
				Bitmap mBitmap = new Bitmap(Convert.ToInt32(416), Convert.ToInt32(240), PixelFormat.Format32bppArgb);
				Graphics g = Graphics.FromImage(mBitmap);
				g.Clear(Color.White);
				if (dataGridView2.Rows.Count > 0)
				{
					DataGridViewRow dataGridViewRow = dataGridView2.Rows[dRow];
					mBitmap = DrawImage_Storge(dRow);
				}
				if (mBitmap != null)
				{
					picPreview.Image = mBitmap;
				}
			}
		}

		private async void DrawImage()
		{
			if (isTransmiting)
			{
				MessageBox.Show("EPD Transmiting, Please wati!");
			}
			else
			{
				string strReturn = oNFC.UnlockPinCode("0000");
				if (strReturn == NFCError.NFC_MSG_SUCCESS)
				{
					isTransmiting = true;
					Console.WriteLine(new NFCError(await oNFC.DrawImageAsync(mBitmap, chkDithering.Checked)).Content + "(at DrawImage)");
				}
				else
				{
					MessageBox.Show(new NFCError(strReturn).Content + "(at UnlockPinCode)");
				}
			}
		}

		private Bitmap DrawImage_Storge(int dRow = 0)
		{
			int num = 416;
			int value = 240;
			mRow = dRow;
			mBitmap = new Bitmap(Convert.ToInt32(num), Convert.ToInt32(value), PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(mBitmap);
			graphics.Clear(Color.White);
			Font font = new Font("Arial", 8f);
			SolidBrush brush = new SolidBrush(Color.Black);
			SolidBrush solidBrush = new SolidBrush(Color.White);
			SolidBrush brush2 = new SolidBrush(Color.Red);
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			Pen pen = new Pen(Color.Black, 2f);
			Point point = new Point(100, 40);
			Size size = new Size(90, 90);
			Rectangle rectangle = new Rectangle(point.X, point.Y, size.Width, size.Height);
			StringFormat stringFormat2 = new StringFormat();
			stringFormat2.Alignment = StringAlignment.Center;
			stringFormat2.LineAlignment = StringAlignment.Center;
			DataGridViewRow dataGridViewRow = dataGridView2.Rows[dRow];
			string text = dataGridViewRow.Cells[0].Value.ToString();
			string text2 = dataGridViewRow.Cells[1].Value.ToString();
			string text3 = dataGridViewRow.Cells[2].Value.ToString();
			string text4 = dataGridViewRow.Cells[3].Value.ToString();
			string text5 = dataGridViewRow.Cells[4].Value.ToString();
			string text6 = dataGridViewRow.Cells[5].Value.ToString();
			string text7 = dataGridViewRow.Cells[6].Value.ToString();
			string text8 = dataGridViewRow.Cells[7].Value.ToString();
			string text9 = dataGridViewRow.Cells[8].Value.ToString();
			Rectangle rectangle2 = new Rectangle(2, 2, 118, 16);
			rectangle2 = new Rectangle(10, 10, 160, 40);
			StringFormat stringFormat3 = new StringFormat();
			font = new Font("Arial", 16f, FontStyle.Bold);
			stringFormat3.FormatFlags = StringFormatFlags.LineLimit;
			graphics.DrawString(text, font, brush2, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(15, 40, 400, 40);
			font = new Font("Arial", 8f);
			graphics.DrawString(text3, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 65, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Storage Bin", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 80, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text2, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 100, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Storage ", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 115, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text5, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 135, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Cost ", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(200, 150, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text4, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 65, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Request Quantity", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 80, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text6, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 100, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Storage Quantity", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 115, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text7, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 135, 100, 20);
			font = new Font("Arial", 8f);
			graphics.DrawString("Applicant ", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(300, 150, 100, 20);
			font = new Font("Arial", 10f, FontStyle.Bold);
			graphics.DrawString(text8, font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(250, 210, 100, 40);
			font = new Font("Arial", 8f);
			stringFormat3.FormatFlags = StringFormatFlags.LineLimit;
			graphics.DrawString("Update Time :", font, brush, rectangle2, stringFormat3);
			rectangle2 = new Rectangle(330, 210, 100, 40);
			font = new Font("Arial", 8f);
			stringFormat3.FormatFlags = StringFormatFlags.LineLimit;
			graphics.DrawString(text9, font, brush, rectangle2, stringFormat3);
			var value2 = new
			{
				No = text,
				Stg_B = text2,
				Desc = text3,
				Cost = text4,
				Stg = text5,
				Rq_Qt = text6,
				Stg_Qt = text7,
				APPL = text8,
				Up_Time = text9,
				wsIP = localIP
			};
			string text10 = JsonConvert.SerializeObject(value2);
			string text11 = config.ip + ":8081/list/";
			try
			{
				Console.WriteLine(text10);
				string str = EncryptAES(text10);
				text11 += HttpUtility.UrlEncode(str);
			}
			catch (Exception value3)
			{
				Console.WriteLine(value3);
			}
			Console.WriteLine(text11);
			QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
			QRCodeData data = qRCodeGenerator.CreateQrCode(text11, QRCodeGenerator.ECCLevel.Q, false, false, QRCodeGenerator.EciMode.Default, -1);
			QRCode qRCode = new QRCode(data);
			Bitmap graphic = qRCode.GetGraphic(20, Color.Black, Color.White, true);
			Bitmap image = new Bitmap(170, 170);
			using (Graphics graphics2 = Graphics.FromImage(image))
			{
				graphics2.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics2.DrawImage(graphic, 0, 0, 170, 170);
			}
			graphics.DrawImage(image, new Point(10, 65));
			point = new Point(15, 60);
			size = new Size(num - 150, 100);
			rectangle = new Rectangle(point.X, point.Y, size.Width, size.Height);
			graphics.DrawLine(pen, point.X, point.Y, 400, point.Y);
			return mBitmap;
		}

		public static string EncryptAES(string toEncrypt)
		{
			byte[] bytes = Encoding.UTF8.GetBytes("P@ssw0rd123456789012345678901234");
			byte[] bytes2 = Encoding.UTF8.GetBytes(toEncrypt);
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = bytes;
			rijndaelManaged.Mode = CipherMode.ECB;
			rijndaelManaged.Padding = PaddingMode.PKCS7;
			ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor();
			byte[] array = cryptoTransform.TransformFinalBlock(bytes2, 0, bytes2.Length);
			return Convert.ToBase64String(array, 0, array.Length);
		}

		private void DetectPort()
		{
			lblPort.Text = "unknown";
			string port = oNFC.GetPort();
			if (port != "0101")
			{
				lblPort.Text = port;
				oNFC = new NFCWrap(lblPort.Text);
				lblTagStatus.Text = "Port detected.";
				string text = oNFC.ConnectTag();
			}
			else
			{
				MessageBox.Show("No correct port detected.");
			}
		}

		private void newWebSocket()
		{
			webSocketServer = new WebSocketServer(8082);
			webSocketServer.AddWebSocketService("/notify", () => new NotifyBehavior(HandleMessageReceived));
			try
			{
				Console.WriteLine(webSocketServer.Port);
				webSocketServer.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine("newWebSocket error : " + ex.Message);
			}
		}

		private void HandleMessageReceived(string message)
		{
			Console.WriteLine("HandleMessageReceived : " + message);
			if ("reset".Equals(message))
			{
				ResetImage();
			}
		}

		private void btnRetry_Click(object sender, EventArgs e)
		{
			DetectPort();
		}

		private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			mRow = e.RowIndex;
			PreviewImage(mRow);
		}

		private void btnUpImage_Click(object sender, EventArgs e)
		{
			lblWarning.Text = "Picking list displaying";
			if (picPreview.Image == null)
			{
				mRow = 0;
				PreviewImage(0);
			}
			DrawImage();
		}

		private void btnAddRow_Click(object sender, EventArgs e)
		{
			string[] array = new string[9]
			{
				"Part_Number",
				"Storage_Bin",
				"Description",
				"Cost",
				"Storage",
				"Request_Qty",
				"Storage_Qty",
				"Applicant",
				"Updated_Time"
			};
			DataGridViewRowCollection rows = dataGridView2.Rows;
			object[] values = array;
			rows.Add(values);
		}

		private void btnUpload_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Image Files(*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png; *.jpg; *.jpeg; *.gif; *.bmp";
			openFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\images";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				Bitmap bitmap = new Bitmap(openFileDialog.FileName);
				if (bitmap.Width < bitmap.Height)
				{
					bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
				}
				Bitmap image = NFCWrap.resizeImage(bitmap, new Size(416, 240));
				picPreview.Image = image;
				mBitmap = image;
			}
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			mRow = -1;
			mBitmap = new Bitmap(Convert.ToInt32(416), Convert.ToInt32(240), PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(mBitmap);
			graphics.Clear(Color.White);
			picPreview.Image = mBitmap;
			UpdateTextBox("Celan Screen!!");
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
					string[] array2 = array[i].Split(',');
					DataGridViewRowCollection rows = dataGridView2.Rows;
					object[] values = array2;
					rows.Add(values);
				}
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
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(NFC_Demo.Form1));
			panel1 = new System.Windows.Forms.Panel();
			lblDeviceID = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			btnRetry = new System.Windows.Forms.Button();
			lblTagStatus = new System.Windows.Forms.Label();
			lblPort = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			panel2 = new System.Windows.Forms.Panel();
			chkDithering = new System.Windows.Forms.CheckBox();
			btnUpload = new System.Windows.Forms.Button();
			btnReset = new System.Windows.Forms.Button();
			btnUpImage = new System.Windows.Forms.Button();
			lblProgress = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			picPreview = new System.Windows.Forms.PictureBox();
			dataGridView2 = new System.Windows.Forms.DataGridView();
			Part_Number = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Storage_Bin = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Cost = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Storage = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Request_Qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Storage_Qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Applicant = new System.Windows.Forms.DataGridViewTextBoxColumn();
			Updated_Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
			lblWarning = new System.Windows.Forms.Label();
			btnImport = new System.Windows.Forms.Button();
			btnAddRow = new System.Windows.Forms.Button();
			label20 = new System.Windows.Forms.Label();
			panel1.SuspendLayout();
			panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
			((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
			SuspendLayout();
			panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel1.Controls.Add(lblDeviceID);
			panel1.Controls.Add(label3);
			panel1.Controls.Add(btnRetry);
			panel1.Controls.Add(lblTagStatus);
			panel1.Controls.Add(lblPort);
			panel1.Controls.Add(label1);
			panel1.Controls.Add(label2);
			panel1.Location = new System.Drawing.Point(12, 12);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(847, 47);
			panel1.TabIndex = 63;
			lblDeviceID.AutoSize = true;
			lblDeviceID.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblDeviceID.ForeColor = System.Drawing.Color.RoyalBlue;
			lblDeviceID.Location = new System.Drawing.Point(633, 14);
			lblDeviceID.Name = "lblDeviceID";
			lblDeviceID.Size = new System.Drawing.Size(74, 16);
			lblDeviceID.TabIndex = 69;
			lblDeviceID.Text = "unknown";
			label3.AutoSize = true;
			label3.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label3.Location = new System.Drawing.Point(563, 14);
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
			panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panel2.Controls.Add(chkDithering);
			panel2.Controls.Add(btnUpload);
			panel2.Controls.Add(btnReset);
			panel2.Controls.Add(btnUpImage);
			panel2.Controls.Add(lblProgress);
			panel2.Controls.Add(label6);
			panel2.Controls.Add(picPreview);
			panel2.Location = new System.Drawing.Point(12, 80);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(847, 291);
			panel2.TabIndex = 64;
			chkDithering.AutoSize = true;
			chkDithering.Location = new System.Drawing.Point(652, 124);
			chkDithering.Name = "chkDithering";
			chkDithering.Size = new System.Drawing.Size(68, 16);
			chkDithering.TabIndex = 75;
			chkDithering.Text = "Dithering";
			chkDithering.UseVisualStyleBackColor = true;
			btnUpload.BackColor = System.Drawing.SystemColors.Window;
			btnUpload.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnUpload.Location = new System.Drawing.Point(652, 58);
			btnUpload.Name = "btnUpload";
			btnUpload.Size = new System.Drawing.Size(133, 27);
			btnUpload.TabIndex = 74;
			btnUpload.Text = "Upload File";
			btnUpload.UseVisualStyleBackColor = false;
			btnUpload.Click += new System.EventHandler(btnUpload_Click);
			btnReset.BackColor = System.Drawing.SystemColors.Window;
			btnReset.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnReset.Location = new System.Drawing.Point(652, 91);
			btnReset.Name = "btnReset";
			btnReset.Size = new System.Drawing.Size(133, 27);
			btnReset.TabIndex = 73;
			btnReset.Text = "Clean Screen";
			btnReset.UseVisualStyleBackColor = false;
			btnReset.Click += new System.EventHandler(btnReset_Click);
			btnUpImage.BackColor = System.Drawing.Color.Green;
			btnUpImage.Font = new System.Drawing.Font("新細明體", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			btnUpImage.ForeColor = System.Drawing.Color.Transparent;
			btnUpImage.Location = new System.Drawing.Point(652, 25);
			btnUpImage.Name = "btnUpImage";
			btnUpImage.Size = new System.Drawing.Size(133, 27);
			btnUpImage.TabIndex = 72;
			btnUpImage.Text = "Push Image";
			btnUpImage.UseVisualStyleBackColor = false;
			btnUpImage.Click += new System.EventHandler(btnUpImage_Click);
			lblProgress.AutoSize = true;
			lblProgress.Font = new System.Drawing.Font("新細明體", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 136);
			lblProgress.ForeColor = System.Drawing.Color.RoyalBlue;
			lblProgress.Location = new System.Drawing.Point(648, 244);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(140, 21);
			lblProgress.TabIndex = 66;
			lblProgress.Text = "Progress : 0% ";
			lblProgress.Visible = false;
			label6.AutoSize = true;
			label6.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			label6.Location = new System.Drawing.Point(18, 140);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(113, 16);
			label6.TabIndex = 53;
			label6.Text = "Image Preview : ";
			picPreview.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picPreview.Location = new System.Drawing.Point(211, 25);
			picPreview.Name = "picPreview";
			picPreview.Size = new System.Drawing.Size(416, 240);
			picPreview.TabIndex = 67;
			picPreview.TabStop = false;
			dataGridView2.AllowUserToAddRows = false;
			dataGridView2.AllowUserToDeleteRows = false;
			dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView2.Columns.AddRange(Part_Number, Storage_Bin, Description, Cost, Storage, Request_Qty, Storage_Qty, Applicant, Updated_Time);
			dataGridView2.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			dataGridView2.Location = new System.Drawing.Point(12, 442);
			dataGridView2.MultiSelect = false;
			dataGridView2.Name = "dataGridView2";
			dataGridView2.ReadOnly = true;
			dataGridView2.RowTemplate.Height = 24;
			dataGridView2.Size = new System.Drawing.Size(847, 290);
			dataGridView2.TabIndex = 74;
			dataGridView2.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView2_CellContentClick);
			Part_Number.HeaderText = "Part_Number";
			Part_Number.Name = "Part_Number";
			Part_Number.ReadOnly = true;
			Storage_Bin.HeaderText = "Storage_Bin";
			Storage_Bin.Name = "Storage_Bin";
			Storage_Bin.ReadOnly = true;
			Description.HeaderText = "Description";
			Description.Name = "Description";
			Description.ReadOnly = true;
			Description.Width = 300;
			Cost.HeaderText = "Cost";
			Cost.Name = "Cost";
			Cost.ReadOnly = true;
			Storage.HeaderText = "Storage";
			Storage.Name = "Storage";
			Storage.ReadOnly = true;
			Request_Qty.HeaderText = "Request_Qty";
			Request_Qty.Name = "Request_Qty";
			Request_Qty.ReadOnly = true;
			Storage_Qty.HeaderText = "Storage_Qty";
			Storage_Qty.Name = "Storage_Qty";
			Storage_Qty.ReadOnly = true;
			Applicant.HeaderText = "Applicant";
			Applicant.Name = "Applicant";
			Applicant.ReadOnly = true;
			Updated_Time.HeaderText = "Updated_Time";
			Updated_Time.Name = "Updated_Time";
			Updated_Time.ReadOnly = true;
			lblWarning.AutoSize = true;
			lblWarning.Font = new System.Drawing.Font("新細明體", 24f, System.Drawing.FontStyle.Bold);
			lblWarning.ForeColor = System.Drawing.Color.Red;
			lblWarning.Location = new System.Drawing.Point(301, 394);
			lblWarning.Name = "lblWarning";
			lblWarning.Size = new System.Drawing.Size(500, 32);
			lblWarning.TabIndex = 72;
			lblWarning.Text = "Do not remove EPD until finished !!!";
			lblWarning.Visible = false;
			btnImport.BackColor = System.Drawing.SystemColors.Window;
			btnImport.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnImport.Location = new System.Drawing.Point(151, 399);
			btnImport.Name = "btnImport";
			btnImport.Size = new System.Drawing.Size(133, 27);
			btnImport.TabIndex = 71;
			btnImport.Text = "Import Data";
			btnImport.UseVisualStyleBackColor = false;
			btnImport.Click += new System.EventHandler(btnImport_Click);
			btnAddRow.BackColor = System.Drawing.SystemColors.Window;
			btnAddRow.Font = new System.Drawing.Font("新細明體", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 136);
			btnAddRow.Location = new System.Drawing.Point(12, 399);
			btnAddRow.Name = "btnAddRow";
			btnAddRow.Size = new System.Drawing.Size(133, 27);
			btnAddRow.TabIndex = 75;
			btnAddRow.Text = "Add Row";
			btnAddRow.UseVisualStyleBackColor = false;
			btnAddRow.Click += new System.EventHandler(btnAddRow_Click);
			label20.AutoSize = true;
			label20.Location = new System.Drawing.Point(305, 739);
			label20.Name = "label20";
			label20.Size = new System.Drawing.Size(272, 12);
			label20.TabIndex = 76;
			label20.Text = "Version 1.0.3 ©2024 Advantech corp All rights reserved.";
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(871, 760);
			base.Controls.Add(label20);
			base.Controls.Add(btnAddRow);
			base.Controls.Add(dataGridView2);
			base.Controls.Add(lblWarning);
			base.Controls.Add(btnImport);
			base.Controls.Add(panel2);
			base.Controls.Add(panel1);
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Name = "Form1";
			Text = "NFC_Demo";
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
			((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}
	}
	internal class Configuration
	{
		public string ip
		{
			get;
			set;
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
			Application.Run(new Form1());
		}
	}
}
namespace NFC_Demo.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
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
					ResourceManager resourceManager = resourceMan = new ResourceManager("NFC_Demo.Properties.Resources", typeof(Resources).Assembly);
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
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;
	}
}
