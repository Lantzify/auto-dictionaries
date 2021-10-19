using System.Linq;
using Umbraco.Core.Services;
using System.Collections.Generic;
using AutoDictionaries.Core.Models;
using Umbraco.Core.Models;
using Umbraco.Core.IO;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Services
{
	public class ADPartialViewService : IADPartialViewService
	{
		private readonly IFileSystems _fileSystem;
		private readonly IFileService _fileService;
		private readonly string _rootPartialViewDirectory;
		private readonly IAutoDictionariesService _autoDictionariesService;

		public ADPartialViewService(IFileSystems fileSystem, IFileService fileService, IAutoDictionariesService autoDictionariesService)
		{
			_fileSystem = fileSystem;
			_fileService = fileService;
			_rootPartialViewDirectory = "/Views/Partials/";
			_autoDictionariesService = autoDictionariesService;
		}

		public List<AutoDictionariesModel> GetAllPartialViews()
		{
			List<AutoDictionariesModel> partialViewList = new List<AutoDictionariesModel>();

			var partialViews = _fileSystem.PartialViewsFileSystem.GetFiles(_rootPartialViewDirectory);

			foreach (var partialViewName in partialViews)
			{
				var partialView = _fileService.GetPartialView(partialViewName);
				
				partialViewList.Add(MapToAutoDictionariesModel(partialView, partialViewName));
			}

			GetDirectories(partialViewList, _rootPartialViewDirectory);

			return partialViewList;
		}

		public void GetDirectories(List<AutoDictionariesModel> partialViewList, string path)
		{
			foreach (var fileDirectory in _fileSystem.PartialViewsFileSystem.GetDirectories(path))
			{
				IEnumerable<string> partialViews = _fileSystem.PartialViewsFileSystem.GetFiles(_rootPartialViewDirectory + fileDirectory);

				foreach (var partialViewName in partialViews)
				{
					var partialView = _fileService.GetPartialView(partialViewName);
					
					partialViewList.Add(MapToAutoDictionariesModel(partialView, partialViewName));
				}

				GetDirectories(partialViewList, _rootPartialViewDirectory + fileDirectory);
			}
		}

		public AutoDictionariesModel GetPartialView(int id)
		{
			var allPartialViews = GetAllPartialViews();

			var partialView = allPartialViews.Where(x => x.Id == id).FirstOrDefault();

			return MapToAutoDictionariesModel(GetUmbracoPartialView(partialView?.Path), partialView?.Path, true);
		}

		public IPartialView GetUmbracoPartialView(string path)
		{
			return _fileService.GetPartialView(path);
		}

		public AutoDictionariesModel MapToAutoDictionariesModel(IPartialView partialView, string path, bool getContent = false)
		{
			return partialView == null ? null : new AutoDictionariesModel()
			{
				Id = partialView.Id,
				Alias = partialView.Alias,
				Name = RemoveFileExtension(partialView.Name),
				Type = "Partial view",
				Path = !string.IsNullOrEmpty(path) ? path : partialView.VirtualPath,
				Content = getContent ? partialView.Content : string.Empty,
				StaticContent = _autoDictionariesService.GetStaticContentFromView(partialView.Content),
				Dictionaries = _autoDictionariesService.GetDictionariesFromView(partialView.Content)
			};
		}

		private string RemoveFileExtension(string name)
		{
			if (!name.Contains(".cshtml")) return name;

			int index = name.LastIndexOf(".cshtml");
			return index == -1 ? name : name.Substring(0, index);
		}
	}
}