const generateDictionaries: UmbExtensionManifest = {
	type: 'modal',
	alias: 'autoDictionaries.generateDictionaries.modal',
	name: 'Auto Dictionaries Generate Dictionaries Modal',
	js: () => import('./generate-dictionaries.element.js'),
};

export const manifests = [
	generateDictionaries
];