namespace AutoDictionaries.Models
{
    public class AutoDictionariesSettings
    {
        public const string AutoDictionaries = "AutoDictionaries";
        public bool Translate { get; set; } = false;
        public string? Translator { get; set; } = "DeepL";
        public string? ApiKey { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? ApiRegion { get; set; }
    }
}