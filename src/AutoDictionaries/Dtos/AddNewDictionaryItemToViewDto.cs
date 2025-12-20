using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Dtos
{
	public class AddNewDictionaryItemToViewDto
	{
		public bool Translate { get; set; }
		public AutoDictionariesModel? AutoDictionariesModel { get; set; }
		public StaticContentDto? StaticContent { get; set; }
	}
}