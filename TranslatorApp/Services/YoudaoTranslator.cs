using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TranslatorApp.Services;

public class YoudaoTranslator : ITranslator
{
    private readonly string _appId;
    private readonly string _secretKey;
    private readonly HttpClient _httpClient;

    public string Name => "有道翻译";
    public bool IsConfigured => !string.IsNullOrEmpty(_appId) && !string.IsNullOrEmpty(_secretKey);

    public YoudaoTranslator(string appId, string secretKey)
    {
        _appId = appId;
        _secretKey = secretKey;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<string> TranslateAsync(string text, string fromLang, string toLang)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("有道翻译未配置");

        var salt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var curtime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signStr = $"{_appId}{text}{salt}{curtime}{_secretKey}";
        var sign = Sha256(signStr);

        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["q"] = text,
            ["from"] = fromLang,
            ["to"] = toLang,
            ["appKey"] = _appId,
            ["salt"] = salt,
            ["sign"] = sign,
            ["signType"] = "v3",
            ["curtime"] = curtime
        });

        var response = await _httpClient.PostAsync("https://openapi.youdao.com/api", formData);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.GetProperty("errorCode").GetInt32() != 0)
            throw new Exception($"有道翻译错误: {root.GetProperty("errorCode")}");

        var transResult = root.GetProperty("translation");
        var results = new List<string>();
        foreach (var item in transResult.EnumerateArray())
        {
            results.Add(item.GetString()!);
        }

        return string.Join("\n", results);
    }

    private static string Sha256(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
