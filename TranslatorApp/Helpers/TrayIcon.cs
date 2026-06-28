using System.Drawing;
using System.Drawing.Text;

namespace TranslatorApp.Helpers;

public class TrayIcon : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;

    public event EventHandler? ShowClicked;
    public event EventHandler? SettingsClicked;
    public event EventHandler? ExitClicked;

    public NotifyIcon NotifyIcon => _notifyIcon;

    public TrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
            Text = "Translator - 中英翻译",
            Visible = true
        };

        _menu = new ContextMenuStrip();

        var miShow = _menu.Items.Add("打开翻译");
        miShow.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
        miShow.Click += (_, _) => ShowClicked?.Invoke(this, EventArgs.Empty);

        _menu.Items.Add(new ToolStripSeparator());

        var miSettings = _menu.Items.Add("设置...");
        miSettings.Click += (_, _) => SettingsClicked?.Invoke(this, EventArgs.Empty);

        _menu.Items.Add(new ToolStripSeparator());

        var miExit = _menu.Items.Add("退出");
        miExit.Click += (_, _) => ExitClicked?.Invoke(this, EventArgs.Empty);

        _notifyIcon.ContextMenuStrip = _menu;
    }

    public void ShowNotification(string title, string text, int durationMs = 3000)
    {
        _notifyIcon.ShowBalloonTip(durationMs, title, text, ToolTipIcon.Info);
    }

    private static Icon CreateIcon()
    {
        var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.Transparent);

        using var brush = new SolidBrush(Color.DodgerBlue);
        g.FillRectangle(brush, 0, 0, 16, 16);

        using var font = new Font("Segoe UI", 8, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.DrawString("译", font, textBrush, 0, -1);

        var hIcon = bitmap.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}
