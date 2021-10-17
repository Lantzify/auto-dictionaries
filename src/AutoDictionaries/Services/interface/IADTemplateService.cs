using Umbraco.Cms.Core.Models;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;

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