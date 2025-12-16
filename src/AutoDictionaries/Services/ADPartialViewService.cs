using System.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using Microsoft.AspNetCore.Hosting;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class ADPartialViewService : IADPartialViewService
	{
		private readonly FileSystems _fileSystem;
		private readonly IPartialViewService _partialViewService;
		private readonly string _rootPartialViewDirectory;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADPartialViewService(FileSystems fileSystem, IPartialViewService partialViewService, IAutoDictionariesService autoDictionariesService)
		{
			_fileSystem = fileSystem;
			_partialViewService = partialViewService;
			_rootPartialViewDirectory = "/Views/Partials/";
			_autoDictionariesService = autoDictionariesService;

		}

		public async Task<List<AutoDictionariesModel>> GetAllPartialViews()
		{
			List<AutoDictionariesModel> partialViewList = new ();

			var partialViews = _fileSystem.PartialViewsFileSystem.GetFiles(_rootPartialViewDirectory);

			foreach (var partialViewName in partialViews)
			{
				var partialView = await _partialViewService.GetAsync(partialViewName);
				
				partialViewList.Add(await MapToAutoDictionariesModel(partialView, partialViewName));
			}

			await GetDirectories(partialViewList, _rootPartialViewDirectory);

			return partialViewList;
		}

		public async Task GetDirectories(List<AutoDictionariesModel> partialViewList, string path)
		{
			foreach (var fileDirectory in _fileSystem.PartialViewsFileSystem.GetDirectories(path))
			{
				IEnumerable<string> partialViews = _fileSystem.PartialViewsFileSystem.GetFiles(_rootPartialViewDirectory + fileDirectory);

				foreach (var partialViewName in partialViews)
				{
					var partialView = await _partialViewService.GetAsync(partialViewName);
					
					partialViewList.Add(await MapToAutoDictionariesModel(partialView, partialViewName));
				}

				GetDirectories(partialViewList, _rootPartialViewDirectory + fileDirectory);
			}
		}

		//Can't use Guid here since it's not reliabvle should use path insetad
		public async Task<AutoDictionariesModel> GetPartialView(int id)
		{
			var allPartialViews = await GetAllPartialViews();

			var partialView = allPartialViews.Where(x => x.Id == id).FirstOrDefault();

			return await MapToAutoDictionariesModel(await GetUmbracoPartialView(partialView.Path), partialView.Path, true);
		}

		public async Task<IPartialView> GetUmbracoPartialView(string path)
		{
			return await _partialViewService.GetAsync(path);
		}

		public async Task<AutoDictionariesModel> MapToAutoDictionariesModel(IPartialView partialView, string path, bool getContent = false)
		{
			if (partialView == null) return null;

			var staticContent = await _autoDictionariesService.GetStaticContentFromView(partialView.Content);

			return new AutoDictionariesModel()
			{
				Id = partialView.Id,
				Key = partialView.Key,
				Alias = partialView.Alias,
				Name = RemoveFileExtension(partialView.Name),
				Type = "Partial view",
				Path = !string.IsNullOrEmpty(path) ? path : partialView.VirtualPath,
				Content = getContent ? partialView.Content : string.Empty,
				StaticContent = staticContent,
				Dictionaries = await _autoDictionariesService.GetDictionariesFromView(partialView.Content),
				MatchDictionaries = staticContent.Where(x => x.Dictionary != null).Count()
			};
		}

		private static string RemoveFileExtension(string name)
		{
			if (!name.Contains(".cshtml")) return name;

			int index = name.LastIndexOf(".cshtml");
			return index == -1 ? name : name.Substring(0, index);
		}
	}
}