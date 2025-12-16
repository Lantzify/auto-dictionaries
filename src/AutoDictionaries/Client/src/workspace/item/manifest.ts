const itemWorkspace: UmbExtensionManifest = {
	type: "workspace",
	alias: "autoDictionaries.item.workspace",
	name: "Auto dictionaries item workspace",
	js: () => import('./workspace.element.js'),
	meta: {
		entityType: "auto-dictionaries-item",
	},
};

const itemContext: UmbExtensionManifest = {
	type: "workspaceContext",
	alias: "autoDictionaries.item.workspace.context",
	name: "Auto dictionaries item workspace context",
	js: () => import("./workspace.context.js"),
	conditions: [
		{
			alias: "Umb.Condition.WorkspaceAlias",
			match: "autoDictionaries.item.workspace",
		},
	],
};

export const manifests = [
	itemWorkspace,
	itemContext,
];