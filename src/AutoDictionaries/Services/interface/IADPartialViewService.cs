using Umbraco.Cms.Core.Models;
using AutoDictionaries.Core.Models;

namespace AutoDictionaries.Core.Services.Interfaces
{
	public interface IADPartialViewService
	{
		Task<List<AutoDictionariesModel>> GetAllPartialViews();
		Task GetDirectories(List<AutoDictionariesModel> partialViewList, string path);
		Task<AutoDictionariesModel> GetPartialView(int id);
		Task<IPartialView> GetUmbracoPartialView(string path);
		Task<AutoDictionariesModel> MapToAutoDictionariesModel(IPartialView partialView, string path, bool getContent = false);
	}
}