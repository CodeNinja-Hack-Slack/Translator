using System.Text.Json;
using TranslatorApp.Models;

namespace TranslatorApp.Helpers;

public static class ConfigManager
{
    private static readonly string ConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private static AppConfig? _cached;

    public static event Action? ConfigChanged;

    public static AppConfig Load()
    {
        if (_cached != null) return _cached;

        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _cached = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                return _cached;
            }
        }
        catch { }
        _cached = new AppConfig();
        return _cached;
    }

    public static void Save(AppConfig config)
    {
        try
        {
            _cached = config;
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static void OnConfigChanged()
    {
        _cached = null;
        ConfigChanged?.Invoke();
    }
}
