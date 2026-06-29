using TranslatorApp.Helpers;
using TranslatorApp.Models;
using TranslatorApp.Services;

namespace TranslatorApp.Forms;

public class MainForm : Form
{
    private readonly HotkeyManager _hotkeys;
    private readonly TranslationService _translator;
    private readonly AppConfig _config;
    private bool _disposed;

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
        Icon = IconHelper.GenerateIcon(32);

        ConfigManager.ConfigChanged += OnConfigChanged;

        RegisterHotkey();
        ApplyIconMode();
    }

    public void Cleanup()
    {
        if (_disposed) return;
        _disposed = true;
        _hotkeys.Dispose();
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
            Icon = IconHelper.GenerateIcon(32);
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
            return;
        }

        if (m.Msg == 0x0112 && _config.ShowInTaskbar) // WM_SYSCOMMAND
        {
            var cmd = m.WParam.ToInt32() & 0xFFF0;
            if (cmd == 0xF120) // SC_RESTORE — taskbar click
            {
                BeginInvoke(new Action(ToggleTaskbarClick));
                return;
            }
        }

        base.WndProc(ref m);
    }

    private void ToggleTaskbarClick()
    {
        var inputForm = Application.OpenForms.OfType<TranslateInputForm>().FirstOrDefault();
        if (inputForm != null && inputForm.Visible)
        {
            inputForm.Hide();
        }
        else
        {
            ShowInputForm();
        }
    }

    private void RegisterHotkey()
    {
        _hotkeys.Register(_config.HotkeyModifiers, _config.HotkeyKey);
    }

    private void ShowInputForm()
    {
        var inputForm = Application.OpenForms.OfType<TranslateInputForm>().FirstOrDefault();
        if (inputForm == null || inputForm.IsDisposed)
        {
            inputForm = new TranslateInputForm();
            inputForm.Show();
            inputForm.Activate();
            return;
        }

        if (inputForm.Visible)
        {
            inputForm.Activate();
            return;
        }

        inputForm.ShowWithAnimation();
        inputForm.Activate();
    }

    public async Task<string> TranslateText(string text)
    {
        return await _translator.TranslateAsync(text);
    }

    private void OnConfigChanged()
    {
        ApplyIconMode();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cleanup();
        }
        base.Dispose(disposing);
    }
}
