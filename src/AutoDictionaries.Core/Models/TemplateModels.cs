﻿using System.Collections.Generic;

namespace AutoDictionaries.Core.Models
{
	public class TemplateModel
	{
		public int Id { get; set; }
		public string Alias { get; set; }
		public string Name { get; set; }
		public List<DictionaryModel> Dictionaries { get; set; }
		public List<StaticContentModel> StaticContent { get; set; }
	}
}