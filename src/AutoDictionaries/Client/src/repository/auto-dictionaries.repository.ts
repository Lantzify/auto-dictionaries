import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbRepositoryBase } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import {
    AddExistingDictionaryItemToViewDto,
    AddNewDictionaryItemToViewDto,
    AutoDictionariesService,
    PreviewAddNewDictionaryItemToViewDto
} from "../api";
import { UmbApi } from "@umbraco-cms/backoffice/extension-api";

export class AutoDictionariesRepository extends UmbRepositoryBase implements UmbApi {
    constructor(host: UmbControllerHost) {
        super(host, "AutoDictionariesRepository");
    }

    async getTranslateSetting() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetTranslateSetting()
        );

        return data ?? false;
    }

    async getTranslatorSetting() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetTranslatorSetting()
        );

        return data !== "" ? data : "DeepL";
    }

    async getApiEndpoint() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetApiEndpoint()
        );

        return data !== "" ? data : "null";
    }

    async getApiKeySetting() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetApiKey()
        );

        return data !== "" ? data : "null";
    }

    async getApiRegionSetting() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetApiRegion()
        );

        return data !== "" ? data : "null";
    }

    async getAllDictionaryItems() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetAllDictionaryItems()
        );

        return data ?? [];
    }

    async getAllViews() {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetAllViews()
        );

        return data;
    }

    async getViewById(id: string) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetViewById({ path: { id } })
        );

        return data;
    }


    async postPreviewAddNewDictionaryItem(dto: PreviewAddNewDictionaryItemToViewDto) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.postPreviewAddNewDictionaryItem({
                body: {
                    autoDictionariesModel: dto.autoDictionariesModel,
                    staticContent: dto.staticContent,
                    canTranslate: dto.canTranslate
                }
            })
        );

        return data;
    }

    async postAddNewDictionaryItem(dto: AddNewDictionaryItemToViewDto) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.postAddNewDictionaryItem({
                body: {
                    autoDictionariesModel: dto.autoDictionariesModel,
                    staticContent: dto.staticContent,
                    translate: dto.translate
                }
            })
        );

        return data;
    }


    async postPreviewAddExistingDictionaryItem(dto: AddExistingDictionaryItemToViewDto) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.postPreviewAddExistingDictionaryItem({
                body: {
                    autoDictionariesModel: dto.autoDictionariesModel,
                    dictionaryKey: dto.dictionaryKey,
                    staticContent: dto.staticContent
                }
            })
        );

        return data;
    }

    async postAddExistingDictionaryItem(dto: AddExistingDictionaryItemToViewDto) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.postAddExistingDictionaryItem({
                body: {
                    autoDictionariesModel: dto.autoDictionariesModel,
                    dictionaryKey: dto.dictionaryKey,
                    staticContent: dto.staticContent
                }
            })
        );

        return data;
    }

    async getTranslateDictionaryItem(id: string) {
        const { data } = await tryExecute(
            this._host,
            AutoDictionariesService.getTranslateDictionaryItemById({ path: { id } })
        );

        return data ?? false;
    }
}

export { AutoDictionariesRepository as api };
export default AutoDictionariesRepository;