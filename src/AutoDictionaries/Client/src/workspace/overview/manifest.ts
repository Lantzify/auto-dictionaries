const workspace: UmbExtensionManifest = {
	type: "workspace",
	alias: "autoDictionaries.workspace",
	name: "Auto dictionaries workspace",
	js: () => import('./workspace.element.js'),
	meta: {
		entityType: "auto-dictionaries-root",
	},
};

const context: UmbExtensionManifest = {
	type: "workspaceContext",
	alias: "autoDictionaries.workspace.context",
	name: "Auto dictionaries workspace context",
	js: () => import("./workspace.context.js"),
	conditions: [
		{
			alias: "Umb.Condition.WorkspaceAlias",
			match: "autoDictionaries.workspace",
		},
	],
};

const workspaceViews: Array<UmbExtensionManifest> = [
	{
		type: "workspaceView",
		alias: "autoDictionaries.workspaceView.overview",
		name: "Auto dictionaries workspace overview view",
		js: () => import("./views/overview/overview.element.js"),
		weight: 20,
		meta: {
			label: "#autoDictionaries_overview",
			pathname: "overview",
			icon:"icon-book"
		},
		conditions: [
			{
				alias: "Umb.Condition.WorkspaceAlias",
				match: "autoDictionaries.workspace"
			}
		]
	},
	{
		type: "workspaceView",
		alias: "autoDictionaries.workspaceView.settings",
		name: "Auto dictionaries workspace settings view",
		js: () => import("./views/settings/settings.element.js"),
		weight: 10,
		meta: {
			label: "#sections_settings",
			pathname: "settings",
            icon: "icon-settings"
		},
		conditions: [
			{
				alias: "Umb.Condition.WorkspaceAlias",
				match: "autoDictionaries.workspace"
			}
		]
	}
];

export const manifests = [
	workspace,
	context,
	...workspaceViews
];