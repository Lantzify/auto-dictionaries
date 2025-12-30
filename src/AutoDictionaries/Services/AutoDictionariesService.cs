using AutoDictionaries.Models;
using Umbraco.Cms.Core.Models;
using AutoDictionaries.Helpers;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class AutoDictionariesService : IAutoDictionariesService
	{
		private readonly Lazy<Task<int>> _languageCount;
		private Lazy<Task<List<DictionaryModel>>> _allDictionaryItems;
		private readonly ILanguageService _languageService;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IDictionaryItemService _dictionaryItemService;
		private readonly IOptions<AutoDictionariesSettings> _adSettings;

		public bool GetTranslateSetting() => _adSettings.Value.Translate;
		public string GetTranslatorSetting() => _adSettings?.Value?.Translator ?? string.Empty;
		public string GetApiEndpoint() => _adSettings?.Value?.ApiEndpoint ?? string.Empty;
		public string GetApiKey() => _adSettings?.Value?.ApiKey ?? string.Empty;
		public string GetApiRegion() => _adSettings?.Value?.ApiRegion ?? string.Empty;

		public AutoDictionariesService(ILanguageService languageService,
			IWebHostEnvironment webHostEnvironment,
			IOptions<AutoDictionariesSettings> adSettings,
			IDictionaryItemService dictionaryItemService)
		{
			_webHostEnvironment = webHostEnvironment;
			_adSettings = adSettings;
			_languageService = languageService;
			_dictionaryItemService = dictionaryItemService;
			_allDictionaryItems = new Lazy<Task<List<DictionaryModel>>>(GetAllDictionaryItems);
		
			_languageCount = new Lazy<Task<int>>(async () =>
			{
				var languages = await _languageService.GetAllAsync();
				return languages.Count();
			});
		}


		public Task<List<DictionaryModel>> LazyAllDictionaryItems() => _allDictionaryItems.Value;

		public void RefreshGetAllDictionaryItems() => _allDictionaryItems = new Lazy<Task<List<DictionaryModel>>>(GetAllDictionaryItems);


		private async Task<List<DictionaryModel>> GetAllDictionaryItems()
		{
			List<DictionaryModel> dictionariesModel = new();

			var dictionaries = await _dictionaryItemService.GetAtRootAsync();

			if (dictionaries != null && dictionaries.Any())
			{
				foreach (var dictionary in dictionaries)
				{
					dictionariesModel.Add(await GetDictionaryItem(dictionary.Key));

					await GetChildrenDictionaryItems(dictionariesModel, dictionary.Key);
				}
			}

			return dictionariesModel;
		}

		public async Task GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid)
		{
			var dictionaries = await _dictionaryItemService.GetChildrenAsync(dictionaryGuid);

			if (dictionaries != null && dictionaries.Any())
			{
				foreach (var dictionary in dictionaries)
				{
					dictionariesModel.Add(await GetDictionaryItem(dictionary.Key));

					await GetChildrenDictionaryItems(dictionariesModel, dictionary.Key);
				}
			}
		}

		public async Task<List<DictionaryModel>> GetDictionariesFromView(string viewContent)
		{
			var dictionariesCount = Regex.Matches(viewContent, "GetDictionaryValue").Count;
			var dictionaries = Regex.Matches(viewContent, @"\bGetDictionaryValue\s*\(\s*(['""])(?<key>[^'""]+)\1\s*\)");
			var listDictionariesModel = await GetDictionaryItems(dictionaries.Cast<Match>()
													.Select(m => m.Groups["key"].Value)
													.Distinct()
													.ToArray());

			if (listDictionariesModel != null && listDictionariesModel.Any())
			{
				foreach (var dictionariesModel in listDictionariesModel)
				{
					dictionariesModel.Used = GetDictionaryCountInView(viewContent, dictionariesModel.Key);
				}
			}

			return listDictionariesModel;
		}

		public async Task<List<StaticContentModel>> GetStaticContentFromView(string viewContent)
		{
			var staticContents = FindStaticContentHelper.ExtractStaticContent(viewContent);

			var groupedContent = staticContents.GroupBy(x => x);

			List<StaticContentModel> staticContentModelList = new();

			foreach (var staticContent in groupedContent)
			{
				staticContentModelList.Add(new StaticContentModel()
				{
					Used = staticContent.Count(),
					StaticContent = staticContent.Key,
					Dictionary = GetDictionaryItemFromStaticContent(await LazyAllDictionaryItems(), staticContent.Key)
				});
			}

			return staticContentModelList;
		}

		public async Task<DictionaryModel> GetDictionaryItem(string dictionaryKey)
		{
			var dictionaryItem = await _dictionaryItemService.GetAsync(dictionaryKey);
			if(dictionaryItem == null)
				return null;

			return await MapToDictionaryModel(dictionaryItem);
		}

		public async Task<DictionaryModel> GetDictionaryItem(Guid dictionaryKey)
		{
			var dictionaryItem = await _dictionaryItemService.GetAsync(dictionaryKey);
			if (dictionaryItem == null)
				return null;

			return await MapToDictionaryModel(dictionaryItem);
		}

		public async Task<List<DictionaryModel>> GetDictionaryItems(string[] dictionaryKeys)
		{
			List<DictionaryModel> dictionaryItems = new();

			if (dictionaryKeys != null && dictionaryKeys.Any())
			{
				foreach (var dictionaryKey in dictionaryKeys)
				{
					var dictionaryItem = await GetDictionaryItem(dictionaryKey);

					if (dictionaryItem != null)
					{
						dictionaryItems.Add(dictionaryItem);
					}
				}
			}

			return dictionaryItems;
		}

		public DictionaryModel GetDictionaryItemFromStaticContent(List<DictionaryModel> dictionaries, string staticContent)
		{
			foreach (var dictionary in dictionaries)
			{
				if (dictionary != null)
				{
					if (dictionary.Translations != null && dictionary.Translations.Contains(staticContent))
					{
						return dictionary;
					}
				}
			}

			return null;
		}

		public async Task<DictionaryModel> CreateDictionaryItem(string dictionaryName, string dictionaryValue, Guid userKey, DictionaryModel? parent = null)
		{
			var defaultLanguage = await _languageService.GetDefaultLanguageAsync();
			if (defaultLanguage == null)
				return null;

			DictionaryItem dictionaryItem = new DictionaryItem(dictionaryName)
			{
				ParentId = parent?.Guid ?? null,
				Translations = new List<DictionaryTranslation>
				{
					new DictionaryTranslation(defaultLanguage, dictionaryValue)
				} 
			};

			var attempt = await _dictionaryItemService.CreateAsync(dictionaryItem, userKey);

			if (!attempt.Success)
				return null;

			return await MapToDictionaryModel(attempt.Result);
		}

		public async Task<DictionaryModel> CreateDictionaryItem(string dictionaryName, List<TranslateModel> translations, Guid userKey, DictionaryModel? parent = null)
		{
			var dicTranslations = new List<DictionaryTranslation>();

			foreach (var translation in translations)
				dicTranslations.Add(new DictionaryTranslation(translation.Language, translation.TranslatedText));

			DictionaryItem dictionaryItem = new DictionaryItem(dictionaryName)
			{
				ParentId = parent?.Guid ?? null,
				Translations = dicTranslations
			};

			var attempt = await _dictionaryItemService.CreateAsync(dictionaryItem, userKey);
			
			if (!attempt.Success)
				return null;

			return await MapToDictionaryModel(attempt.Result);
		}

		public string PreviewAddDictionaryItemToView(string viewContent, string path, List<StaticContentDto> staticContent)
		{
			var physicalPath = _webHostEnvironment.ContentRootFileProvider?.GetFileInfo(path).PhysicalPath;
			string text = System.IO.File.ReadAllText(physicalPath ?? string.Empty);
			foreach (var item in staticContent)
			{
				string insert = $"@Umbraco.GetDictionaryValue(\"{item.SafeAlias}\")";
				var regex = @"(?<=>|)(" + item.StaticContent + @"+)(?=\s|<\/)";

				var staticContentInView = Regex.Matches(viewContent, regex)
											.Cast<Match>()
											.Select(m => m.Value)
											.ToList();
				if (staticContentInView.Any())
					text = Regex.Replace(text, regex, insert);
			}

			return text;
		}

		public bool AddDictionaryItemToView(string viewContent, string path, DictionaryModel dictionary, string staticContent)
		{
			string insert = $"@Umbraco.GetDictionaryValue(\"{dictionary.Key}\")";
			var regex = @"(?<=>|)(" + staticContent + @"+)(?=\s|<\/)";

			var staticContentInView = Regex.Matches(viewContent, regex)
										.Cast<Match>()
										.Select(m => m.Value)
										.ToList();

			if (staticContentInView.Any())
			{
				var physicalPath = _webHostEnvironment?.ContentRootFileProvider?.GetFileInfo(path)?.PhysicalPath ?? string.Empty;
				if (string.IsNullOrEmpty(physicalPath))
					return false;

				string text = System.IO.File.ReadAllText(physicalPath);
				System.IO.File.WriteAllText(physicalPath, Regex.Replace(text, regex, insert));

				return true;
			}

			return false;
		}

		public int GetDictionaryCountInView(string vierwContent, string dictionaryKey)
		{
			if (!string.IsNullOrEmpty(vierwContent))
			{
				string umbDictionary = @"(GetDictionaryValue\(\""" + dictionaryKey + @"\""\))";

				var dictionaries = Regex.Matches(vierwContent, umbDictionary)
											.Cast<Match>()
											.Select(m => m.Value)
											.ToList();

				return dictionaries.Count;
			}

			return 0;
		}


		private Task<int> GetLanguageCountAsync() => _languageCount.Value;
		public async Task<DictionaryModel> MapToDictionaryModel(IDictionaryItem dictionary)
		{
			if (dictionary != null)
			{
				List<string> translations = dictionary.Translations.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).ToList();

				var languageCount = await GetLanguageCountAsync();

				return new DictionaryModel()
				{
					Id = dictionary.Id,
					Key = dictionary.ItemKey,
					Guid = dictionary.Key,
					Translations = translations,
					Translated = translations.Count == languageCount
				};
			}
			return null;
		}
	}
}