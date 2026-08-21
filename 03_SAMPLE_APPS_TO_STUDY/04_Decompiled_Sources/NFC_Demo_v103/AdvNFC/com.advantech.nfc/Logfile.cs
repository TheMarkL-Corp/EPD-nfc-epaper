using System;
using System.IO;

namespace com.advantech.nfc
{
	public class Logfile
	{
		private string _fileName;

		public Logfile()
		{
			_fileName = null;
		}

		public Logfile(string fileName)
		{
			_fileName = fileName;
		}

		public void WriteLog(string message)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\";
			Console.WriteLine("WriteLog  " + text);
			string path = text + DateTime.Now.ToString("yyyyMMdd") + ".txt";
			if (_fileName != null)
			{
				path = text + _fileName + ".txt";
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
			using (StreamWriter w = File.AppendText(path))
			{
				Log(message, w);
			}
		}

		public void ClearFile(string fileName)
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\Log\\";
			Console.WriteLine("WriteLog  " + text);
			string path = text + fileName + ".txt";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private static void Log(string logMessage, TextWriter w)
		{
			w.Write("\r\nLog Entry : ");
			DateTime now = DateTime.Now;
			string arg = now.ToLongTimeString();
			now = DateTime.Now;
			w.WriteLine("{0} {1}", arg, now.ToLongDateString());
			w.WriteLine("  :");
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine("-------------------------------");
		}
	}
}
