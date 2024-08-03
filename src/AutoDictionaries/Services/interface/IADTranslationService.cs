using AutoDictionaries.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
    public interface IADTranslationService
    {
        List<TranslateModel> Translate(string textToTranslate);
        Task<List<TranslateModel>> DeepLTranslate(string textToTranslate);
    }
}