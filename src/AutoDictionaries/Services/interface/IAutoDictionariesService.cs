using System;
using Umbraco.Cms.Core.Models;
using AutoDictionaries.Models;
using System.Collections.Generic;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IAutoDictionariesService
	{
		Task<List<DictionaryModel>> GetAllDictionaryItems();
		Task GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid);
		Task<List<DictionaryModel>> GetDictionariesFromView(string viewContent);
		Task<List<StaticContentModel>> GetStaticContentFromView(string viewContent);
		Task<DictionaryModel> GetDictionaryItem(string dictionaryKey);
		Task<DictionaryModel> GetDictionaryItem(Guid dictionaryKey);
		Task<List<DictionaryModel>> GetDictionaryItems(string[] dictionaryKeys);
		DictionaryModel GetDictionaryItemFromStaticContent(List<DictionaryModel> dictionaries, string staticContent);
		//DictionaryModel CreateDictionaryItem(string dictionaryName, string dictionaryValue, int? parentId = null);
		//DictionaryModel CreateDictionaryItem(string dictionaryName, List<TranslateModel> translations, int? parentId = null);
        string PreviewAddDictionaryItemToView(string viewContent, string path, List<StaticContentDto> staticContent);
        bool AddDictionaryItemToView(string viewContent, string path, DictionaryModel dictionary, string staticContent);
		int GetDictionaryCountInView(string viewContent, string dictionaryKey);
		Task<DictionaryModel> MapToDictionaryModel(IDictionaryItem dictionary);

		public bool GetTranslateSetting();
		public string GetTranslatorSetting();
		public string GetApiKey();
		public string GetApiEndpoint();
		public string GetApiRegion();
    }
}