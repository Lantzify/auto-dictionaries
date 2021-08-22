using Umbraco.Core;
using Umbraco.Core.Composing;
using AutoDictionaries.Core.Services;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Compositions
{
	public class AutoDictionariesComposer : IUserComposer
	{
		public void Compose(Composition composition)
		{
			composition.Register<IADTemplateService, ADTemplateService>(Lifetime.Request);
			composition.Register<IADPartialViewService, ADPartialViewService>(Lifetime.Request);
			composition.Register<IAutoDictionariesService, AutoDictionariesService>(Lifetime.Request);
		}
	}
}