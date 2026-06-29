using TranslatorApp.Helpers;
using TranslatorApp.Models;

namespace TranslatorApp.Forms;

public class TranslateInputForm : Form
{
    private const int CompactHeight = 350;
    private const int ExpandedHeight = 700;

    private readonly TextBox _inputBox;
    private readonly Button _translateButton;
    private readonly Label _resultLabel;
    private readonly Label _sourceLabel;
    private readonly LinkLabel _settingsLink;

    private ComboBox _translatorCombo = null!;
    private TextBox _baiduAppIdBox = null!;
    private TextBox _baiduKeyBox = null!;
    private TextBox _youdaoAppIdBox = null!;
    private TextBox _youdaoKeyBox = null!;
    private Button _hotkeyButton = null!;
    private NumericUpDown _durationUpDown = null!;
    private CheckBox _taskbarCheck = null!;
    private CheckBox _autoDetectCheck = null!;
    private Panel _settingsPanel = null!;
    private Button _saveSettingsButton = null!;

    private readonly AppConfig _config;
    private bool _listeningForHotkey;
    private bool _settingsVisible;

    public TranslateInputForm()
    {
        _config = ConfigManager.Load();

        Text = "Translator - 中英翻译";
        Size = new Size(520, CompactHeight);
        MinimumSize = new Size(480, CompactHeight);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        ShowIcon = false;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;

        _sourceLabel = new Label
        {
            Text = $"按 {HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey)} 呼出此窗口",
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
            Size = new Size(488, 220),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Microsoft YaHei", 10.5f),
            BackColor = Color.White,
            Padding = new Padding(8),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoEllipsis = true
        };

        _settingsLink = new LinkLabel
        {
            Text = "设置",
            Location = new Point(466, 8),
            Size = new Size(36, 18),
            LinkColor = Color.Gray,
            ActiveLinkColor = Color.DimGray,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _settingsLink.LinkClicked += (_, _) => ToggleSettings();

        _translateButton.Click += async (_, _) => await DoTranslate();
        _inputBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                _ = DoTranslate();
            }
        };

        Controls.Add(_sourceLabel);
        Controls.Add(_inputBox);
        Controls.Add(_translateButton);
        Controls.Add(_resultLabel);
        Controls.Add(_settingsLink);

        BuildSettingsPanel();
        UpdateHotkeyHint(HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey));

        FormClosing += (_, e) =>
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                SaveSettings();
                Program.MainFormRef?.Cleanup();
                Application.Exit();
            }
        };

        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                Hide();
            }
        };

        Load += (_, _) => _inputBox.Focus();
    }

    private void ToggleSettings()
    {
        _settingsVisible = !_settingsVisible;
        _settingsPanel.Visible = _settingsVisible;
        _settingsLink.Text = _settingsVisible ? "关闭" : "设置";
        MinimumSize = _settingsVisible ? new Size(480, ExpandedHeight) : new Size(480, CompactHeight);
        Height = _settingsVisible ? ExpandedHeight : CompactHeight;
        if (_settingsVisible)
            _resultLabel.Height = 160;
        else
            _resultLabel.Height = ClientSize.Height - _resultLabel.Top - 14;
        _inputBox.Focus();
    }

    public void ShowWithAnimation()
    {
        Show();
        Activate();
    }

    private void BuildSettingsPanel()
    {
        _settingsPanel = new Panel
        {
            Location = new Point(14, 246),
            Size = new Size(488, 385),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false
        };

        var sep = new Label
        {
            Text = "────────────────── 设置 ──────────────────",
            Location = new Point(0, 0),
            Size = new Size(488, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft YaHei", 9),
            ForeColor = Color.Gray
        };
        _settingsPanel.Controls.Add(sep);

        var y = 30;

        _settingsPanel.Controls.Add(new Label
        {
            Text = "翻译引擎:",
            Location = new Point(0, y + 4),
            Size = new Size(80, 20)
        });
        _translatorCombo = new ComboBox
        {
            Location = new Point(85, y),
            Size = new Size(140, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _translatorCombo.Items.AddRange(new[] { "Bing", "Baidu", "Youdao" });
        _translatorCombo.SelectedItem = _config.PrimaryTranslator;
        _settingsPanel.Controls.Add(_translatorCombo);

        var bingHint = new Label
        {
            Text = "✓ Bing 无需配置",
            Location = new Point(235, y + 3),
            Size = new Size(120, 20),
            Font = new Font("Microsoft YaHei", 8),
            ForeColor = Color.Green
        };
        _settingsPanel.Controls.Add(bingHint);

        y += 32;
        var groupBaidu = new GroupBox { Text = "百度翻译 API", Location = new Point(0, y), Size = new Size(488, 60) };
        groupBaidu.Controls.Add(new Label { Text = "App ID:", Location = new Point(10, 18), Size = new Size(50, 20) });
        _baiduAppIdBox = new TextBox { Text = _config.BaiduAppId, Location = new Point(60, 16), Size = new Size(180, 23) };
        groupBaidu.Controls.Add(_baiduAppIdBox);
        groupBaidu.Controls.Add(new Label { Text = "密钥:", Location = new Point(250, 18), Size = new Size(40, 20) });
        _baiduKeyBox = new TextBox { Text = _config.BaiduSecretKey, Location = new Point(290, 16), Size = new Size(185, 23), UseSystemPasswordChar = true };
        groupBaidu.Controls.Add(_baiduKeyBox);
        _settingsPanel.Controls.Add(groupBaidu);

        y += 70;
        var groupYoudao = new GroupBox { Text = "有道翻译 API", Location = new Point(0, y), Size = new Size(488, 60) };
        groupYoudao.Controls.Add(new Label { Text = "App ID:", Location = new Point(10, 18), Size = new Size(50, 20) });
        _youdaoAppIdBox = new TextBox { Text = _config.YoudaoAppId, Location = new Point(60, 16), Size = new Size(180, 23) };
        groupYoudao.Controls.Add(_youdaoAppIdBox);
        groupYoudao.Controls.Add(new Label { Text = "密钥:", Location = new Point(250, 18), Size = new Size(40, 20) });
        _youdaoKeyBox = new TextBox { Text = _config.YoudaoSecretKey, Location = new Point(290, 16), Size = new Size(185, 23), UseSystemPasswordChar = true };
        groupYoudao.Controls.Add(_youdaoKeyBox);
        _settingsPanel.Controls.Add(groupYoudao);

        y += 70;
        var groupHotkey = new GroupBox { Text = "快捷键", Location = new Point(0, y), Size = new Size(488, 50) };
        _hotkeyButton = new Button
        {
            Text = $"点击设置  (当前: {HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey)})",
            Location = new Point(14, 16),
            Size = new Size(460, 24),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei", 9)
        };
        _hotkeyButton.Click += HotkeyButton_Click;
        groupHotkey.Controls.Add(_hotkeyButton);
        _settingsPanel.Controls.Add(groupHotkey);

        y += 60;
        _autoDetectCheck = new CheckBox
        {
            Text = "自动检测语言",
            Location = new Point(0, y),
            Size = new Size(130, 24),
            Checked = _config.AutoDetectLanguage
        };
        _settingsPanel.Controls.Add(_autoDetectCheck);

        _taskbarCheck = new CheckBox
        {
            Text = "在任务栏显示图标",
            Location = new Point(150, y),
            Size = new Size(160, 24),
            Checked = _config.ShowInTaskbar
        };
        _settingsPanel.Controls.Add(_taskbarCheck);

        y += 28;
        _settingsPanel.Controls.Add(new Label
        {
            Text = "通知时长 (毫秒):",
            Location = new Point(0, y + 4),
            Size = new Size(110, 20)
        });
        _durationUpDown = new NumericUpDown
        {
            Location = new Point(110, y),
            Size = new Size(60, 23),
            Minimum = 1000,
            Maximum = 10000,
            Increment = 500,
            Value = _config.NotificationDurationMs
        };
        _settingsPanel.Controls.Add(_durationUpDown);

        y += 35;
        _saveSettingsButton = new Button
        {
            Text = "保存设置",
            Location = new Point(195, y),
            Size = new Size(100, 28)
        };
        _saveSettingsButton.Click += (_, _) =>
        {
            SaveSettings();
            MessageBox.Show("设置已保存", "Translator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        _settingsPanel.Controls.Add(_saveSettingsButton);

        Controls.Add(_settingsPanel);
        KeyDown += TranslateInputForm_KeyDown;
    }

    public void UpdateHotkeyHint(string hotkeyText)
    {
        _sourceLabel.Text = $"按 {hotkeyText} 呼出此窗口";
    }

    private void SaveSettings()
    {
        _config.PrimaryTranslator = _translatorCombo.SelectedItem?.ToString() ?? "Bing";
        _config.BaiduAppId = _baiduAppIdBox.Text.Trim();
        _config.BaiduSecretKey = _baiduKeyBox.Text.Trim();
        _config.YoudaoAppId = _youdaoAppIdBox.Text.Trim();
        _config.YoudaoSecretKey = _youdaoKeyBox.Text.Trim();
        _config.AutoDetectLanguage = _autoDetectCheck.Checked;
        _config.NotificationDurationMs = (int)_durationUpDown.Value;
        _config.ShowInTaskbar = _taskbarCheck.Checked;

        ConfigManager.Save(_config);
        ConfigManager.OnConfigChanged();
    }

    private void HotkeyButton_Click(object? sender, EventArgs e)
    {
        _listeningForHotkey = true;
        _hotkeyButton.Text = "按下快捷键... (Esc 取消)";
        _hotkeyButton.Focus();
    }

    private void TranslateInputForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!_listeningForHotkey) return;

        if (e.KeyCode == Keys.Escape)
        {
            _listeningForHotkey = false;
            _hotkeyButton.Text = $"点击设置  (当前: {HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey)})";
            e.SuppressKeyPress = true;
            return;
        }

        var modifiers = 0;
        if (e.Alt) modifiers |= (int)HotkeyHelper.MOD_ALT;
        if (e.Control) modifiers |= (int)HotkeyHelper.MOD_CONTROL;
        if (e.Shift) modifiers |= (int)HotkeyHelper.MOD_SHIFT;

        var key = (int)e.KeyCode;
        var isModifier = key is (int)Keys.Menu or (int)Keys.ControlKey or (int)Keys.ShiftKey;

        if (modifiers == 0 || isModifier) return;

        _config.HotkeyModifiers = modifiers;
        _config.HotkeyKey = key;

        _listeningForHotkey = false;
        _hotkeyButton.Text = $"点击设置  (当前: {HotkeyHelper.Format(modifiers, key)})";
        e.SuppressKeyPress = true;
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
