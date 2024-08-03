using Umbraco.Cms.Core.Models;

namespace AutoDictionaries.Models
{
    public class TranslateModel
    {
        public ILanguage? Language { get; set; }
        public string? TranslatedText { get; set; }
    }
}