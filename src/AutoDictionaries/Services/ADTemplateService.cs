using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Models;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class ADTemplateService : IADTemplateService
	{
		private readonly ITemplateService _templateService;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADTemplateService(ITemplateService templateService, IAutoDictionariesService autoDictionariesService)
		{
			_templateService = templateService;
			_autoDictionariesService = autoDictionariesService;
		}

		public async Task<List<AutoDictionariesModel>> GetAllTemplates()
		{
			List<AutoDictionariesModel> templateList = new ();

			var templates = await _templateService.GetAllAsync();

			if (templates != null && templates.Any())
			{
				foreach (var template in templates)
				{
					templateList.Add(await MapToMapToAutoDictionariesModel(template));
				}
			}

			return templateList;
		}

		public async Task<AutoDictionariesModel> GetTemplate(Guid templateId)
		{
			var template = await _templateService.GetAsync(templateId);
			if(template == null) return null;

			return await MapToMapToAutoDictionariesModel(template);
		}

		public async Task<ITemplate> GetUmbracoTemplate(Guid templateId) =>  await _templateService.GetAsync(templateId);


		public async Task<AutoDictionariesModel> MapToMapToAutoDictionariesModel(ITemplate template)
		{
			if (template == null)
				return null;

			var staticContent = await _autoDictionariesService.GetStaticContentFromView(template.Content ?? "");

			return new AutoDictionariesModel()
			{
				Id = template.Id,
				Key = template.Key,
				Alias = template.Alias,
				Name = template.Name,
				Path = template.VirtualPath,
				Type = "Template",
				StaticContent = staticContent,
				Dictionaries = await _autoDictionariesService.GetDictionariesFromView(template.Content ?? ""),
				MatchDictionaries = staticContent.Where(x => x.Dictionary != null).Count()
			};
		}
	}
}