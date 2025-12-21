using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Dtos
{
	public class AddExistingDictionaryItemToViewDto
	{
		public AutoDictionariesModel? AutoDictionariesModel { get; set; }
		public Guid DictionaryKey { get; set; }
		public string? StaticContent { get; set; }
	}
}