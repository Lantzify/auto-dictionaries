﻿using System;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using System.Text.RegularExpressions;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Services
{
	public class AutoDictionariesService : IAutoDictionariesService
	{
		private readonly int _languageCount;
		private readonly List<DictionaryModel> _dictionaryItems;
		private readonly ILocalizationService _localizationService;

		public AutoDictionariesService(ILocalizationService localizationService)
		{
			_localizationService = localizationService;
			_dictionaryItems = GetAllDictionaryItems();
			_languageCount = _localizationService.GetAllLanguages().Count();
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

		public List<DictionaryModel> GetDictionariesFromView(string viewContent)
		{
			var dictionariesCount = Regex.Matches(viewContent, "GetDictionaryValue").Count;
			var dictionaries = Regex.Matches(viewContent, @"(?<=GetDictionaryValue[(])(.*)(?=[)])");
			var listDictionariesModel = GetDictionaryItems(dictionaries.Cast<Match>()
													.Select(m => m.Value)
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

		public List<StaticContentModel> GetStaticContentFromView(string viewContent)
		{
			var staticContents = Regex.Matches(viewContent, @"(?<=>)([\w\s[().,\-:;#$%^&*\[\]\""'+–\/!?|`~]+)(?=<\/)")
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
				string text = System.IO.File.ReadAllText(IOHelper.MapPath(path));

				text = Regex.Replace(text, regex, insert);
				System.IO.File.WriteAllText(IOHelper.MapPath(path), text);

				return true;
			}

			return false;
		}

		public string CreateDictionaryKey(string staticContent, string prefix = null)
		{
			string p = string.IsNullOrEmpty(prefix) ? "" : prefix + "_";
			return $"{p}{staticContent.ToLower().Replace(" ", "_")}";
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

				return dictionaries.Count();
			}

			return 0;
		}

		public DictionaryModel MapToDictionaryModel(IDictionaryItem dictionary)
		{
			if (dictionary != null)
			{
				List<string> translations = dictionary.Translations.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).ToList();

				return new DictionaryModel()
				{
					Id = dictionary.Id,
					Key = dictionary.ItemKey,
					Guid = dictionary.Key,
					Translations = translations,
					Translated = translations.Count() == _languageCount
				};
			}
			return null;
		}
	}
}