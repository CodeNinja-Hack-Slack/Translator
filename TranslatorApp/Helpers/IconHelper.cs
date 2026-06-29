using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace TranslatorApp.Helpers;

public static class IconHelper
{
    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static Icon GenerateIcon(int size)
    {
        using var bitmap = new Bitmap(size, size);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
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
        var icon = Icon.FromHandle(hIcon);
        var owned = (Icon)icon.Clone();
        DestroyIcon(hIcon);
        return owned;
    }
}
