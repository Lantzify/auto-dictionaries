using System.Collections.Generic;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Dtos
{
	public class PreviewAddNewDictionaryItemToViewDto
	{
		public AutoDictionariesModel AutoDictionariesModel { get; set; }
		public List<StaticContentDto> StaticContent { get; set; }
	}
}