import { manifests as trees } from './tree/manifest.js';
import { manifests as overviewWorkspace } from './workspace/overview/manifest.js';
import { manifests as itemWorkspace } from './workspace/item/manifest.js';
import { manifests as localization } from './localization/manifest.js';
import { manifests as repository } from './repository/manifest.js';

export const manifests = [
    ...trees,
    ...overviewWorkspace,
    ...itemWorkspace,
    ...localization,
    ...repository
];