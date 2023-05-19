using System;
using System.Linq;
using Umbraco.Cms.Core.IO;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Cms.Web.BackOffice.Controllers;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Controllers
{
    public class AutoDictionariesApiController : UmbracoAuthorizedApiController
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IADTemplateService _adTemplateService;
        private readonly IADPartialViewService _adPartialViewService;
        private readonly IAutoDictionariesService _autoDictionariesService;

        public AutoDictionariesApiController(IShortStringHelper shortStringHelper, IADTemplateService adTemplateService, IADPartialViewService adPartialViewService, IAutoDictionariesService autoDictionariesService)
        {
            _shortStringHelper = shortStringHelper;
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

        [HttpGet]
        public string GetPreview(string id)
        {
            return _adTemplateService.GetUmbracoTemplate(int.Parse(id)).Content;

        }

        [HttpPost]
        public string[] PreviewAddExistingDictionaryItemToView(AddExistingDictionaryItemToViewDto dto)
        {

            var dictionary = _autoDictionariesService.GetDictionaryItem(dto.DictionaryId);
            PathContentDto pathContent = GetPathAndContentFromView(dto.AutoDictionariesModel);

            List<StaticContentDto> staticContent = new List<StaticContentDto>()
            {
                new StaticContentDto()
                {
                    SafeAlias = dictionary.Key,
                    StaticContent = dto.StaticContent
                }
            };

            return new string[]
            {
                pathContent.Content,
                _autoDictionariesService.PreviewAddDictionaryItemToView(pathContent.Content, pathContent.Path, staticContent)
            };
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
        public string[] PreviewAddNewDictionaryItemToView(PreviewAddNewDictionaryItemToViewDto dto)
        {
            foreach (var staticContent in dto.StaticContent)
                staticContent.SafeAlias = $"{(staticContent.Parent != "0" ? staticContent.Parent + "_" : null)}{_shortStringHelper.CleanStringForSafeAlias(staticContent.StaticContent)}";

            PathContentDto pathContent = GetPathAndContentFromView(dto.AutoDictionariesModel);

            return new string[]
            {
                pathContent.Content,
                _autoDictionariesService.PreviewAddDictionaryItemToView(pathContent.Content, pathContent.Path, dto.StaticContent)
            };
        }

        [HttpPost]
        public bool AddNewDictionaryItemToView(AddNewDictionaryItemToViewDto dto)
        {
            try
            {
                var parent = _autoDictionariesService.GetDictionaryItem(dto.StaticContent.Parent);
                var dictionaryName = $"{(dto.StaticContent.Parent != "0" ? dto.StaticContent.Parent + "_" : null)}{_shortStringHelper.CleanStringForSafeAlias(dto.StaticContent.StaticContent)}";
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
            PathContentDto pathContent = new();

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
    }
}