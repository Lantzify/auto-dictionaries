using System.Web.Http;
using Umbraco.Web.WebApi;
using AutoDictionaries.Core.Services.Interfaces;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using Umbraco.Core.Services;
using System;
using AutoDictionaries.Core.Dtos;
using System.Linq;
using Umbraco.Core.IO;

namespace AutoDictionaries.Core.Controllers
{
	public class AutoDictionariesApiController : UmbracoAuthorizedApiController
	{
		private readonly IADTemplateService _adTemplateService;
		private readonly IADPartialViewService _adPartialViewService;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public AutoDictionariesApiController(IADTemplateService adTemplateService, IADPartialViewService adPartialViewService, IAutoDictionariesService autoDictionariesService)
		{
			_adTemplateService = adTemplateService;
			_adPartialViewService = adPartialViewService;
			_autoDictionariesService = autoDictionariesService;
		}

		[HttpGet]
		public List<AutoDictionariesModel> GetAllViews()
		{
			return _adTemplateService.GetAllTemplates().Concat(_adPartialViewService.GetAllPartialViews()).OrderBy(x => x.Name).ToList();
		}

		[HttpGet]
		public AutoDictionariesModel GetView(int id)
		{
			var template = _adTemplateService.GetTemplate(id);

			if (template != null)
				return template;

			var partialView = _adPartialViewService.GetPartialView(id);

			if (partialView != null)
				return partialView;

			return null;
		}

		[HttpGet]
		public List<DictionaryModel> GetAllDictionaryItems()
		{
			return _autoDictionariesService.GetAllDictionaryItems();
		}

		[HttpPost]
		public bool AddExistingDictionaryItemToView(AddExistingDictionaryItemToViewDto dto)
		{
			try
			{
				var dictionary = _autoDictionariesService.GetDictionaryItem(dto.DictionaryId);
				PathContentDto pathContent = GetPathAndContentFromView(dto.AutoDictionariesModel);

				if (!_autoDictionariesService.AddDictionaryItemToView(pathContent.Content, pathContent.Path, dictionary, dto.StaticContent))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		[HttpPost]
		public bool AddNewDictionaryItemToView(AddNewDictionaryItemToViewDto dto)
		{
			try
			{
				var parent = _autoDictionariesService.GetDictionaryItem(dto.StaticContent.Parent);
				var dictionaryName = _autoDictionariesService.CreateDictionaryKey(dto.StaticContent.StaticContent, dto.StaticContent.Parent != "0" ? parent.Key : null); ;
				var dictionary = _autoDictionariesService.CreateDictionaryItem(dictionaryName, dto.StaticContent.StaticContent, parent?.Id);
				PathContentDto pathContent = GetPathAndContentFromView(dto.AutoDictionariesModel);

				if (!_autoDictionariesService.AddDictionaryItemToView(pathContent.Content, pathContent.Path, dictionary, dto.StaticContent.StaticContent))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		private PathContentDto GetPathAndContentFromView(AutoDictionariesModel autoDictionariesModel)
		{
			PathContentDto pathContent = new PathContentDto();

			switch (autoDictionariesModel.Type)
			{
				case "Template":

					var template = _adTemplateService.GetUmbracoTemplate(autoDictionariesModel.Id);
					if (template != null)
					{
						pathContent.Path = template.VirtualPath;
						pathContent.Content = template.Content;
					}

					break;
				case "Partial view":

					var partialView = _adPartialViewService.GetUmbracoPartialView(autoDictionariesModel.Path);
					if (partialView != null)
					{
						pathContent.Path = "/Views/Partials/" + partialView.Path;
						pathContent.Content = partialView.Content;
					}

					break;
			}

			return pathContent;
		}

		[HttpPost]
		public bool SavePartialView(PathContentDto dto)
		{
			try
			{
				System.IO.File.WriteAllText(IOHelper.MapPath(dto.Path), dto.Content);
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}
	}
}