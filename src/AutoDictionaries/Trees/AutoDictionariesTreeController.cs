using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;
using AutoDictionaries.Core.Services.Interfaces;
using Umbraco.Cms.Web.Common.Attributes;

namespace AutoDictionaries.Core.Trees
{
	[PluginController("AutoDictionaries")]
	//[UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
	[Tree("translation", "autoDictionaries", SortOrder = 12, TreeTitle = "Auto dictionaries", TreeGroup = "autoDictionariesGroup")]
	public class AutoDictionariesTreeController : TreeController
	{
		private readonly IADTemplateService _adTemplateService;
		private readonly IADPartialViewService _adPartialViewService;
		private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

		public AutoDictionariesTreeController(
			ILocalizedTextService localizedTextService,
			UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
			IMenuItemCollectionFactory menuItemCollectionFactory,
			IEventAggregator eventAggregator,
			IADPartialViewService adPartialViewService, IADTemplateService adTemplateService) 
			: base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
		{
			_adTemplateService = adTemplateService;
			_adPartialViewService = adPartialViewService;
			_menuItemCollectionFactory = menuItemCollectionFactory ?? throw new ArgumentNullException(nameof(menuItemCollectionFactory));
		}

		protected override ActionResult<TreeNode> CreateRootNode(FormCollection queryStrings)
		{
			var rootResult = base.CreateRootNode(queryStrings);
			if (!(rootResult.Result is null))
				return rootResult;

			var root = rootResult.Value;

			root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Translation, "autoDictionaries", "overview");
			root.Icon = "icon-book";
			root.HasChildren = false;
			root.MenuUrl = null;

			return root;
		}


		protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
		{
			if (id == Constants.System.Root.ToInvariantString())
			{
				var nodes = new TreeNodeCollection();

				foreach (var template in _adTemplateService.GetAllTemplates().Concat(_adPartialViewService.GetAllPartialViews()))
				{
					var node = CreateTreeNode($"{template.Id}", "-1", queryStrings, template.Alias, "icon-book", false);
					nodes.Add(node);
				}

				return nodes;
			}

			throw new NotSupportedException();
		}

		protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
		{
			var menu = _menuItemCollectionFactory.Create();

			if (id == Constants.System.Root.ToInvariantString())
			{
				menu.Items.Add(new CreateChildEntity(LocalizedTextService));
				menu.Items.Add(new RefreshNode(LocalizedTextService, true));

				return menu;
			}

			menu.Items.Add<ActionDelete>(LocalizedTextService, true, opensDialog: true);

			return menu;
		}
	}
}