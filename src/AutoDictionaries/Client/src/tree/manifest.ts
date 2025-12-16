const repository: UmbExtensionManifest = {
    type: "repository",
    alias: "autoDictionaries.tree.repository",
    name: "AutoDictionaries Repository Settings",
    api: () => import("./auto-dictionaries.repository.js"),
};

const menu: UmbExtensionManifest = {
    type: "menu",
    alias: "autoDictionaries.menu",
    name: "Auto dictionaries",
    meta: {
        label: "Auto dictionaries",
        icon: "icon-book",
        entityType: "auto-dictionaries-root",
    },
};

const tree: UmbExtensionManifest = {
    type: "tree",
    kind: "default",
    alias: "autoDictionaries.tree",
    name: "Auto Dictionaries Tree Settings",
    meta: {
        repositoryAlias: repository.alias,
    },
};

const menuItem: UmbExtensionManifest = {
    type: 'menuItem',
    //kind: "tree",
    alias: 'autoDictionaries.tree.menu.item',
    name: 'Auto Dictionarie Tree Item',
    meta: {
        label: 'Auto dictionaries',
        icon: 'icon-book',
        entityType: "auto-dictionaries-root",
        menus: [
            menu.alias
        ],
        treeAlias: tree.alias,
        
    },
};

const menuSidebarApp: UmbExtensionManifest = {
    type: "sectionSidebarApp",
    kind: "menu",
    alias: "autoDictionaries.sidebarapp",
    name: "Auto Dictionarie sidebar menu",
    weight: 100,
    meta: {
        label: "Auto Dictionary",
        menu: menu.alias,
    },
    conditions: [
        {
            alias: "Umb.Condition.SectionAlias",
            match: "Umb.Section.Translation",
        },
    ],
};

export const manifests = [
    menuSidebarApp,
    menu,
    repository,
    tree,
    menuItem,
];