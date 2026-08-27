using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AG_EPD_Tag
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private const int SW_RESTORE = 9;

        [STAThread]
        private static void Main()
        {
            bool isNewInstance;
            using (Mutex mutex = new Mutex(true, "Global\\MedTRx_EPD_SingleInstance_Mutex", out isNewInstance))
            {
                if (!isNewInstance)
                {
                    IntPtr hWnd = FindWindow(null, "MedTRx EPD v1.0.2");
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                        SetForegroundWindow(hWnd);
                    }
                    else
                    {
                        MessageBox.Show("MedTRx EPD is already running.", "MedTRx EPD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
