using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Dtos
{
	public class PreviewAddNewDictionaryItemToViewDto
	{
		public AutoDictionariesModel? AutoDictionariesModel { get; set; }
		public List<StaticContentDto>? StaticContent { get; set; }
		public bool CanTranslate { get; set; }
	}
}