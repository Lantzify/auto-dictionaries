using AutoDictionaries.Core.Models;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace AutoDictionaries.Core.Services
{
	public interface IAutoDictionariesService
	{
		List<TemplateModel> GetAllTemplates();
		List<DictionaryModel> GetAllDictionaryItems();
		void GetChildrenDictionaryItems(List<DictionaryModel> dictionariesModel, Guid dictionaryGuid);
		TemplateModel GetTemplate(int templateId);
		ITemplate GetUmbracoTemplate(int templateId);
		List<DictionaryModel> GetTemplateDictionaries(ITemplate template);
		List<StaticContentModel> GetTemplateStaticContent(ITemplate template);
		DictionaryModel GetDictionaryItem(string dictionaryKey);
		DictionaryModel GetDictionaryItem(int dictionaryKey);
		List<DictionaryModel> GetDictionaryItems(string[] dictionaryKeys);
		DictionaryModel GetDictionaryItemFromStaticContent(List<DictionaryModel> dictionaries, string staticContent);
		DictionaryModel CreateDictionaryItem(string dictionaryName, string dictionaryValue, int? parentId);
		string CreateDictionaryKey(string staticContent, string prefix = null);
		bool AddDictionaryItemToTemplate(ITemplate template, DictionaryModel dictionary, string content);
	}
}