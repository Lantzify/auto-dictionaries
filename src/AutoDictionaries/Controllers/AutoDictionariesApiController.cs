using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Strings;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;
using Umbraco.Cms.Web.BackOffice.Controllers;
using AutoDictionaries.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoDictionaries.Core.Controllers
{
    public class AutoDictionariesApiController : UmbracoAuthorizedApiController
    {
        private readonly ILogger<AutoDictionariesApiController> _logger;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IADTemplateService _adTemplateService;
        private readonly IADTranslationService _adTranslationService;
        private readonly IADPartialViewService _adPartialViewService;
        private readonly IAutoDictionariesService _autoDictionariesService;
        
        public AutoDictionariesApiController(ILogger<AutoDictionariesApiController> logger,
            IShortStringHelper shortStringHelper, 
            IADTemplateService adTemplateService,
            IADTranslationService adTranslationService,
            IADPartialViewService adPartialViewService,
            IAutoDictionariesService autoDictionariesService)
        {
            _logger = logger;
            _shortStringHelper = shortStringHelper;
            _adTemplateService = adTemplateService;
            _adTranslationService = adTranslationService;
            _adPartialViewService = adPartialViewService;
            _autoDictionariesService = autoDictionariesService;
        }

        [HttpGet]
        public List<AutoDictionariesModel> GetAllViews() => _adTemplateService.GetAllTemplates().Concat(_adPartialViewService.GetAllPartialViews()).OrderBy(x => x.Name).ToList();

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
        public List<DictionaryModel> GetAllDictionaryItems() => _autoDictionariesService.GetAllDictionaryItems();

        [HttpGet]
        public string GetPreview(string id) => _adTemplateService.GetUmbracoTemplate(int.Parse(id)).Content;


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
                _logger.LogError(ex, "Failed to add existing dictionary item to view {0}", ex.Message);
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
                DictionaryModel dictionary;


                if (!dto.Tanslate)
                {
                    dictionary = _autoDictionariesService.CreateDictionaryItem(dictionaryName, dto.StaticContent.StaticContent, parent?.Id);
                }
                else
                {
                    dictionary = _autoDictionariesService.CreateDictionaryItem(dictionaryName, _adTranslationService.Translate(dto.StaticContent.StaticContent), parent?.Id);
                }

                PathContentDto pathContent = GetPathAndContentFromView(dto.AutoDictionariesModel);

                if (!_autoDictionariesService.AddDictionaryItemToView(pathContent.Content, pathContent.Path, dictionary, dto.StaticContent.StaticContent))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new dictionary item to view {0}", ex.Message);
                return false;
            }

            return true;
        }

        [HttpGet]
        public bool GetTranslateSetting() => _autoDictionariesService.GetTranslateSetting();

        [HttpGet]
        public string GetTranslatorSetting() => _autoDictionariesService.GetTranslatorSetting();

        [HttpGet]
        public string GetApiKey() => _autoDictionariesService.GetApiKey();

        [HttpGet]
        public string GetApiEndpoint() => _autoDictionariesService.GetApiEndpoint();

        [HttpGet]
        public string GetApiRegion() => _autoDictionariesService.GetApiRegion();

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