using Asp.Versioning;
using Umbraco.Cms.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Dtos;
using AutoDictionaries.Core.Models;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Common.Attributes;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using AutoDictionaries.Core.Services.Interfaces;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;

namespace AutoDictionaries.Core.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
    [MapToApi("autoDictionaries")]
	[ApiExplorerSettings(GroupName = "autoDictionaries")]
	[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
	public class AutoDictionariesApiController : Controller
    {

        private readonly ILogger<AutoDictionariesApiController> _logger;
		private readonly ILanguageService _languageService;
		private readonly IShortStringHelper _shortStringHelper;
        private readonly IADTemplateService _adTemplateService;      
        private readonly IADTranslationService _adTranslationService;
        private readonly IADPartialViewService _adPartialViewService;
		private readonly IDictionaryItemService _dictionaryItemService;
		private readonly IAutoDictionariesService _autoDictionariesService;
		private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

		public AutoDictionariesApiController(ILogger<AutoDictionariesApiController> logger,
			ILanguageService languageService,
			IShortStringHelper shortStringHelper,
            IADTemplateService adTemplateService,
            IADTranslationService adTranslationService,
            IADPartialViewService adPartialViewService,
			IDictionaryItemService dictionaryItemService,
			IAutoDictionariesService autoDictionariesService,
			IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        {
            _logger = logger;
			_languageService = languageService;
            _shortStringHelper = shortStringHelper;
            _adTemplateService = adTemplateService;
            _adTranslationService = adTranslationService;
            _adPartialViewService = adPartialViewService;
            _dictionaryItemService = dictionaryItemService;
            _autoDictionariesService = autoDictionariesService;
			_backOfficeSecurityAccessor = backOfficeSecurityAccessor;
		}

        [HttpGet("get-all-views")]
        public async Task<List<AutoDictionariesModel>> GetAllViews() 
        {
            var templates = await _adTemplateService.GetAllTemplates();
            var partialViews = await _adPartialViewService.GetAllPartialViews();

            return templates.Concat(partialViews).OrderBy(x => x.Name).ToList();
		}


		[HttpGet("get-view/{id}")]
		public async Task<AutoDictionariesModel> GetView(string id)
		{
			if (Guid.TryParse(id, out Guid key))
			{
				var template = await _adTemplateService.GetTemplate(key);
				if (template != null)
					return template;
			}

			if (int.TryParse(id, out int intId))
			{
				var partialView = await _adPartialViewService.GetPartialView(intId);
				if (partialView != null)
					return partialView;
			}

			return null;
		}

		[HttpGet("get-all-dictionary-items")]
        public async Task<List<DictionaryModel>> GetAllDictionaryItems() => await _autoDictionariesService.LazyAllDictionaryItems();

        [HttpGet("get-preview/{id}")]
        public async Task<string> GetPreview(string id) 
        {
            var template = await _adTemplateService.GetUmbracoTemplate(Guid.Parse(id));
			return template?.Content ?? "";
		} 


        [HttpPost("preview-add-existing-dictionary-item")]
        public async Task<ActionResult<string[]>> PreviewAddExistingDictionaryItemToView(AddExistingDictionaryItemToViewDto dto)
        {
			if (dto.AutoDictionariesModel is null)
				return BadRequest("AutoDictionariesModel is required.");

			var dictionary = await _autoDictionariesService.GetDictionaryItem(dto.DictionaryKey);
            PathContentDto pathContent = await GetPathAndContentFromView(dto.AutoDictionariesModel);

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

        [HttpPost("add-existing-dictionary-item")]
        public async Task<ActionResult<bool>> AddExistingDictionaryItemToView(AddExistingDictionaryItemToViewDto dto)
        {
            try
            {
				if (dto.AutoDictionariesModel is null)
					return BadRequest("AutoDictionariesModel is required.");

				var dictionary = await _autoDictionariesService.GetDictionaryItem(dto.DictionaryKey);
                PathContentDto pathContent = await GetPathAndContentFromView(dto.AutoDictionariesModel);

                if (!_autoDictionariesService.AddDictionaryItemToView(pathContent?.Content ?? "", pathContent?.Path ?? "", dictionary, dto?.StaticContent ?? ""))
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

        [HttpPost("preview-add-new-dictionary-item")]
        public async Task<ActionResult<string[]>> PreviewAddNewDictionaryItemToView(PreviewAddNewDictionaryItemToViewDto dto)
        {
			if (dto.AutoDictionariesModel is null) 
                return BadRequest("AutoDictionariesModel is required.");

			if (dto.StaticContent is null || dto.StaticContent.Count == 0) 
                return BadRequest("StaticContent is required.");

            string culture = await _languageService.GetDefaultIsoCodeAsync();
			foreach (var staticContent in dto.StaticContent)
            {
				var parentPrefix = string.IsNullOrWhiteSpace(staticContent.Parent) ? "" : staticContent.Parent + "_";
				staticContent.SafeAlias = parentPrefix + _shortStringHelper.CleanStringForUrlSegment(staticContent.StaticContent ?? "", culture);
			}

			var pathContent = await GetPathAndContentFromView(dto.AutoDictionariesModel);

			return Ok(new []
            {
                pathContent.Content,
                _autoDictionariesService.PreviewAddDictionaryItemToView(
                    pathContent.Content,
                    pathContent.Path, 
                    dto.StaticContent)
            });
        }

        [HttpPost("add-new-dictionary-item")]
        public async Task<ActionResult<bool>> AddNewDictionaryItemToView(AddNewDictionaryItemToViewDto dto)
        {
            try
            {
				if (dto.AutoDictionariesModel is null)
					return BadRequest("AutoDictionariesModel is required.");

				if (dto.StaticContent == null)
					return BadRequest("Static Content is required.");

				var parent = await _autoDictionariesService.GetDictionaryItem(dto.StaticContent.Parent);
				var parentPrefix = string.IsNullOrWhiteSpace(dto.StaticContent.Parent) ? "" : dto.StaticContent.Parent + "_";
				string culture = await _languageService.GetDefaultIsoCodeAsync();
				var dictionaryName = parentPrefix + _shortStringHelper.CleanStringForUrlSegment(dto.StaticContent.StaticContent ?? "", culture);

				var userKey = CurrentUserKeyOrNull();
				if (userKey is null) 
                    return Unauthorized();

				DictionaryModel dictionary;
				if (!dto.Translate)
                {
                    dictionary = await _autoDictionariesService.CreateDictionaryItem(dictionaryName, dto.StaticContent.StaticContent, userKey.Value, parent);
                }
                else
                {
                    if (string.IsNullOrEmpty(GetApiKeySetting()))
						return BadRequest("No API key found");

					dictionary = await _autoDictionariesService.CreateDictionaryItem(dictionaryName, await _adTranslationService.Translate(dto.StaticContent.StaticContent), userKey.Value, parent);
                }

                PathContentDto pathContent = await GetPathAndContentFromView(dto.AutoDictionariesModel);

                if (!_autoDictionariesService.AddDictionaryItemToView(pathContent.Content, pathContent.Path, dictionary, dto.StaticContent.StaticContent))
                    return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new dictionary item to view {0}", ex.Message);
                return false;
            }

            return true;
        }

        [HttpGet("translate-dictionary-item/{id}")]
        public async Task<ActionResult<bool>> TranslateDictionaryItem(Guid id)
        {
            try
            {
				var userKey = CurrentUserKeyOrNull();
				if (userKey is null)
					return Unauthorized();

				var defaultLang = await _languageService.GetDefaultIsoCodeAsync();

                var item = await _dictionaryItemService.GetAsync(id);
                if (item == null)
                    return false;

				var defaultText = item.Translations.FirstOrDefault(x => x.LanguageIsoCode == defaultLang);

                if (string.IsNullOrEmpty(defaultText?.Value))
                    return false;

                var newTranslations = await _adTranslationService.Translate(defaultText.Value);

                var translations = item.Translations.ToList();
				translations.AddRange(newTranslations.Where(x => x.Language?.CultureInfo?.Name != defaultLang).Select(x => new DictionaryTranslation(x.Language, x.TranslatedText)));

                item.Translations = translations;

               

				var attempt = await _dictionaryItemService.UpdateAsync(item, userKey.Value);
                if (!attempt.Success)
                    return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to translate dictionary item {0}", ex.Message);
                return false;
            }

            return true;
        }

        [HttpGet("get-translate-setting")]
        public bool GetTranslateSetting() => _autoDictionariesService.GetTranslateSetting();

        [HttpGet("get-translator-setting")]
        public string GetTranslatorSetting() => _autoDictionariesService.GetTranslatorSetting();

        [HttpGet("get-api-key")]
        public string GetApiKeySetting() => _autoDictionariesService.GetApiKey();

        [HttpGet("get-api-endpoint")]
        public string GetApiEndpointSetting() => _autoDictionariesService.GetApiEndpoint();

        [HttpGet("get-api-region")]
        public string GetApiRegionSetting() => _autoDictionariesService.GetApiRegion();

        private async Task<PathContentDto> GetPathAndContentFromView(AutoDictionariesModel autoDictionariesModel)
        {
            PathContentDto pathContent = new();

            switch (autoDictionariesModel.Type)
            {
                case "Template":

                    var template = await _adTemplateService.GetUmbracoTemplate(autoDictionariesModel.Key);
                    if (template != null)
                    {
                        pathContent.Path = template.VirtualPath;
                        pathContent.Content = template.Content;
                    }

                    break;
                case "Partial view":

                    var partialView = await _adPartialViewService.GetUmbracoPartialView(autoDictionariesModel?.Path ?? "");
                    if (partialView != null)
                    {
                        pathContent.Path = "/Views/Partials/" + partialView.Path;
                        pathContent.Content = partialView.Content;
                    }

                    break;
            }

            return pathContent;
        }


		[HttpGet("Children")]
		public async Task<ActionResult<PagedViewModel<AutoDictionariesModel>>> GetChildren()
		{    
			return Ok(new PagedViewModel<AutoDictionariesModel>
			{
				Items = await GetAllViews(),
				Total = 100
			});
		}

		private Guid? CurrentUserKeyOrNull() => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key;
	}
}