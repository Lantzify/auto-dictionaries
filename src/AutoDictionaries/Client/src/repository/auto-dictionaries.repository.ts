import { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbRepositoryBase } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from "@umbraco-cms/backoffice/resources";
import { AutoDictionariesService } from "../api";
import { UmbApi } from "@umbraco-cms/backoffice/extension-api";


export class AutoDictionariesRepository extends UmbRepositoryBase implements UmbApi {
    constructor(host: UmbControllerHost) {
        super(host, "AutoDictionariesRepository");
    }


    async getTranslateSetting() {
        const { data, error } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetTranslateSetting()
        );

        return data ?? false;
    }

    async getAllDictionaryItems() {
        const { data, error } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetAllDictionaryItems()
        );

        return data ?? [];
    }

    async getAllViews() {
        const { data, error } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetAllViews()
        );

        return data;
    }

    async getViewById(id: string) {
        const { data, error } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetViewById({ path: { id } })
        );

        return data;
    }

}

export { AutoDictionariesRepository as api };
export default AutoDictionariesRepository;