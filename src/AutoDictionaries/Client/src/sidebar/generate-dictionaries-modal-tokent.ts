import { UmbModalToken } from "@umbraco-cms/backoffice/modal";
import { PreviewAddNewDictionaryItemToViewDto } from "../api";

export const GENERATE_DICTIONARY_MODAL_TOKEN = new UmbModalToken<PreviewAddNewDictionaryItemToViewDto, string[]>('autoDictionaries.generateDictionaries.modal', {
    modal: {
        type: 'sidebar',
        size: 'medium'
    }
});