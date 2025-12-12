const localizations: Array<UmbExtensionManifest> = [
	{
		type: 'localization',
		alias: 'autoDictionarie.lang.ja',
		name: 'Japanese',
		weight: 0,
		meta: {
			culture: 'ja',
		},
		js: () => import('./languages/ja'),
	},
	{
		type: 'localization',
		alias: 'autoDictionarie.lang.en',
		name: 'English',
		weight: 1,
		meta: {
			culture: 'en',
		},
		js: () => import('./languages/en'),
	}
];

export const manifests: UmbExtensionManifest[] = [...localizations];