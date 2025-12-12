import { manifests as trees } from './tree/manifest.js';
import { manifests as workspace } from './workspace/manifest.js';
import { manifests as localization } from './localization/manifest.js';

export const manifests = [
    ...trees,
    ...workspace,
    ...localization
];