using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Services
{
	public class ADTemplateService : IADTemplateService
	{
		private readonly IFileService _fileService;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADTemplateService(IFileService fileService, IAutoDictionariesService autoDictionariesService)
		{
			_fileService = fileService;
			_autoDictionariesService = autoDictionariesService;
		}

		public List<AutoDictionariesModel> GetAllTemplates()
		{
			List<AutoDictionariesModel> templateList = new ();

			var templates = _fileService.GetTemplates();

			if (templates != null && templates.Any())
			{
				foreach (var template in templates)
				{
					templateList.Add(MapToMapToAutoDictionariesModel(template));
				}
			}

			return templateList;
		}

		public AutoDictionariesModel GetTemplate(int templateId)
		{
			var template = _fileService.GetTemplate(templateId);

			return MapToMapToAutoDictionariesModel(template);
		}

		public ITemplate GetUmbracoTemplate(int templateId)
		{
			return _fileService.GetTemplate(templateId);
		}

		public AutoDictionariesModel MapToMapToAutoDictionariesModel(ITemplate template)
		{
			if (template == null) return null;

			var staticContent = _autoDictionariesService.GetStaticContentFromView(template.Content);

			return new AutoDictionariesModel()
			{
				Id = template.Id,
				Alias = template.Alias,
				Name = template.Name,
				Path = template.VirtualPath,
				Type = "Template",
				StaticContent = staticContent,
				Dictionaries = _autoDictionariesService.GetDictionariesFromView(template.Content),
				MatchDictionaries = staticContent.Where(x => x.Dictionary != null).Count()
			};
		}
	}
}