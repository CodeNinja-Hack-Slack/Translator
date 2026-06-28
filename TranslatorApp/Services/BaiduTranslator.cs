using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TranslatorApp.Services;

public class BaiduTranslator : ITranslator
{
    private readonly string _appId;
    private readonly string _secretKey;
    private readonly HttpClient _httpClient;

    public string Name => "百度翻译";
    public bool IsConfigured => !string.IsNullOrEmpty(_appId) && !string.IsNullOrEmpty(_secretKey);

    public BaiduTranslator(string appId, string secretKey)
    {
        _appId = appId;
        _secretKey = secretKey;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<string> TranslateAsync(string text, string fromLang, string toLang)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("百度翻译未配置");

        var salt = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var sign = Md5($"{_appId}{text}{salt}{_secretKey}");

        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["q"] = text,
            ["from"] = fromLang,
            ["to"] = toLang,
            ["appid"] = _appId,
            ["salt"] = salt,
            ["sign"] = sign
        });

        var response = await _httpClient.PostAsync("https://api.fanyi.baidu.com/api/trans/vip/translate", formData);
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("error_code", out var errorCode))
            throw new Exception($"百度翻译错误: {errorCode} - {root.GetProperty("error_msg").GetString()}");

        var transResult = root.GetProperty("trans_result");
        var results = new List<string>();
        foreach (var item in transResult.EnumerateArray())
        {
            results.Add(item.GetProperty("dst").GetString()!);
        }

        return string.Join("\n", results);
    }

    private static string Md5(string input)
    {
        using (var md5 = MD5.Create())
        {
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
