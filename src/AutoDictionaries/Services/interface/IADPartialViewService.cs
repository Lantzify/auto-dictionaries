using Umbraco.Cms.Core.Models;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IADPartialViewService
	{
		List<AutoDictionariesModel> GetAllPartialViews();
		void GetDirectories(List<AutoDictionariesModel> partialViewList, string path);
		AutoDictionariesModel GetPartialView(int id);
		IPartialView GetUmbracoPartialView(string path);
		AutoDictionariesModel MapToAutoDictionariesModel(IPartialView partialView, string path, bool getContent = false);
	}
}