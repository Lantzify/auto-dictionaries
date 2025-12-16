using Umbraco.Cms.Core.Models;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IADTemplateService
	{
		Task<List<AutoDictionariesModel>> GetAllTemplates();
		Task<AutoDictionariesModel> GetTemplate(Guid templateId);
		Task<ITemplate> GetUmbracoTemplate(Guid templateId);
		Task<AutoDictionariesModel> MapToMapToAutoDictionariesModel(ITemplate template);
	}
}