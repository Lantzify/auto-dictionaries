using System;
using System.Collections.Generic;

namespace AutoDictionaries.Core.Models
{
	public class DictionaryModel
	{
		public string Key { get; set; }
		public int Id { get; set; }
		public Guid Guid { get; set; }
		public int Used { get; set; }
		public List<string> Translations { get; set; }
		public bool Translated { get; set; }
	}
}