using AutoDictionaries.Core.Models;
using System.Collections.Generic;

namespace AutoDictionaries.Core.Dtos
{
	public class AddNewDictionaryItemToViewDto
	{
		public AutoDictionariesModel AutoDictionariesModel { get; set; }
		public StaticContentDto StaticContent { get; set; }
	}
}