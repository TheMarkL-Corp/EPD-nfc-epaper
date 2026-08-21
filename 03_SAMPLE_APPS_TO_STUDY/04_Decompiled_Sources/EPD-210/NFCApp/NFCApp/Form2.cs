using AdvNFCWrap;
using BarcodeLib;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace NFCApp
{
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
}
