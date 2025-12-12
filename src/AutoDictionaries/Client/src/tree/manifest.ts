const menuSidebarApp: UmbExtensionManifest = {
	type: "sectionSidebarApp",
	kind: "menu",
	alias: "autoDictionaries.sidebarapp",
	name: "Auto Dictionarie sidebar menu",
	weight: 10,
	meta: {
		label: "Auto Dictionary",
		menu: "autoDictionaries.menu",
	},
	conditions: [
		{
			alias: "Umb.Condition.SectionAlias",
			match: "Umb.Section.Translation",
		},
	],
};


const menu: UmbExtensionManifest = {
	type: "menu",
	alias: "autoDictionaries.menu",
	name: "Auto dictionaries",
	meta: {
		label: "Auto dictionaries",
		icon: "icon-book",
		//entityType: "auto-dictionaries-menu",
	},
};

const menuItem: UmbExtensionManifest = {
	type: 'menuItem',
	alias: 'autoDictionaries.menu.item',
	name: 'Auto Dictionarie item',
	//element: "auto-dictionaries-menu",
	meta: {
		label: 'Auto dictionaries',
		icon: 'icon-book',
		entityType: "auto-dictionaries",
		menus: [
			"autoDictionaries.menu"
		],
	},
};

export const manifests = [
	menu,
	menuItem,
	menuSidebarApp
];