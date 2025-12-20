import { UmbModalToken } from "@umbraco-cms/backoffice/modal";
import { AddExistingDictionaryItemToViewDto } from "../api";

export const MATCH_DICTIONARY_MODAL_TOKEN = new UmbModalToken<AddExistingDictionaryItemToViewDto, string[]>('autoDictionaries.matchDictionaries.modal', {
    modal: {
        type: 'sidebar',
        size: 'medium'
    }
});