using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using System.Net.Http.Formatting;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Core.Trees
{
	[PluginController("AutoDictionaries")]
	[UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
	[Tree("translation", "autoDictionaries", SortOrder = 12, TreeTitle = "Auto dictionaries", TreeGroup = "autoDictionariesGroup")]
	public class AutoDictionariesTreeController : TreeController
	{
		private readonly IADPartialViewService _adPartialViewService;
		private readonly IADTemplateService _adTemplateService;

		public AutoDictionariesTreeController(IADPartialViewService adPartialViewService, IADTemplateService adTemplateService)
		{
			_adPartialViewService = adPartialViewService;
			_adTemplateService = adTemplateService;
		}

		protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
		{
			var root = base.CreateRootNode(queryStrings);

			root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Translation, "autoDictionaries", "overview");
			root.Icon = "icon-book";
			root.HasChildren = false;
			root.MenuUrl = null;

			return root;
		}

		protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
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

		protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
		{
			var menu = new MenuItemCollection();

			if (id == Constants.System.Root.ToInvariantString())
			{
				menu.Items.Add(new CreateChildEntity(Services.TextService));
				menu.Items.Add(new RefreshNode(Services.TextService, true));

				return menu;
			}
			menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);

			return menu;
		}
	}
}