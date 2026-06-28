namespace TranslatorApp.Services;

public interface ITranslator
{
    string Name { get; }
    bool IsConfigured { get; }
    Task<string> TranslateAsync(string text, string fromLang, string toLang);
}
