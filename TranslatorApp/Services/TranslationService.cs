using System.Text.RegularExpressions;
using TranslatorApp.Models;

namespace TranslatorApp.Services;

public class TranslationService
{
    private static readonly Regex ChineseCharRegex = new Regex(
        @"[\u4e00-\u9fff\u3400-\u4dbf]", RegexOptions.Compiled);

    private readonly BingTranslator _bing;
    private readonly BaiduTranslator _baidu;
    private readonly YoudaoTranslator _youdao;
    private readonly AppConfig _config;

    public TranslationService(AppConfig config)
    {
        _config = config;
        _bing = new BingTranslator();
        _baidu = new BaiduTranslator(config.BaiduAppId, config.BaiduSecretKey);
        _youdao = new YoudaoTranslator(config.YoudaoAppId, config.YoudaoSecretKey);
    }

    public ITranslator GetActiveTranslator()
    {
        if (!_bing.IsConfigured)
            throw new InvalidOperationException("Bing 翻译不可用");

        return _config.PrimaryTranslator switch
        {
            "Baidu" when _baidu.IsConfigured => _baidu,
            "Youdao" when _youdao.IsConfigured => _youdao,
            _ => _bing
        };
    }

    public (string from, string to) DetectLanguages(string text)
    {
        if (!_config.AutoDetectLanguage)
            return ("auto", "zh");

        var isChinese = ChineseCharRegex.IsMatch(text);
        return isChinese ? ("zh", "en") : ("en", "zh");
    }

    public async Task<string> TranslateAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        var translator = GetActiveTranslator();
        var (from, to) = DetectLanguages(text);

        try
        {
            return await translator.TranslateAsync(text.Trim(), from, to);
        }
        catch (Exception ex)
        {
            return $"翻译失败: {ex.Message}";
        }
    }
}
