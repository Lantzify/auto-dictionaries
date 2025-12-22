import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UmbTreeServerDataSourceBase, type UmbTreeItemModel } from "@umbraco-cms/backoffice/tree";
import type { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";

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

const getAncestorsOf = async (): Promise<UmbDataSourceResponse<any>> => {
    return { data: { items: [] } };
};

const getRootItems = async (): Promise<UmbDataSourceResponse<any>> => {
    try {
        const { data, error } = await AutoDictionariesService.getChildren();
        
        if (error || !data) {
            return { error: error ? new Error(JSON.stringify(error)) : new Error('Unknown error') };
        }
        
        return {
            data: {
                items: data.items ?? [],
                total: data.total ?? 0
            }
        };
    } catch (err) {
        return { error: err instanceof Error ? err : new Error('Failed to fetch root items') };
    }
};
  
const getChildrenOf = async (): Promise<UmbDataSourceResponse<any>> => {
    return { data: { items: [] } };
};

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