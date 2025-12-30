using Umbraco.Cms.Core.Models;
using AutoDictionaries.Models;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IAutoDictionariesService
	{

		void RefreshGetAllDictionaryItems();
		Task<List<DictionaryModel>> LazyAllDictionaryItems();
		Task GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid);
		Task<List<DictionaryModel>> GetDictionariesFromView(string viewContent);
		Task<List<StaticContentModel>> GetStaticContentFromView(string viewContent);
		Task<DictionaryModel> GetDictionaryItem(string dictionaryKey);
		Task<DictionaryModel> GetDictionaryItem(Guid dictionaryKey);
		Task<List<DictionaryModel>> GetDictionaryItems(string[] dictionaryKeys);
		DictionaryModel GetDictionaryItemFromStaticContent(List<DictionaryModel> dictionaries, string staticContent);
		Task<DictionaryModel> CreateDictionaryItem(string dictionaryName, string dictionaryValue, Guid userKey, DictionaryModel? parent = null);   
		Task<DictionaryModel> CreateDictionaryItem(string dictionaryName, List<TranslateModel> translations, Guid userKey, DictionaryModel? parent = null);

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