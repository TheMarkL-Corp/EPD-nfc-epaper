using System;
using System.Collections.Generic;

namespace AG_EPD_Tag
{
    public static class Localization
    {
        public static string CurrentLanguage = "en"; // "en" or "zh-TW"

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            {"AppTitle", "MedTRx EPD v1.0.2"},
            {"ComPort", "COM Port:"},
            {"Refresh", "Refresh"},
            {"Scanning", "Scanning..."},
            {"NoTag", "No Tag Detected"},
            {"TagConnecting", "Tag Detected (Connecting...)"},
            {"TagReady", "Tag Ready to Write"},
            {"NoPorts", "No COM Ports Found"},
            {"PlugInReader", "Please plug in NFC Reader USB"},
            {"OpenFailed", "Failed to open port {0}"},
            {"TagInfo", "UID: {0} | FW: {1}"},
            {"TagContentGroup", " Tag Text Settings "},
            {"Line2Label", "Header Text:"},
            {"Line1Label", "Main Body Text:"},
            {"StyleGroup", " Display Layout Style "},
            {"StyleB", "Style B: Clean White (Default)"},
            {"StyleA", "Style A: Black Header Banner"},
            {"ShowBorder", "Show Rounded Outer Border"},
            {"PreviewGroup", " Live Tag Preview (2.13\" 296×128) "},
            {"ProgramTag", "Write Tag"},
            {"Ready", "System Ready"},
            {"Duration", "Duration: {0}s"},
            {"InitiatingFlash", "Initiating Tag Transfer..."},
            {"ErasingFlash", "Erasing Tag Flash (0%)..."},
            {"UploadingImage", "Uploading Image ({0}%)..."},
            {"RefreshingDisplay", "Refreshing E-Paper Display (90%)..."},
            {"Completed", "Write Complete! Display Updated (100%)"},
            {"FlashError", "Error: Tag Write Failed!"},
            {"TagNotReadyTitle", "Tag Not Ready"},
            {"TagNotReadyMsg", "Please place a 2.13\" EPD tag flat on the reader first!"},
            {"TransferFailedTitle", "Write Failed"},
            {"TransferFailedMsg", "Failed to update tag image. Please ensure tag remains stable on the reader during transfer."},
            {"ProgrammingErrorTitle", "Write Error"}
        };

        private static readonly Dictionary<string, string> ZhTW = new Dictionary<string, string>
        {
            {"AppTitle", "MedTRx EPD v1.0.2"},
            {"ComPort", "序列埠 (COM):"},
            {"Refresh", "重新整理"},
            {"Scanning", "搜尋中..."},
            {"NoTag", "未感應到標籤"},
            {"TagConnecting", "已感應標籤 (連線中...)"},
            {"TagReady", "標籤就緒 (可寫入)"},
            {"NoPorts", "未找到通訊埠"},
            {"PlugInReader", "請插入 NFC 讀卡機 USB"},
            {"OpenFailed", "無法開啟通訊埠 {0}"},
            {"TagInfo", "UID: {0} | 韌體: {1}"},
            {"TagContentGroup", " 標籤文字設定 "},
            {"Line2Label", "頂欄文字 (Header):"},
            {"Line1Label", "主要內容文字 (Body):"},
            {"StyleGroup", " 標籤顯示樣式 "},
            {"StyleB", "樣式 B：白底黑字 (預設)"},
            {"StyleA", "樣式 A：黑底頂欄"},
            {"ShowBorder", "顯示外圍圓角外框"},
            {"PreviewGroup", " 即時標籤畫面預覽 (2.13\" 296×128) "},
            {"ProgramTag", "寫入標籤"},
            {"Ready", "系統就緒"},
            {"Duration", "耗時: {0} 秒"},
            {"InitiatingFlash", "開始傳輸影像資料..."},
            {"ErasingFlash", "抹除標籤記憶體中 (0%)..."},
            {"UploadingImage", "上傳影像資料中 ({0}%)..."},
            {"RefreshingDisplay", "更新電子紙畫面中 (90%)..."},
            {"Completed", "寫入完成！畫面已更新 (100%)"},
            {"FlashError", "錯誤：標籤寫入失敗！"},
            {"TagNotReadyTitle", "標籤未就緒"},
            {"TagNotReadyMsg", "請先將 2.13 吋 EPD 電子紙標籤平放於讀卡機上！"},
            {"TransferFailedTitle", "寫入失敗"},
            {"TransferFailedMsg", "標籤影像更新失敗，請確認標籤在傳輸期間平穩放置於讀卡機上。"},
            {"ProgrammingErrorTitle", "寫入錯誤"}
        };

        public static string Get(string key, params object[] args)
        {
            Dictionary<string, string> dict = (CurrentLanguage == "zh-TW" || CurrentLanguage == "zh") ? ZhTW : En;
            string val;
            if (!dict.TryGetValue(key, out val))
            {
                if (!En.TryGetValue(key, out val))
                {
                    val = key;
                }
            }

            if (args != null && args.Length > 0)
            {
                try { return string.Format(val, args); } catch { return val; }
            }
            return val;
        }
    }
}
