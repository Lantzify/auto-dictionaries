using System.Collections.Generic;

namespace AutoDictionaries.Core.Models
{
	public class AutoDictionariesModel
	{
		public int Id { get; set; }
		public string Alias { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string Path { get; set; }
		public List<DictionaryModel> Dictionaries { get; set; }
		public List<StaticContentModel> StaticContent { get; set; }
	}
}