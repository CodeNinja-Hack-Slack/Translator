namespace TranslatorApp.Models;

public class AppConfig
{
    public string PrimaryTranslator { get; set; } = "Bing";
    public string BaiduAppId { get; set; } = "";
    public string BaiduSecretKey { get; set; } = "";
    public string YoudaoAppId { get; set; } = "";
    public string YoudaoSecretKey { get; set; } = "";
    public bool AutoDetectLanguage { get; set; } = true;
    public int NotificationDurationMs { get; set; } = 3000;

    public int HotkeyModifiers { get; set; } = 1;
    public int HotkeyKey { get; set; } = 0x43;
    public bool ShowInTaskbar { get; set; } = true;
    public bool ShowTrayIcon { get; set; } = true;
}
