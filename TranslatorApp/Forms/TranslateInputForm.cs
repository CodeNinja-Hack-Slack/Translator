using TranslatorApp.Helpers;
using TranslatorApp.Models;
using TranslatorApp.Services;

namespace TranslatorApp.Forms;

public class TranslateInputForm : Form
{
    private readonly TextBox _inputBox;
    private readonly Button _translateButton;
    private readonly Label _resultLabel;
    private readonly Label _sourceLabel;

    public TranslateInputForm()
    {
        Text = "Translator - 中英翻译";
        Size = new Size(520, 380);
        MinimumSize = new Size(400, 300);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        ShowIcon = false;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;

        _sourceLabel = new Label
        {
            Text = "按 Alt+C 呼出此窗口",
            Location = new Point(14, 10),
            Size = new Size(400, 18),
            Font = new Font("Microsoft YaHei", 9),
            ForeColor = Color.Gray
        };

        _inputBox = new TextBox
        {
            Location = new Point(14, 34),
            Size = new Size(400, 26),
            Font = new Font("Microsoft YaHei", 11),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _translateButton = new Button
        {
            Text = "翻译",
            Location = new Point(424, 33),
            Size = new Size(80, 28),
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _resultLabel = new Label
        {
            Location = new Point(14, 74),
            Size = new Size(488, 260),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Microsoft YaHei", 10.5f),
            BackColor = Color.White,
            Padding = new Padding(8),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true
        };

        Controls.AddRange(new Control[] { _sourceLabel, _inputBox, _translateButton, _resultLabel });

        _translateButton.Click += async (_, _) => await DoTranslate();
        _inputBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                _ = DoTranslate();
            }
        };

        Load += (_, _) => _inputBox.Focus();
    }

    public void UpdateHotkeyHint(string hotkeyText)
    {
        _sourceLabel.Text = $"按 {hotkeyText} 呼出此窗口";
    }

    private async Task DoTranslate()
    {
        var text = _inputBox.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        _resultLabel.Text = "翻译中...";
        _translateButton.Enabled = false;

        try
        {
            var result = await Program.MainFormRef?.TranslateText(text)!;
            _resultLabel.Text = result;
        }
        catch (Exception ex)
        {
            _resultLabel.Text = $"错误: {ex.Message}";
        }
        finally
        {
            _translateButton.Enabled = true;
            _inputBox.Focus();
            _inputBox.SelectAll();
        }
    }
}
