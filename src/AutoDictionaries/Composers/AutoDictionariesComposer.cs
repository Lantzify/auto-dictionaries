using AutoDictionaries.Models;
using AutoDictionaries.Services;
using AutoDictionaries.Composers;
using Umbraco.Cms.Core.Composing;
using AutoDictionaries.Notifications;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Composers
{
    public class AutoDictionariesComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {

            builder.Services.AddSingleton<IADTemplateService, ADTemplateService>()
                            .AddSingleton<IADPartialViewService, ADPartialViewService>()
							.AddSingleton<IADTranslationService, ADTranslationService>()
							.AddSingleton<IAutoDictionariesService, AutoDictionariesService>();

            builder.AddNotificationHandler<DictionaryItemSavedNotification, DictionaryItemLazyNotification>()
                   .AddNotificationHandler<DictionaryItemDeletedNotification, DictionaryItemLazyNotification>();

			builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

			builder.Services.AddOptions<AutoDictionariesSettings>()
                .Bind(builder.Config.GetSection(AutoDictionariesSettings.AutoDictionaries));
        }
    }
}