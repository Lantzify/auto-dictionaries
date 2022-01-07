using AutoDictionaries.Core.Models;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

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
		bool AddDictionaryItemToView(string viewContent, string path, DictionaryModel dictionary, string staticContent);
		int GetDictionaryCountInView(string vierwContent, string dictionaryKey);
		DictionaryModel MapToDictionaryModel(IDictionaryItem dictionary);
	}
}