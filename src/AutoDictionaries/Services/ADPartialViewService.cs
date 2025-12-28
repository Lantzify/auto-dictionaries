using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Models;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Services
{
	public class ADPartialViewService : IADPartialViewService
	{
		private readonly FileSystems _fileSystem;
		private readonly IPartialViewService _partialViewService;
		private readonly string _rootPartialViewDirectory;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADPartialViewService(FileSystems fileSystem,
			IPartialViewService partialViewService,
			IAutoDictionariesService autoDictionariesService)
		{
			_fileSystem = fileSystem;
			_partialViewService = partialViewService;
			_rootPartialViewDirectory = "/Views/Partials/";
			_autoDictionariesService = autoDictionariesService;

		}

		public async Task<List<AutoDictionariesModel>> GetAllPartialViews()
		{
			List<AutoDictionariesModel> partialViewList = new ();

			var partialViews = _fileSystem.PartialViewsFileSystem?.GetFiles(_rootPartialViewDirectory);

			if(partialViews == null || !partialViews.Any())
				return partialViewList;

			foreach (var partialViewName in partialViews)
			{
				var partialView = await _partialViewService.GetAsync(partialViewName);
				if(partialView == null)
					continue;

				partialViewList.Add(await MapToAutoDictionariesModel(partialView, partialViewName));
			}

			await GetDirectories(partialViewList, _rootPartialViewDirectory);

			return partialViewList;
		}

		public async Task GetDirectories(List<AutoDictionariesModel> partialViewList, string path)
		{
			var directories = _fileSystem.PartialViewsFileSystem?.GetDirectories(path) ?? Array.Empty<string>();
			foreach (var fileDirectory in directories)
			{
				IEnumerable<string> partialViews = _fileSystem.PartialViewsFileSystem?.GetFiles(_rootPartialViewDirectory + fileDirectory) ?? Array.Empty<string>();

				foreach (var partialViewName in partialViews)
				{
					var partialView = await _partialViewService.GetAsync(partialViewName);
					
					if(partialView == null)
						continue;

					partialViewList.Add(await MapToAutoDictionariesModel(partialView, partialViewName));
				}

				await GetDirectories(partialViewList, _rootPartialViewDirectory + fileDirectory);
			}
		}

		public async Task<AutoDictionariesModel> GetPartialView(int id)
		{
			var allPartialViews = await GetAllPartialViews();

			var partialView = allPartialViews.Where(x => x.Id == id).FirstOrDefault();
			if(partialView == null || string.IsNullOrEmpty(partialView.Path))
				return null;

			return await MapToAutoDictionariesModel(await GetUmbracoPartialView(partialView.Path), partialView.Path, true);
		}

		public async Task<IPartialView> GetUmbracoPartialView(string path)
		{
			return await _partialViewService.GetAsync(path);
		}

		public async Task<AutoDictionariesModel> MapToAutoDictionariesModel(IPartialView partialView, string path, bool getContent = false)
		{
			if (partialView == null) 
				return null;

			var staticContent = await _autoDictionariesService.GetStaticContentFromView(partialView.Content ?? string.Empty);

			return new AutoDictionariesModel()
			{
				Id = partialView.Id,
				Key = partialView.Key,
				Alias = partialView.Alias,
				Name = RemoveFileExtension(partialView?.Name ?? ""),
				Type = "Partial view",
				Path = !string.IsNullOrEmpty(path) ? path : partialView?.VirtualPath,
				Content = getContent ? partialView?.Content : string.Empty,
				StaticContent = staticContent,
				Dictionaries = await _autoDictionariesService.GetDictionariesFromView(partialView?.Content ?? string.Empty),
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