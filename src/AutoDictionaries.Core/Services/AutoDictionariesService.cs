using System.Linq;
using Umbraco.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;
using System;
using System.IO;
using System.Text;
using Umbraco.Core.IO;

namespace AutoDictionaries.Core.Services
{
	public class AutoDictionariesService : IAutoDictionariesService
	{
		private readonly ILocalizationService _localizationService;
		private readonly IFileService _fileService;
		private readonly List<DictionaryModel> _dictionaryItems;
		private int _languageCount;

		public AutoDictionariesService(ILocalizationService localizationService, IFileService fileService)
		{
			_localizationService = localizationService;
			_fileService = fileService;
			_dictionaryItems = GetAllDictionaryItems();
			_languageCount = _localizationService.GetAllLanguages().Count();
		}

		public List<TemplateModel> GetAllTemplates()
		{
			List<TemplateModel> templateList = new List<TemplateModel>();

			var templates = _fileService.GetTemplates();

			if (templates != null && templates.Any())
			{
				foreach (var template in templates)
				{
					templateList.Add(MapToTemplateModel(template));
				}
			}

			return templateList;
		}

		public List<DictionaryModel> GetAllDictionaryItems()
		{
			List<DictionaryModel> dictionariesModel = new List<DictionaryModel>();

			var dictionaries = _localizationService.GetRootDictionaryItems();

			if (dictionaries != null && dictionaries.Any())
			{
				foreach (var dictionary in dictionaries)
				{
					dictionariesModel.Add(GetDictionaryItem(dictionary.Id));

					GetChildrenDictionaryItems(dictionariesModel, dictionary.Key);
				}
			}

			return dictionariesModel;
		}

		public void GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid)
		{
			var dictionaries = _localizationService.GetDictionaryItemChildren(dictionaryGuid);

			if (dictionaries != null && dictionaries.Any())
			{
				foreach (var dictionary in dictionaries)
				{
					dictionariesModel.Add(GetDictionaryItem(dictionary.Id));

					GetChildrenDictionaryItems(dictionariesModel, dictionary.Key);
				}
			}
		}

		public TemplateModel GetTemplate(int templateId)
		{
			var template = _fileService.GetTemplate(templateId);

			return MapToTemplateModel(template);
		}

		public ITemplate GetUmbracoTemplate(int templateId)
		{
			return _fileService.GetTemplate(templateId);
		}

		public List<DictionaryModel> GetTemplateDictionaries(ITemplate template)
		{
			var dictionariesCount = Regex.Matches(template.Content, "GetDictionaryValue").Count;
			var dictionaries = Regex.Matches(template.Content, @"(?<=GetDictionaryValue[(])(.*)(?=[)])");
			var listDictionariesModel = GetDictionaryItems(dictionaries.Cast<Match>()
													.Select(m => m.Value)
													.ToArray());

			if (listDictionariesModel != null && listDictionariesModel.Any())
			{
				foreach (var dictionariesModel in listDictionariesModel)
				{
					dictionariesModel.Used = GetDictionaryCountInTemplate(template, dictionariesModel.Key);
				}
			}

			return listDictionariesModel;
		}

		public List<StaticContentModel> GetTemplateStaticContent(ITemplate template)
		{
			var templateContent = template.Content;

			var staticContents = Regex.Matches(templateContent, @"(?<=>)([\w\s]+)(?=<\/)")
										.Cast<Match>()
										.Where(x => !string.IsNullOrWhiteSpace(x.Value))
										.Select(m => m.Value.Trim())
										.ToList();
			var groupedContent = staticContents.GroupBy(x => x);

			List<StaticContentModel> staticContentModelList = new List<StaticContentModel>();

			foreach (var staticContent in groupedContent)
			{
				staticContentModelList.Add(new StaticContentModel()
				{
					Used = staticContent.Count(),
					StaticContent = staticContent.Key,
					Dictionary = GetDictionaryItemFromStaticContent(_dictionaryItems, staticContent.Key)
				});
			}

			return staticContentModelList;
		}

		public DictionaryModel GetDictionaryItem(string dictionaryKey)
		{
			return MapToDictionaryModel(_localizationService.GetDictionaryItemByKey(Regex.Replace(dictionaryKey, @"[\""]", "")));
		}
		public DictionaryModel GetDictionaryItem(int dictionaryId)
		{
			return MapToDictionaryModel(_localizationService.GetDictionaryItemById(dictionaryId));
		}

		public List<DictionaryModel> GetDictionaryItems(string[] dictionaryKeys)
		{
			List<DictionaryModel> dictionaryItems = new List<DictionaryModel>();

			if (dictionaryKeys != null && dictionaryKeys.Any())
			{
				foreach (var dictionaryKey in dictionaryKeys)
				{
					var dictionaryItem = GetDictionaryItem(dictionaryKey);

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

		public DictionaryModel CreateDictionaryItem(string dictionaryName, string dictionaryValue, int? parentId = null)
		{
			return MapToDictionaryModel(_localizationService.CreateDictionaryItemWithIdentity(dictionaryName, GetDictionaryItem(parentId ?? -1)?.Guid, dictionaryValue));
		}

		public bool AddDictionaryItemToTemplate(ITemplate template, DictionaryModel dictionary, string content)
		{
			string insert = $"@Umbraco.GetDictionaryValue(\"{dictionary.Key}\")";
			var regex = @"(?<=>|)(" + content + @"+)(?=\s|<\/)";

			var staticContent = Regex.Matches(template.Content, regex)
										.Cast<Match>()
										.Select(m => m.Value)
										.ToList();

			if (staticContent.Any())
			{
				string text = System.IO.File.ReadAllText(IOHelper.MapPath(template.VirtualPath));

				text = Regex.Replace(text, regex, insert);
				System.IO.File.WriteAllText(IOHelper.MapPath(template.VirtualPath), text);

				return true;
			}

			return false;
		}

		public string CreateDictionaryKey(string staticContent, string prefix = null)
		{
			string p = string.IsNullOrEmpty(prefix) ? "" : prefix + "_";
			return $"{p}{staticContent.ToLower().Replace(" ", "_")}";
		}

		private int GetDictionaryCountInTemplate(ITemplate template, string dictionaryKey)
		{
			if (template != null)
			{
				string umbDictionary = @"(GetDictionaryValue\(\""" + dictionaryKey + @"\""\))";

				var dictionaries = Regex.Matches(template.Content, umbDictionary)
											.Cast<Match>()
											.Select(m => m.Value)
											.ToList();

				return dictionaries.Count();
			}

			return 0;
		}

		private DictionaryModel MapToDictionaryModel(IDictionaryItem dictionary)
		{
			if (dictionary != null)
			{
				List<string> translations = dictionary.Translations.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).ToList();

				return new DictionaryModel()
				{
					Id = dictionary.Id,
					Key = dictionary.ItemKey,
					Guid = dictionary.Key,
					//Used = GetDictionaryCountInTemplate(template, dictionary.ItemKey),
					Translations = translations,
					Translated = translations.Count() == _languageCount
				};
			}
			return null;
		}

		private TemplateModel MapToTemplateModel(ITemplate template)
		{
			TemplateModel templateModel = new TemplateModel();

			if (template != null)
			{
				templateModel.Id = template.Id;
				templateModel.Alias = template.Alias;
				templateModel.Name = template.Name;
				templateModel.StaticContent = GetTemplateStaticContent(template);
				templateModel.Dictionaries = GetTemplateDictionaries(template);
			}

			return templateModel;
		}
	}
}