using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using AutoDictionaries.Core.Services;

namespace AutoDictionaries.Core.Compositions
{
	[RuntimeLevel(MinLevel = RuntimeLevel.Run)]
	public class AutoDictionariesComposer : IUserComposer
	{
		public void Compose(Composition composition)
		{
			composition.Register<IAutoDictionariesService, AutoDictionariesService>(Lifetime.Request);
		}
	}
}
