import type { ManifestStore } from '@umbraco-cms/backoffice/extension-registry';

export const AUTO_DICTIONARIES_REPOSITORY_ALIAS = 'Auto.Dictionaries.Repository';

const repository: ManifestStore = {
    type: 'store',
    alias: AUTO_DICTIONARIES_REPOSITORY_ALIAS,
    name: 'Auto Dictionaries Repository',
    api: () => import('./auto-dictionaries.repository.js'),
};

export const manifests = [repository];