using System.Runtime.InteropServices;

namespace TranslatorApp.Helpers;

public class HotkeyManager : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public const int WM_HOTKEY = 0x0312;
    public const int ID_TRANSLATE_INPUT = 1;

    private readonly IntPtr _hWnd;
    private int _currentModifiers;
    private int _currentKey;
    private bool _registered;

    public HotkeyManager(IntPtr hWnd)
    {
        _hWnd = hWnd;
    }

    public bool Register(int modifiers, int key)
    {
        Unregister();
        _currentModifiers = modifiers;
        _currentKey = key;
        _registered = RegisterHotKey(_hWnd, ID_TRANSLATE_INPUT, (uint)modifiers | MOD_NOREPEAT, (uint)key);
        return _registered;
    }

    public bool Reregister(int modifiers, int key)
    {
        return Register(modifiers, key);
    }

    public void Unregister()
    {
        if (_registered)
        {
            UnregisterHotKey(_hWnd, ID_TRANSLATE_INPUT);
            _registered = false;
        }
    }

    private const uint MOD_NOREPEAT = 0x4000;

    public void Dispose()
    {
        Unregister();
    }
}
