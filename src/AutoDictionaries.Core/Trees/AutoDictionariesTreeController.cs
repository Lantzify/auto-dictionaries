using AutoDictionaries.Core.Services;
using System;
using System.Net.Http.Formatting;
using Umbraco.Core;
using Umbraco.Web.Actions;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;

namespace AutoDictionaries.Core.Trees
{
	[PluginController("AutoDictionaries")]
	[UmbracoTreeAuthorize(Constants.Trees.DocumentTypes)]
	[Tree("translation", "autoDictionaries", SortOrder = 12, TreeTitle = "Auto dictionaries", TreeGroup = "autoDictionariesGroup")]
	public class AutoDictionariesTreeController : TreeController
	{
		private readonly IAutoDictionariesService _autoDictionariesService;

		public AutoDictionariesTreeController(IAutoDictionariesService autoDictionariesService)
		{
			_autoDictionariesService = autoDictionariesService;
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

				foreach (var template in _autoDictionariesService.GetAllTemplates())
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
			// create a Menu Item Collection to return so people can interact with the nodes in your tree
			var menu = new MenuItemCollection();

			if (id == Constants.System.Root.ToInvariantString())
			{
				// root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
				// add your menu item actions or custom ActionMenuItems
				menu.Items.Add(new CreateChildEntity(Services.TextService));
				// add refresh menu item (note no dialog)
				menu.Items.Add(new RefreshNode(Services.TextService, true));
				return menu;
			}
			// add a delete action to each individual item
			menu.Items.Add<ActionDelete>(Services.TextService, true, opensDialog: true);

			return menu;
		}
	}
}