using AutoDictionaries.Core.Models;
using System.Collections.Generic;

namespace AutoDictionaries.Core.Dtos
{
	public class AddNewDictionaryItemToTemplateDto
	{
		public TemplateModel Template { get; set; }
		public StaticContentDto StaticContent { get; set; }
	}
}