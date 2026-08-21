using System;
using System.IO;

namespace AG_EPD_Tag
{
    public class AppSettings
    {
        public string Language { get; set; }
        public string LastComPort { get; set; }
        public int LastStyle { get; set; }
        public bool ShowBorder { get; set; }

        public AppSettings()
        {
            Language = "en";
            LastComPort = "";
            LastStyle = 1; // 0 = Style A, 1 = Style B (Default)
            ShowBorder = false;
        }

        private static string SettingsFilePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_settings.ini");
            }
        }

        public static AppSettings Load()
        {
            AppSettings settings = new AppSettings();
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string[] lines = File.ReadAllLines(SettingsFilePath);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                            continue;

                        int eqIdx = trimmed.IndexOf('=');
                        if (eqIdx > 0)
                        {
                            string key = trimmed.Substring(0, eqIdx).Trim();
                            string val = trimmed.Substring(eqIdx + 1).Trim();

                            switch (key.ToLowerInvariant())
                            {
                                case "language":
                                    settings.Language = val;
                                    break;
                                case "lastcomport":
                                    settings.LastComPort = val;
                                    break;
                                case "laststyle":
                                    int style;
                                    if (int.TryParse(val, out style)) settings.LastStyle = style;
                                    break;
                                case "showborder":
                                    bool border;
                                    if (bool.TryParse(val, out border)) settings.ShowBorder = border;
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }
            return settings;
        }

        public void Save()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(SettingsFilePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("# MedTRx EPD Application Settings");
                    writer.WriteLine("Language=" + Language);
                    writer.WriteLine("LastComPort=" + LastComPort);
                    writer.WriteLine("LastStyle=" + LastStyle);
                    writer.WriteLine("ShowBorder=" + ShowBorder);
                }
            }
            catch { }
        }
    }
}
