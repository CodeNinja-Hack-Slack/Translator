using TranslatorApp.Helpers;
using TranslatorApp.Models;
using TranslatorApp.Services;

namespace TranslatorApp.Forms;

public class MainForm : Form
{
    private readonly HotkeyManager _hotkeys;
    private readonly TrayIcon _tray;
    private readonly TranslationService _translator;
    private readonly AppConfig _config;
    private TranslateInputForm? _inputForm;
    private SettingsForm? _settingsForm;

    public TranslationService Translator => _translator;

    public MainForm()
    {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;

        _config = ConfigManager.Load();
        _translator = new TranslationService(_config);
        _hotkeys = new HotkeyManager(Handle);

        _tray = new TrayIcon();
        _tray.ShowClicked += (_, _) => ShowInputForm();
        _tray.SettingsClicked += (_, _) => ShowSettings();
        _tray.ExitClicked += (_, _) => Exit();

        ConfigManager.ConfigChanged += OnConfigChanged;

        RegisterHotkey();

        BeginInvoke(new Action(() =>
            _tray.ShowNotification("Translator 已启动", "按快捷键呼出翻译窗口", 2000)));
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == HotkeyManager.WM_HOTKEY && m.WParam.ToInt32() == HotkeyManager.ID_TRANSLATE_INPUT)
        {
            ShowInputForm();
        }
        base.WndProc(ref m);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    private void RegisterHotkey()
    {
        _hotkeys.Register(_config.HotkeyModifiers, _config.HotkeyKey);
    }

    private void ShowInputForm()
    {
        if (_inputForm == null || _inputForm.IsDisposed)
        {
            _inputForm = new TranslateInputForm();
            _inputForm.UpdateHotkeyHint(HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey));
            _inputForm.FormClosed += (_, _) => _inputForm = null;
        }

        if (_inputForm.Visible)
        {
            _inputForm.Activate();
            return;
        }

        _inputForm.Show();
        _inputForm.Activate();
    }

    public async Task<string> TranslateText(string text)
    {
        return await _translator.TranslateAsync(text);
    }

    private void ShowSettings()
    {
        if (_settingsForm == null || _settingsForm.IsDisposed)
        {
            _settingsForm = new SettingsForm(_config);
            _settingsForm.FormClosed += (_, _) =>
            {
                _settingsForm = null;
                RegisterHotkey();
                if (_inputForm != null && !_inputForm.IsDisposed)
                    _inputForm.UpdateHotkeyHint(HotkeyHelper.Format(_config.HotkeyModifiers, _config.HotkeyKey));
            };
        }

        _settingsForm.Show();
        _settingsForm.Activate();
    }

    private void OnConfigChanged()
    {
        ConfigManager.Save(_config);
    }

    private void Exit()
    {
        _hotkeys.Dispose();
        _tray.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hotkeys.Dispose();
            _tray.Dispose();
        }
        base.Dispose(disposing);
    }
}
