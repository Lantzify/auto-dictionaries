using AutoDictionaries.Core.Models;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IADTemplateService
	{
		List<AutoDictionariesModel> GetAllTemplates();
		AutoDictionariesModel GetTemplate(int templateId);
		ITemplate GetUmbracoTemplate(int templateId);
		AutoDictionariesModel MapToMapToAutoDictionariesModel(ITemplate template);
	}
}