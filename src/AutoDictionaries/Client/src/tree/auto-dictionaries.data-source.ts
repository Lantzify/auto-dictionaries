import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import {
    type UmbTreeAncestorsOfRequestArgs,
    type UmbTreeChildrenOfRequestArgs,
    type UmbTreeRootItemsRequestArgs,
    UmbTreeServerDataSourceBase,
    type UmbTreeItemModel

} from "@umbraco-cms/backoffice/tree";

import { AutoDictionariesService, type AutoDictionariesModel } from '../api';


export class autoDictionariesTreeDataSource extends UmbTreeServerDataSourceBase<any, any> {
    constructor(host: UmbControllerHost) {
        super(host, {
            getRootItems,
            getChildrenOf,
            getAncestorsOf,
            mapper,
        });
    }
}

const getAncestorsOf = async (args: UmbTreeAncestorsOfRequestArgs) =>  [];


const getRootItems = async (args: UmbTreeRootItemsRequestArgs) =>
    await AutoDictionariesService.getChildren();
  
const getChildrenOf = async (args: UmbTreeChildrenOfRequestArgs) => [];

const mapper = (item: AutoDictionariesModel): UmbTreeItemModel => {
    return {
        unique: item?.key ?? null,
        parent: { unique: null, entityType: "auto-dictionaries-root" },
        name: item.name ?? "unknown",
        entityType: "auto-dictionaries-item",
        hasChildren: false,
        isFolder: false,
        icon: "icon-book"
    };
};