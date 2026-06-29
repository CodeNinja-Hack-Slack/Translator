using TranslatorApp.Helpers;
using TranslatorApp.Models;

namespace TranslatorApp.Forms;

public class SettingsForm : Form
{
    private readonly ComboBox _translatorCombo;
    private readonly TextBox _baiduAppIdBox;
    private readonly TextBox _baiduKeyBox;
    private readonly TextBox _youdaoAppIdBox;
    private readonly TextBox _youdaoKeyBox;
    private readonly Button _hotkeyButton;
    private readonly NumericUpDown _durationUpDown;
    private readonly CheckBox _showInTaskbarCheck;
    private readonly AppConfig _config;

    private bool _listeningForHotkey;

    public SettingsForm(AppConfig config)
    {
        _config = config;
        Text = "设置 - Translator";
        Size = new Size(420, 380);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;

        var y = 14;

        Controls.Add(new Label { Text = "翻译引擎:", Location = new Point(14, y + 4), Size = new Size(80, 20) });
        _translatorCombo = new ComboBox
        {
            Location = new Point(100, y), Size = new Size(140, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _translatorCombo.Items.AddRange(new[] { "Bing", "Baidu", "Youdao" });
        _translatorCombo.SelectedItem = config.PrimaryTranslator;
        Controls.Add(_translatorCombo);

        var bingHint = new Label
        {
            Text = "✓ Bing 无需配置",
            Location = new Point(250, y + 3), Size = new Size(100, 20),
            Font = new Font("Microsoft YaHei", 8),
            ForeColor = Color.Green
        };
        Controls.Add(bingHint);

        y += 35;
        var groupBaidu = new GroupBox { Text = "百度翻译 API", Location = new Point(14, y), Size = new Size(380, 74) };
        Controls.Add(groupBaidu);

        groupBaidu.Controls.Add(new Label { Text = "App ID:", Location = new Point(10, 20), Size = new Size(55, 20) });
        _baiduAppIdBox = new TextBox { Text = config.BaiduAppId, Location = new Point(70, 18), Size = new Size(295, 23) };
        groupBaidu.Controls.Add(_baiduAppIdBox);

        groupBaidu.Controls.Add(new Label { Text = "密钥:", Location = new Point(10, 46), Size = new Size(55, 20) });
        _baiduKeyBox = new TextBox { Text = config.BaiduSecretKey, Location = new Point(70, 44), Size = new Size(295, 23), UseSystemPasswordChar = true };
        groupBaidu.Controls.Add(_baiduKeyBox);

        y += 84;
        var groupYoudao = new GroupBox { Text = "有道翻译 API", Location = new Point(14, y), Size = new Size(380, 74) };
        Controls.Add(groupYoudao);

        groupYoudao.Controls.Add(new Label { Text = "App ID:", Location = new Point(10, 20), Size = new Size(55, 20) });
        _youdaoAppIdBox = new TextBox { Text = config.YoudaoAppId, Location = new Point(70, 18), Size = new Size(295, 23) };
        groupYoudao.Controls.Add(_youdaoAppIdBox);

        groupYoudao.Controls.Add(new Label { Text = "密钥:", Location = new Point(10, 46), Size = new Size(55, 20) });
        _youdaoKeyBox = new TextBox { Text = config.YoudaoSecretKey, Location = new Point(70, 44), Size = new Size(295, 23), UseSystemPasswordChar = true };
        groupYoudao.Controls.Add(_youdaoKeyBox);

        y += 84;
        var groupHotkey = new GroupBox { Text = "快捷键", Location = new Point(14, y), Size = new Size(380, 50) };
        Controls.Add(groupHotkey);

        _hotkeyButton = new Button
        {
            Text = $"点击设置  (当前: {HotkeyHelper.Format(config.HotkeyModifiers, config.HotkeyKey)})",
            Location = new Point(14, 18), Size = new Size(350, 24),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Microsoft YaHei", 9)
        };
        _hotkeyButton.Click += HotkeyButton_Click;
        groupHotkey.Controls.Add(_hotkeyButton);

        y += 55;
        _showInTaskbarCheck = new CheckBox
        {
            Text = "在任务栏显示图标",
            Location = new Point(14, y),
            Size = new Size(200, 24),
            Checked = config.ShowInTaskbar,
            Font = new Font("Microsoft YaHei", 9)
        };
        Controls.Add(_showInTaskbarCheck);

        y += 30;
        Controls.Add(new Label { Text = "通知时长 (毫秒):", Location = new Point(14, y + 4), Size = new Size(110, 20) });
        _durationUpDown = new NumericUpDown
        {
            Location = new Point(130, y), Size = new Size(60, 23),
            Minimum = 1000, Maximum = 10000, Increment = 500,
            Value = config.NotificationDurationMs
        };
        Controls.Add(_durationUpDown);

        y += 35;
        var saveBtn = new Button { Text = "保存", Location = new Point(160, y), Size = new Size(100, 30) };
        saveBtn.Click += Save_Click;
        Controls.Add(saveBtn);

        KeyDown += SettingsForm_KeyDown;
    }

    private void HotkeyButton_Click(object? sender, EventArgs e)
    {
        _listeningForHotkey = true;
        _hotkeyButton.Text = "按下快捷键... (Esc 取消)";
        _hotkeyButton.Focus();
    }

    private void SettingsForm_KeyDown(object? sender, KeyEventArgs e)
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

    private void Save_Click(object? sender, EventArgs e)
    {
        _config.PrimaryTranslator = _translatorCombo.SelectedItem?.ToString() ?? "Bing";
        _config.BaiduAppId = _baiduAppIdBox.Text.Trim();
        _config.BaiduSecretKey = _baiduKeyBox.Text.Trim();
        _config.YoudaoAppId = _youdaoAppIdBox.Text.Trim();
        _config.YoudaoSecretKey = _youdaoKeyBox.Text.Trim();
        _config.NotificationDurationMs = (int)_durationUpDown.Value;
        _config.ShowInTaskbar = _showInTaskbarCheck.Checked;

        ConfigManager.Save(_config);
        ConfigManager.OnConfigChanged();
        MessageBox.Show("设置已保存", "Translator", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
