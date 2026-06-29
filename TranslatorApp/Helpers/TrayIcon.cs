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

    private static Icon CreateIcon() => GenerateIcon(16);

    public static Icon GenerateIcon(int size)
    {
        var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        var s = (float)size;
        var r = s * 0.22f;

        using var path = new GraphicsPath();
        path.AddArc(0, 0, r * 2, r * 2, 180, 90);
        path.AddArc(s - r * 2, 0, r * 2, r * 2, 270, 90);
        path.AddArc(s - r * 2, s - r * 2, r * 2, r * 2, 0, 90);
        path.AddArc(0, s - r * 2, r * 2, r * 2, 90, 90);
        path.CloseFigure();

        using var bgBrush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(s * 0.7f, s),
            Color.FromArgb(0, 130, 230),
            Color.FromArgb(0, 60, 160));
        g.FillPath(bgBrush, path);

        using var hlBrush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(0, s * 0.45f),
            Color.FromArgb(50, Color.White),
            Color.FromArgb(0, Color.White));
        g.SetClip(path);
        g.FillRectangle(hlBrush, 0, 0, s, s * 0.45f);
        g.ResetClip();

        var fontSize = s * 0.58f;
        using var font = new Font("Microsoft YaHei", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        using var textBrush = new SolidBrush(Color.White);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString("译", font, textBrush, new RectangleF(-0.5f, 0.5f, s, s), format);

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
