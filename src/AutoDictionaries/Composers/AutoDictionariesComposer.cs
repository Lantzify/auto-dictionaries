using Umbraco.Cms.Core.Composing;
using AutoDictionaries.Core.Services;
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
		}
	}
}