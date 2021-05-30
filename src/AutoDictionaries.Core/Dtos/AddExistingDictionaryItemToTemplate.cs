using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Dtos
{
	public class AddExistingDictionaryItemToTemplate
	{
		public TemplateModel Template { get; set; }
		public int DictionaryId { get; set; }
		public string StaticContent { get; set; }
	}
}