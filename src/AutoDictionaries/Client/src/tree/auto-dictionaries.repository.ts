import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import type { UmbApi } from "@umbraco-cms/backoffice/extension-api";
import { UmbTreeRepositoryBase, type UmbTreeItemModel, type UmbTreeRootModel } from "@umbraco-cms/backoffice/tree";
import { autoDictionariesTreeDataSource } from "./auto-dictionaries.data-source";

export class autoDictionariesRepository extends UmbTreeRepositoryBase<UmbTreeItemModel, UmbTreeRootModel>
    implements UmbApi {
    constructor(host: UmbControllerHost) {
        super(host, autoDictionariesTreeDataSource);
    }

    async requestTreeRoot() {
       
        const data: UmbTreeRootModel = {
            unique: null,
            entityType: "auto-dictionaries-root",
            name: "Auto Dictionaries",
            icon: "icon-book",
            hasChildren: false,
            isFolder: true,
        };

        return { data };
    }
}

export { autoDictionariesRepository as api };