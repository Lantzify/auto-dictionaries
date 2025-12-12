using AutoDictionaries.Models;
using Umbraco.Cms.Core.Manifest;
using AutoDictionaries.Services;
using AutoDictionaries.Composers;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Composers
{
    public class AutoDictionariesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<IADTemplateService, ADTemplateService>();
            builder.Services.AddScoped<IADPartialViewService, ADPartialViewService>();
            builder.Services.AddScoped<IAutoDictionariesService, AutoDictionariesService>();
            builder.Services.AddScoped<IADTranslationService, ADTranslationService>();

            builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

			builder.Services.AddOptions<AutoDictionariesSettings>()
                .Bind(builder.Config.GetSection(AutoDictionariesSettings.AutoDictionaries));
        }
    }
}