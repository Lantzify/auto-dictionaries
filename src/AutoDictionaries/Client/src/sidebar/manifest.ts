const generateDictionaries: UmbExtensionManifest = {
	type: 'modal',
	alias: 'autoDictionaries.generateDictionaries.modal',
	name: 'Auto Dictionaries Generate Dictionaries Modal',
	js: () => import('./generate-dictionaries.element.js'),
};

const matchDictionaries: UmbExtensionManifest = {
	type: 'modal',
	alias: 'autoDictionaries.matchDictionaries.modal',
	name: 'Auto Dictionaries Match Dictionaries Modal',
	js: () => import('./match-dictionaries.element.js'),
};

export const manifests = [
	generateDictionaries,
	matchDictionaries
];