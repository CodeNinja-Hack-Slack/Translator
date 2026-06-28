namespace TranslatorApp.Helpers;

public static class HotkeyHelper
{
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;

    public static string Format(int modifiers, int key)
    {
        var parts = new List<string>();
        if ((modifiers & MOD_ALT) != 0) parts.Add("Alt");
        if ((modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((modifiers & MOD_SHIFT) != 0) parts.Add("Shift");
        if ((modifiers & MOD_WIN) != 0) parts.Add("Win");

        var keyName = (Keys)key switch
        {
            Keys.D0 => "0", Keys.D1 => "1", Keys.D2 => "2", Keys.D3 => "3", Keys.D4 => "4",
            Keys.D5 => "5", Keys.D6 => "6", Keys.D7 => "7", Keys.D8 => "8", Keys.D9 => "9",
            Keys.A => "A", Keys.B => "B", Keys.C => "C", Keys.D => "D", Keys.E => "E",
            Keys.F => "F", Keys.G => "G", Keys.H => "H", Keys.I => "I", Keys.J => "J",
            Keys.K => "K", Keys.L => "L", Keys.M => "M", Keys.N => "N", Keys.O => "O",
            Keys.P => "P", Keys.Q => "Q", Keys.R => "R", Keys.S => "S", Keys.T => "T",
            Keys.U => "U", Keys.V => "V", Keys.W => "W", Keys.X => "X", Keys.Y => "Y",
            Keys.Z => "Z",
            Keys.F1 => "F1", Keys.F2 => "F2", Keys.F3 => "F3", Keys.F4 => "F4",
            Keys.F5 => "F5", Keys.F6 => "F6", Keys.F7 => "F7", Keys.F8 => "F8",
            Keys.F9 => "F9", Keys.F10 => "F10", Keys.F11 => "F11", Keys.F12 => "F12",
            _ => ((Keys)key).ToString()
        };

        parts.Add(keyName);
        return string.Join(" + ", parts);
    }
}
