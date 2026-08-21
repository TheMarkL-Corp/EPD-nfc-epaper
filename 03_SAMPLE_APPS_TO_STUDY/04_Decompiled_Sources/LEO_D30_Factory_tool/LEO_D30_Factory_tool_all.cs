using com.advantech.nfc;
using com.advantech.nfc.cmd;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Timers;
using System.Windows.Forms;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyTitle("EPD30x_Factory_tool")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("EPD30x_Factory_tool")]
[assembly: AssemblyCopyright("Copyright ©  2018")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("6e752dbc-12cd-42e6-a0e0-24764d7f2af2")]
[assembly: TargetFramework(".NETFramework,Version=v4.6.1", FrameworkDisplayName = ".NET Framework 4.6.1")]
[assembly: AssemblyVersion("1.0.9319.25993")]
namespace LEOD30_Factory_tool
{
	public class Form1 : Form, NFCTagChangeListener, IDrawImageCallback
	{
		private NFCManager manager;

		private INFCEDPAPI api;

		private INFCCommand nfc;

		private NFCTagState curentState;

		private float spendTime;

		private string vername;

		private System.Timers.Timer spendtimer = new System.Timers.Timer();

		private IContainer components;

		private ImageList imageList1;

		private Panel panel1;

		private PictureBox pictureBox1;

		private Panel panel2;

		private TextBox textBox4;

		private ComboBox comboBoxPorts;

		private Button btnUpdate;

		private ProgressBar progressBar1;

		private Label lblProgress;

		private Label lblSpendTime;

		private Label slblCount;

		private Button btnExplore;

		private TextBox txtBoxPath;

		private PictureBox picboxPreview;

		public Form1()
		{
			InitializeComponent();
			AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
			Text = $"{assemblyTitleAttribute.Title} Ver:{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}";
			manager = NFCManager.getInstance();
			manager.TagChange = this;
			pictureBox1.Image = imageList1.Images[0];
			btnUpdate.Enabled = (curentState == NFCTagState.NFC_TAG_STATE_COMM_ON);
			spendtimer.Interval = 100.0;
			spendtimer.Elapsed += spendtimer_Elapsed;
			spendtimer.Stop();
		}

		private void spendtimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Invoke((MethodInvoker)delegate
			{
				spendTime += 1f;
				lblSpendTime.Text = string.Concat(spendTime / 10f);
				lblSpendTime.Update();
			});
		}

		public void GetTagID()
		{
			byte[] tagID = api.getTagID();
			string text = "";
			if (tagID == null)
			{
				text = "??";
			}
			else
			{
				byte[] array = tagID;
				foreach (byte b in array)
				{
					text += $"{b:X2}";
				}
			}
			textBox4.Text = text;
		}

		public void onTagStateChange(NFCTagState state)
		{
			Invoke((MethodInvoker)delegate
			{
				curentState = state;
				switch (state)
				{
				case NFCTagState.NFC_TAG_STATE_TAG_OFF:
					pictureBox1.Image = imageList1.Images[0];
					break;
				case NFCTagState.NFC_TAG_STATE_TAG_ON:
					pictureBox1.Image = imageList1.Images[1];
					GetTagID();
					break;
				case NFCTagState.NFC_TAG_STATE_COMM_ON:
					pictureBox1.Image = imageList1.Images[2];
					break;
				}
				btnUpdate.Enabled = (curentState == NFCTagState.NFC_TAG_STATE_COMM_ON);
			});
		}

		private void UpdateButtons(bool isReadyForTest)
		{
			btnUpdate.Enabled = isReadyForTest;
			comboBoxPorts.Enabled = isReadyForTest;
		}

		private void unLock()
		{
			byte[] data = new byte[4]
			{
				48,
				48,
				48,
				48
			};
			if (!api.UnlockPinCode(data))
			{
				throw new Exception("Unlock Fail");
			}
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			if (txtBoxPath.Text == string.Empty)
			{
				MessageBox.Show("Please select an image to upload first!");
			}
			else
			{
				UpdateButtons(false);
				unLock();
				spendTime = 0f;
				spendtimer.Start();
				showImage();
			}
		}

		private void showImage()
		{
			try
			{
				Bitmap bitmap = new Bitmap(txtBoxPath.Text);
				string platformName = manager._epd_api.GetPlatformName();
				vername = manager._epd_api.GetVersion();
				string[] array = vername.Split('.');
				int num;
				int num3;
				int num2;
				int num4 = num3 = (num2 = (num = 0));
				if (array.Length == 2)
				{
					num3 = int.Parse(array[0]);
					num2 = int.Parse(array[1]);
				}
				else
				{
					num3 = int.Parse(array[0]);
					num2 = int.Parse(array[1]);
					num = int.Parse(array[2]);
				}
				num4 = num3 * 100 + num2 * 10 + num;
				int num5 = 1;
				int iwidthedge = Epd29.iwidthedge;
				int iheightedge = Epd29.iheightedge;
				int num6 = 1;
				int lz4packsize = 0;
				if (platformName.Equals("EPD-210--TC2"))
				{
					if (num4 < 100)
					{
						num5 = 0;
					}
					else if (num4 >= 400)
					{
						num5 = 1;
						lz4packsize = 4096;
					}
					else
					{
						num5 = 1;
						lz4packsize = 1024;
					}
					num6 = 1;
				}
				else if (platformName.Equals("D30-ED29-TC2") || platformName.Equals("D30-EL29-TC2"))
				{
					num5 = 0;
					num6 = 1;
				}
				else if (platformName.Equals("EPD-303--TC2"))
				{
					num5 = 1;
					iwidthedge = Epd37.iwidthedge;
					iheightedge = Epd37.iheightedge;
					num6 = 2;
					lz4packsize = 5120;
				}
				else if (platformName.Equals("EPD-302--TC2"))
				{
					num5 = 1;
					iwidthedge = EPD_BW_37.iwidthedge;
					iheightedge = EPD_BW_37.iheightedge;
					num6 = 1;
					lz4packsize = 5120;
				}
				else if (platformName.Equals("EPD-304--TC2"))
				{
					num5 = 1;
					iwidthedge = EPD_BWYR_37.iwidthedge;
					iheightedge = EPD_BWYR_37.iheightedge;
					num6 = 2;
					lz4packsize = 5120;
				}
				else
				{
					num6 = 1;
					num5 = 0;
				}
				EinkImage image = new EinkImage(iwidthedge, iheightedge, num6, bitmap, num5, lz4packsize, platformName, vername);
				api.DrawImage(image, DrawImageMethod.DIMethod_Normal, this);
				bitmap.Dispose();
			}
			catch (Exception)
			{
				MessageBox.Show("Image format error!");
			}
		}

		public void onProgress(DrawImageState state, object data)
		{
			int num = (int)data;
			Logfile logfile = new Logfile();
			string info = string.Empty;
			switch (state)
			{
			case DrawImageState.DIState_Erase:
				info = "ERASE...";
				num = 0;
				break;
			case DrawImageState.DIState_SendData:
				if (num == 100)
				{
					num = 90;
				}
				else
				{
					num = (int)(10.0 + (double)num * 0.8);
				}
				info = "SendData...";
				break;
			case DrawImageState.DIState_WriteToEPD:
				info = "WriteToEPD...";
				num = 90;
				break;
			case DrawImageState.DIState_Error:
				num = 100;
				info = "Tag Error...";
				break;
			case DrawImageState.DIState_Finish:
				num = 100;
				info = "Finish...";
				break;
			}
			Invoke((MethodInvoker)delegate
			{
				lblProgress.Text = $" {info}{num}%";
				progressBar1.Value = num;
			});
			if (num == 100)
			{
				spendtimer.Stop();
				Invoke((MethodInvoker)delegate
				{
					UpdateButtons(true);
				});
				string message = $"DeviceID : {textBox4.Text}\r\nFWVersion : {vername}\r\n";
				logfile.Devinfo_Log(message);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string[] array = null;
			array = SerialPort.GetPortNames();
			for (int i = 0; i < array.Length; i++)
			{
				comboBoxPorts.Items.Add(array[i]);
			}
			if (comboBoxPorts.Items.Count > 0 && comboBoxPorts.Text == "")
			{
				comboBoxPorts.Text = comboBoxPorts.Items[0].ToString();
			}
		}

		private void buildConnection()
		{
			nfc = new D30Command(comboBoxPorts.Text);
			if (nfc.openNFC())
			{
				manager.setNFCCommand(nfc);
				api = manager.getNfcAPI();
			}
			else
			{
				nfc = null;
				MessageBox.Show("Cannot open NFC Reader");
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			closeConnection();
		}

		private void closeConnection()
		{
			if (nfc != null)
			{
				onTagStateChange(NFCTagState.NFC_TAG_STATE_TAG_OFF);
				api = null;
				manager.setNFCCommand(null);
				nfc.closeNFC();
				nfc = null;
			}
		}

		private void comboBoxPorts_SelectedIndexChanged(object sender, EventArgs e)
		{
			closeConnection();
			buildConnection();
		}

		private void updatePreview(string filePath)
		{
			picboxPreview.Image = new Bitmap(filePath);
			if (picboxPreview.Image.Width < picboxPreview.Image.Height)
			{
				picboxPreview.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
			}
		}

		private void btnExplore_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Select jpg file to update";
			openFileDialog.InitialDirectory = ".\\";
			openFileDialog.Filter = "xls files (*.*)|*.jpg";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				txtBoxPath.Text = openFileDialog.FileName;
				updatePreview(txtBoxPath.Text);
				UpdateButtons(true);
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(LEOD30_Factory_tool.Form1));
			imageList1 = new System.Windows.Forms.ImageList(components);
			panel1 = new System.Windows.Forms.Panel();
			comboBoxPorts = new System.Windows.Forms.ComboBox();
			textBox4 = new System.Windows.Forms.TextBox();
			pictureBox1 = new System.Windows.Forms.PictureBox();
			panel2 = new System.Windows.Forms.Panel();
			btnExplore = new System.Windows.Forms.Button();
			txtBoxPath = new System.Windows.Forms.TextBox();
			lblSpendTime = new System.Windows.Forms.Label();
			slblCount = new System.Windows.Forms.Label();
			lblProgress = new System.Windows.Forms.Label();
			btnUpdate = new System.Windows.Forms.Button();
			progressBar1 = new System.Windows.Forms.ProgressBar();
			picboxPreview = new System.Windows.Forms.PictureBox();
			panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
			panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)picboxPreview).BeginInit();
			SuspendLayout();
			imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)componentResourceManager.GetObject("imageList1.ImageStream");
			imageList1.TransparentColor = System.Drawing.Color.Transparent;
			imageList1.Images.SetKeyName(0, "red_led.png");
			imageList1.Images.SetKeyName(1, "yellow_led.png");
			imageList1.Images.SetKeyName(2, "green_led.png");
			imageList1.Images.SetKeyName(3, "blue_led.png");
			panel1.Controls.Add(comboBoxPorts);
			panel1.Controls.Add(textBox4);
			panel1.Controls.Add(pictureBox1);
			panel1.Dock = System.Windows.Forms.DockStyle.Top;
			panel1.Location = new System.Drawing.Point(0, 0);
			panel1.Margin = new System.Windows.Forms.Padding(2);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(358, 46);
			panel1.TabIndex = 7;
			comboBoxPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			comboBoxPorts.FormattingEnabled = true;
			comboBoxPorts.Location = new System.Drawing.Point(218, 15);
			comboBoxPorts.Margin = new System.Windows.Forms.Padding(2);
			comboBoxPorts.Name = "comboBoxPorts";
			comboBoxPorts.Size = new System.Drawing.Size(92, 20);
			comboBoxPorts.TabIndex = 7;
			comboBoxPorts.SelectedIndexChanged += new System.EventHandler(comboBoxPorts_SelectedIndexChanged);
			textBox4.Location = new System.Drawing.Point(70, 13);
			textBox4.Margin = new System.Windows.Forms.Padding(2);
			textBox4.Name = "textBox4";
			textBox4.ReadOnly = true;
			textBox4.Size = new System.Drawing.Size(144, 22);
			textBox4.TabIndex = 6;
			pictureBox1.Location = new System.Drawing.Point(32, 7);
			pictureBox1.Margin = new System.Windows.Forms.Padding(2);
			pictureBox1.Name = "pictureBox1";
			pictureBox1.Size = new System.Drawing.Size(32, 32);
			pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			pictureBox1.TabIndex = 5;
			pictureBox1.TabStop = false;
			panel2.Controls.Add(picboxPreview);
			panel2.Controls.Add(btnExplore);
			panel2.Controls.Add(txtBoxPath);
			panel2.Controls.Add(lblSpendTime);
			panel2.Controls.Add(slblCount);
			panel2.Controls.Add(lblProgress);
			panel2.Controls.Add(btnUpdate);
			panel2.Controls.Add(progressBar1);
			panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			panel2.Location = new System.Drawing.Point(0, 46);
			panel2.Margin = new System.Windows.Forms.Padding(2);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(358, 291);
			panel2.TabIndex = 8;
			btnExplore.Location = new System.Drawing.Point(260, 29);
			btnExplore.Name = "btnExplore";
			btnExplore.Size = new System.Drawing.Size(53, 23);
			btnExplore.TabIndex = 23;
			btnExplore.Text = "...";
			btnExplore.UseVisualStyleBackColor = true;
			btnExplore.Click += new System.EventHandler(btnExplore_Click);
			txtBoxPath.Location = new System.Drawing.Point(47, 31);
			txtBoxPath.Name = "txtBoxPath";
			txtBoxPath.ReadOnly = true;
			txtBoxPath.Size = new System.Drawing.Size(207, 22);
			txtBoxPath.TabIndex = 22;
			lblSpendTime.AutoSize = true;
			lblSpendTime.Location = new System.Drawing.Point(196, 173);
			lblSpendTime.Name = "lblSpendTime";
			lblSpendTime.Size = new System.Drawing.Size(11, 12);
			lblSpendTime.TabIndex = 21;
			lblSpendTime.Text = "0";
			slblCount.AutoSize = true;
			slblCount.Location = new System.Drawing.Point(127, 173);
			slblCount.Name = "slblCount";
			slblCount.Size = new System.Drawing.Size(63, 12);
			slblCount.TabIndex = 20;
			slblCount.Text = "Spend time: ";
			lblProgress.AutoSize = true;
			lblProgress.Location = new System.Drawing.Point(207, 203);
			lblProgress.Name = "lblProgress";
			lblProgress.Size = new System.Drawing.Size(0, 12);
			lblProgress.TabIndex = 9;
			btnUpdate.Location = new System.Drawing.Point(49, 233);
			btnUpdate.Margin = new System.Windows.Forms.Padding(2);
			btnUpdate.Name = "btnUpdate";
			btnUpdate.Size = new System.Drawing.Size(264, 38);
			btnUpdate.TabIndex = 7;
			btnUpdate.Text = "Update";
			btnUpdate.UseVisualStyleBackColor = true;
			btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
			progressBar1.Location = new System.Drawing.Point(48, 200);
			progressBar1.Margin = new System.Windows.Forms.Padding(2);
			progressBar1.Name = "progressBar1";
			progressBar1.Size = new System.Drawing.Size(131, 18);
			progressBar1.TabIndex = 6;
			picboxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			picboxPreview.Location = new System.Drawing.Point(65, 63);
			picboxPreview.Name = "picboxPreview";
			picboxPreview.Size = new System.Drawing.Size(231, 100);
			picboxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			picboxPreview.TabIndex = 24;
			picboxPreview.TabStop = false;
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(358, 337);
			base.Controls.Add(panel2);
			base.Controls.Add(panel1);
			base.Margin = new System.Windows.Forms.Padding(2);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "Form1";
			Text = "LEO-D30 Factory Tool";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
			base.Load += new System.EventHandler(Form1_Load);
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)picboxPreview).EndInit();
			ResumeLayout(false);
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
namespace LEOD30_Factory_tool.Properties
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
					resourceMan = new ResourceManager("LEOD30_Factory_tool.Properties.Resources", typeof(Resources).Assembly);
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
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.7.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;
	}
}
