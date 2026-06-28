using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TranslatorApp.Services;

public class BingTranslator : ITranslator
{
    private readonly HttpClient _httpClient;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0";

    public string Name => "Bing 翻译";
    public bool IsConfigured => true;

    public BingTranslator()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }

    public async Task<string> TranslateAsync(string text, string fromLang, string toLang)
    {
        var token = await GetTokenAsync();

        var msFromLang = ConvertToMsLang(fromLang);
        var msToLang = ConvertToMsLang(toLang);

        var requestBody = JsonSerializer.Serialize(new[] { new { Text = text } });
        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var fromParam = string.IsNullOrEmpty(msFromLang) ? "" : $"&from={msFromLang}";
        var url = $"https://api.cognitive.microsofttranslator.com/translate" +
                  $"?api-version=3.0{fromParam}&to={msToLang}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = content;

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var detail = string.IsNullOrEmpty(responseBody) ? response.ReasonPhrase ?? "" : responseBody;
            throw new HttpRequestException($"Bing 翻译 API 错误 ({(int)response.StatusCode}): {detail}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var translations = doc.RootElement[0].GetProperty("translations");
        var results = new List<string>();
        foreach (var t in translations.EnumerateArray())
        {
            results.Add(t.GetProperty("text").GetString()!);
        }

        return string.Join("\n", results);
    }

    private async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken!;

        await _tokenLock.WaitAsync();
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken!;

            using var response = await _httpClient.GetAsync(
                "https://edge.microsoft.com/translate/auth");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var shortMsg = responseBody.Length <= 100 ? responseBody : responseBody.Substring(0, 100) + "...";
                throw new HttpRequestException(
                    $"Bing 翻译认证失败 ({(int)response.StatusCode}): {shortMsg}");
            }

            _cachedToken = responseBody;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(8);

            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static string ConvertToMsLang(string lang)
    {
        return lang switch
        {
            "zh" => "zh-Hans",
            "en" => "en",
            "auto" or "" => "",
            _ => lang
        };
    }
}
