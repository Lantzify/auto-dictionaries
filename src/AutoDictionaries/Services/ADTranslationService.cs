using DeepL;
using System.Text;
using Newtonsoft.Json;
using AutoDictionaries.Dtos;
using AutoDictionaries.Models;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class ADTranslationService : IADTranslationService
	{
		private readonly ILanguageService _languageService;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADTranslationService(ILanguageService languageService,
			IAutoDictionariesService autoDictionariesService)
		{
			_languageService = languageService;
			_autoDictionariesService = autoDictionariesService;
		}

		public async Task<List<TranslateModel>> Translate(string textToTranslate)
		{
			if (string.IsNullOrEmpty(textToTranslate))
				return new List<TranslateModel>();

			return _autoDictionariesService.GetTranslatorSetting() switch
			{
				"DeepL" => await DeepLTranslate(textToTranslate),
				"MicrosoftTranslation" => await MicrosoftTranslatorTranslate(textToTranslate),
				_ => new List<TranslateModel>(),
			};
		}

		public async Task<List<TranslateModel>> DeepLTranslate(string textToTranslate)
		{
			var translator = new Translator(_autoDictionariesService.GetApiKey());

			string defaultLangISOCode = await _languageService.GetDefaultIsoCodeAsync();

			List<TranslateModel> translations = new List<TranslateModel>();

			foreach (var lang in await _languageService.GetAllAsync())
			{

				if (!lang.IsDefault)
				{
					string iso = lang?.CultureInfo?.TwoLetterISOLanguageName ?? "";
					var translatedText = await translator.TranslateTextAsync(textToTranslate,
						defaultLangISOCode.Contains("-") ? defaultLangISOCode.Split("-").FirstOrDefault() : defaultLangISOCode,
						string.Format("{0}{1}", iso, iso == "en" ? "-US" : string.Empty).ToUpper());

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
					var fullDefaultLangISOCode = await _languageService.GetDefaultIsoCodeAsync();
					var defaultLangISOCode = fullDefaultLangISOCode.Split("-").First();
					object[] body = [new { Text = textToTranslate }];

					string route = string.Empty;
					request.Method = HttpMethod.Post;
					request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
					request.Headers.Add("Ocp-Apim-Subscription-Key", _autoDictionariesService.GetApiKey());

					string region = _autoDictionariesService.GetApiRegion();

					if (!string.IsNullOrEmpty(region))
						request.Headers.Add("Ocp-Apim-Subscription-Region", region);

					foreach (var lang in await _languageService.GetAllAsync())
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
									TranslatedText = result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text
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