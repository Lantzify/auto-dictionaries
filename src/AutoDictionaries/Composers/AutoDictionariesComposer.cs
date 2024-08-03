using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Composing;
using System.Collections.Generic;
using AutoDictionaries.Core.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AutoDictionaries.Core.Services.Interfaces;
using AutoDictionaries.Services;
using AutoDictionaries.Models;

namespace AutoDictionaries.Core.Composers
{
    public class AutoDictionariesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<AutoDictionariesManifestFilter>();
            builder.Services.AddScoped<IADTemplateService, ADTemplateService>();
            builder.Services.AddScoped<IADPartialViewService, ADPartialViewService>();
            builder.Services.AddScoped<IAutoDictionariesService, AutoDictionariesService>();
            builder.Services.AddScoped<IADTranslationService, ADTranslationService>();

            builder.Services.AddOptions<AutoDictionariesSettings>()
                .Bind(builder.Config.GetSection(AutoDictionariesSettings.AutoDictionaries));
        }
    }

    internal class AutoDictionariesManifestFilter : IManifestFilter
    {
        public void Filter(List<PackageManifest> manifests)
        {
            manifests.Add(new PackageManifest
            {
                PackageName = "AutoDictionaries",
                Scripts = new[]
                {
                    "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/overview.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/edit.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/apps/overview/overview.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/apps/settings/overview.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/dictionaryItem.edit.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/generateDictionaries.controller.js",
                    "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/matchDictionary.controller.js"
                 },
                Stylesheets = new[]
                {
                    "/App_Plugins/AutoDictionaries/css/autoDictionaries.css"
                }
            });
        }
    }
}