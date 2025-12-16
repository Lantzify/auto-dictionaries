using Umbraco.Cms.Core.Models;
using AutoDictionaries.Models;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Dtos;
using Microsoft.AspNetCore.Hosting;
using AutoDictionaries.Core.Models;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class AutoDictionariesService : IAutoDictionariesService
	{
		private readonly Lazy<Task<int>> _languageCount;
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
			_languageCount = new Lazy<Task<int>>(async () =>
			{
				var languages = await _languageService.GetAllAsync();
				return languages.Count();
			});
		}

		public async Task<List<DictionaryModel>> GetAllDictionaryItems()
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
			var dictionaries = Regex.Matches(viewContent, @"(?<=GetDictionaryValue[(])(.*)(?=[)])");
			var listDictionariesModel = await GetDictionaryItems(dictionaries.Cast<Match>()
													.Select(m => m.Value)
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
			var staticContents = Regex.Matches(viewContent, @"(?<!(=>))(?<=>)(?![.,])([\s\w,.&?!'#\(]+)(.*?)")
										.Cast<Match>()
										.Where(x => !string.IsNullOrWhiteSpace(x.Value) &&
													Regex.Match(x.Value, @"\D+").Length > 1 &&
													!Regex.IsMatch(x.Value.Trim()[0].ToString(), @"\W") &&
													!Regex.IsMatch(x.Value, @"(if *\()"))
										.Select(m => m.Value.Trim())
										.ToList();
			var groupedContent = staticContents.GroupBy(x => x);

			List<StaticContentModel> staticContentModelList = new();

			foreach (var staticContent in groupedContent)
			{
				staticContentModelList.Add(new StaticContentModel()
				{
					Used = staticContent.Count(),
					StaticContent = staticContent.Key,
					Dictionary = GetDictionaryItemFromStaticContent(await GetAllDictionaryItems(), staticContent.Key)
				});
			}

			return staticContentModelList;
		}

		public async Task<DictionaryModel> GetDictionaryItem(string dictionaryKey)
		{
			return await MapToDictionaryModel(await _dictionaryItemService.GetAsync(Regex.Replace(dictionaryKey, @"[\""]", "")));
		}

		public async Task<DictionaryModel> GetDictionaryItem(Guid dictionaryKey)
		{
			return await MapToDictionaryModel(await _dictionaryItemService.GetAsync(dictionaryKey));
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
					if (dictionary.Translations.Contains(staticContent))
					{
						return dictionary;
					}
				}
			}

			return null;
		}

		//public DictionaryModel CreateDictionaryItem(string dictionaryName, string dictionaryValue, int? parentId = null)
		//{
		//	return MapToDictionaryModel(_dictionaryItemService.CreateAsync(dictionaryName, await GetDictionaryItem(parentId ?? -1)?.Guid, dictionaryValue));
		//}

		//public async Task<DictionaryModel> CreateDictionaryItem(string dictionaryName, List<TranslateModel> translations, int? parentId = null)
		//{
		//	var dictionaryItem = _dictionaryItemService.CreateDictionaryItemWithIdentity(dictionaryName, GetDictionaryItem(parentId ?? -1)?.Guid);

		//	foreach (var translation in translations)
		//		_localizationService.AddOrUpdateDictionaryValue(dictionaryItem, translation.Language, translation.TranslatedText);

		//	await _dictionaryItemService.UpdateAsync(dictionaryItem);

		//	return MapToDictionaryModel(dictionaryItem);
		//}

		public string PreviewAddDictionaryItemToView(string viewContent, string path, List<StaticContentDto> staticContent)
		{
			string text = System.IO.File.ReadAllText(_webHostEnvironment.ContentRootFileProvider.GetFileInfo(path).PhysicalPath);
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
				string text = System.IO.File.ReadAllText(_webHostEnvironment.ContentRootFileProvider.GetFileInfo(path).PhysicalPath);
				System.IO.File.WriteAllText(_webHostEnvironment.ContentRootFileProvider.GetFileInfo(path).PhysicalPath, Regex.Replace(text, regex, insert));

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