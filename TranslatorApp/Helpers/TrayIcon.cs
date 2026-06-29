using System.Drawing;
using System.Drawing.Drawing2D;
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
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var brush = new LinearGradientBrush(
            new Point(0, 0), new Point(16, 16),
            Color.FromArgb(0, 130, 230),
            Color.FromArgb(0, 60, 160));
        g.FillEllipse(brush, 0, 0, 16, 16);

        using var font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString("译", font, textBrush, new RectangleF(0, 0, 16, 16), format);

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
