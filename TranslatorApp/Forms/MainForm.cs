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
    public bool IsTaskbarMode => _config.ShowInTaskbar;

    public void SetTaskbarMode(bool show)
    {
        _config.ShowInTaskbar = show;
        ApplyIconMode();
        ConfigManager.Save(_config);
    }

    public MainForm()
    {
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.SizableToolWindow;

        _config = ConfigManager.Load();
        _translator = new TranslationService(_config);
        _hotkeys = new HotkeyManager(Handle);
        Icon = TrayIcon.GenerateIcon(32);

        _tray = new TrayIcon();
        _tray.ShowClicked += (_, _) => ShowInputForm();
        _tray.SettingsClicked += (_, _) => ShowSettings();
        _tray.ExitClicked += (_, _) => Exit();

        ConfigManager.ConfigChanged += OnConfigChanged;

        RegisterHotkey();
        ApplyIconMode();

        BeginInvoke(new Action(() =>
            _tray.ShowNotification("Translator 已启动", "按快捷键呼出翻译窗口", 2000)));
    }

    protected override void SetVisibleCore(bool value)
    {
        if (_config is { ShowInTaskbar: true })
            base.SetVisibleCore(value);
        else
            base.SetVisibleCore(false);
    }

    private void ApplyIconMode()
    {
        if (_config.ShowInTaskbar)
        {
            ShowInTaskbar = true;
            FormBorderStyle = FormBorderStyle.Sizable;
            if (IsHandleCreated) RecreateHandle();
            if (!Visible) Show();
            WindowState = FormWindowState.Minimized;
        }
        else
        {
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            if (Visible) Hide();
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == HotkeyManager.WM_HOTKEY && m.WParam.ToInt32() == HotkeyManager.ID_TRANSLATE_INPUT)
        {
            ShowInputForm();
        }
        base.WndProc(ref m);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        if (_config.ShowInTaskbar)
        {
            BeginInvoke(new Action(() =>
            {
                ShowInputForm();
                WindowState = FormWindowState.Minimized;
            }));
        }
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
        ApplyIconMode();
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
