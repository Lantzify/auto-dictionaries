using System.Web.Http;
using Umbraco.Web.WebApi;
using AutoDictionaries.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using Umbraco.Core.Services;
using System;
using AutoDictionaries.Core.Dtos;

namespace AutoDictionaries.Core.Controllers
{
	public class AutoDictionariesApiController : UmbracoAuthorizedApiController
	{
		private readonly IAutoDictionariesService _autoDictionariesService;

		public AutoDictionariesApiController(IAutoDictionariesService autoDictionariesService)
		{
			_autoDictionariesService = autoDictionariesService;
		}

		[HttpGet]
		public List<TemplateModel> GetAllTemplates()
		{
			return _autoDictionariesService.GetAllTemplates();
		}

		[HttpGet]
		public TemplateModel GetTemplate(int templateId)
		{
			return _autoDictionariesService.GetTemplate(templateId);
		}
		[HttpGet]
		public List<DictionaryModel> GetAllDictionaryItems()
		{
			return _autoDictionariesService.GetAllDictionaryItems();
		}

		[HttpPost]
		public bool AddExistingDictionaryItemToTemplate(AddExistingDictionaryItemToTemplate dto)
		{
			try
			{
				var dictionary = _autoDictionariesService.GetDictionaryItem(dto.DictionaryId);
				var template = _autoDictionariesService.GetUmbracoTemplate(dto.Template.Id);
				_autoDictionariesService.AddDictionaryItemToTemplate(template, dictionary, dto.StaticContent);
			}
			catch (Exception ex)
			{
				return false;
			}

			return true;
		}

		[HttpPost]
		public bool AddNewDictionaryItemToTemplate(AddNewDictionaryItemToTemplateDto dto)
		{
			try
			{
				var parent = _autoDictionariesService.GetDictionaryItem(dto.StaticContent.Parent);
				var dictionaryName = _autoDictionariesService.CreateDictionaryKey(dto.StaticContent.StaticContent, dto.StaticContent.Parent != "0" ? parent.Key : null); ;
				var dictionary = _autoDictionariesService.CreateDictionaryItem(dictionaryName, dto.StaticContent.StaticContent, parent?.Id);
				var template = _autoDictionariesService.GetUmbracoTemplate(dto.Template.Id);

				if (!_autoDictionariesService.AddDictionaryItemToTemplate(template, dictionary, dto.StaticContent.StaticContent))
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
	}
}