using System;
using Umbraco.Cms.Core.Models;
using System.Collections.Generic;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;
using AutoDictionaries.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IAutoDictionariesService
	{
		List<DictionaryModel> GetAllDictionaryItems();
		void GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid);
		List<DictionaryModel> GetDictionariesFromView(string viewContent);
		List<StaticContentModel> GetStaticContentFromView(string viewContent);
		DictionaryModel GetDictionaryItem(string dictionaryKey);
		DictionaryModel GetDictionaryItem(int dictionaryId);
		List<DictionaryModel> GetDictionaryItems(string[] dictionaryKeys);
		DictionaryModel GetDictionaryItemFromStaticContent(List<DictionaryModel> dictionaries, string staticContent);
		DictionaryModel CreateDictionaryItem(string dictionaryName, string dictionaryValue, int? parentId = null);
		DictionaryModel CreateDictionaryItem(string dictionaryName, List<TranslateModel> translations, int? parentId = null);
        string PreviewAddDictionaryItemToView(string viewContent, string path, List<StaticContentDto> staticContent);
        bool AddDictionaryItemToView(string viewContent, string path, DictionaryModel dictionary, string staticContent);
		int GetDictionaryCountInView(string viewContent, string dictionaryKey);
		DictionaryModel MapToDictionaryModel(IDictionaryItem dictionary);

		public bool GetTranslateSetting();
		public string GetTranslatorSetting();
		public string GetApiKey();
		public string GetApiEndpoint();
		public string GetApiRegion();
    }
}