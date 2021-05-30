using System.Collections.Generic;
using Umbraco.Core.Models;

namespace AutoDictionaries.Core.Models
{
	public class StaticContentModel
	{
		public int Used { get; set; }
		public string StaticContent { get; set; }
		public DictionaryModel Dictionary { get; set; }
	}
}