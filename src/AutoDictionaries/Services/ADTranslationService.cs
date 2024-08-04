using AutoDictionaries.Core.Services.Interfaces;
using AutoDictionaries.Dtos;
using AutoDictionaries.Models;
using DeepL;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Text;
using Umbraco.Cms.Core.Services;

namespace AutoDictionaries.Services
{
    public class ADTranslationService : IADTranslationService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IAutoDictionariesService _autoDictionariesService;

        public ADTranslationService(ILocalizationService localizationService,
            IAutoDictionariesService autoDictionariesService)
        {
            _localizationService = localizationService;
            _autoDictionariesService = autoDictionariesService;
        }

        public List<TranslateModel> Translate(string textToTranslate)
        {
            return _autoDictionariesService.GetTranslatorSetting() switch
            {
                "DeepL" => DeepLTranslate(textToTranslate).Result,
                "MicrosoftTranslation" => MicrosoftTranslatorTranslate(textToTranslate).Result,
                _ => new List<TranslateModel>(),
            };
        }

        public async Task<List<TranslateModel>> DeepLTranslate(string textToTranslate)
        {
            var translator = new Translator(_autoDictionariesService.GetApiKey());

            var defaultLangISOCode = _localizationService.GetDefaultLanguageIsoCode().Split("-").First();

            List<TranslateModel> translations = new List<TranslateModel>();

            foreach (var lang in _localizationService.GetAllLanguages())
            {

                if (!lang.IsDefault)
                {
                    var translatedText = await translator.TranslateTextAsync(textToTranslate,
                        defaultLangISOCode,
                        lang.IsoCode.Split("-").First());

                    translations.Add(new TranslateModel
                    {
                        Language = lang,
                        TranslatedText = translatedText.Text
                    });
                }
                else
                {
                    translations.Add(new TranslateModel
                    {
                        Language = lang,
                        TranslatedText = textToTranslate
                    });
                }
            }

            return translations;
        }

        public async Task<List<TranslateModel>> MicrosoftTranslatorTranslate(string textToTranslate)
        {
            List<TranslateModel> translations = new List<TranslateModel>();

            using (HttpClient client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    var defaultLangISOCode = _localizationService.GetDefaultLanguageIsoCode().Split("-").First();
                    object[] body = [new { Text = textToTranslate }];

                    string route = string.Empty;
                    request.Method = HttpMethod.Post;
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _autoDictionariesService.GetApiKey());

                    string region = _autoDictionariesService.GetApiRegion();

                    if(!string.IsNullOrEmpty(region))
                        request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                    foreach (var lang in _localizationService.GetAllLanguages())
                    {
                        if (!lang.IsDefault)
                        {
                            route = string.Format("translate?api-version=3.0&from={0}&to={1}", defaultLangISOCode, lang.IsoCode.Split("-").First());
                            request.RequestUri = new Uri(_autoDictionariesService.GetApiEndpoint() + route);

                            HttpResponseMessage response = await client.SendAsync(request);
                            if (response.IsSuccessStatusCode)
                            {
                                string stringResult = await response.Content.ReadAsStringAsync();
                                var result = JsonConvert.DeserializeObject<List<TranslationResponse>>(stringResult);

                                translations.Add(new TranslateModel
                                {
                                    Language = lang,
                                    TranslatedText = result.FirstOrDefault()?.Translations.FirstOrDefault()?.Text
                                });
                            }
                        }
                        else
                        {
                            translations.Add(new TranslateModel
                            {
                                Language = lang,
                                TranslatedText = textToTranslate
                            });
                        }
                    }
                }
            }

            return translations;
        }
    }
}