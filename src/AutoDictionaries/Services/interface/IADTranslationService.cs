using AutoDictionaries.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
    public interface IADTranslationService
    {
        Task<List<TranslateModel>> Translate(string textToTranslate);
        Task<List<TranslateModel>> DeepLTranslate(string textToTranslate);
    }
}