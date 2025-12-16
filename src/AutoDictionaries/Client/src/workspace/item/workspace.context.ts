import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WORKSPACE_CONTEXT, type UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { AutoDictionariesService, type AutoDictionariesModel } from '../../api';

export class AutoDictionariesItemWorkspaceContext extends UmbControllerBase implements UmbWorkspaceContext {
    public readonly workspaceAlias: string = "autoDictionaries.item.workspace";

    #unique = new UmbObjectState<string | undefined>(undefined);
    public readonly unique = this.#unique.asObservable();

    #currentItem = new UmbObjectState<AutoDictionariesModel | undefined>(undefined);
    public readonly currentItem = this.#currentItem.asObservable();

    #isLoading = new UmbObjectState<boolean>(false);
    public readonly isLoading = this.#isLoading.asObservable();

    constructor(host: UmbControllerHostElement) {
        super(host);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this);
    }

    setUnique(unique: string | undefined) {
        this.#unique.setValue(unique);
        if (unique) {
            this.load(unique);
        }
    }

    getUnique(): string | undefined {
        return this.#unique.getValue();
    }

    async load(id: string) {
        this.#isLoading.setValue(true);

        const { data, error } = await tryExecute(
            this._host,
            AutoDictionariesService.getGetViewById({ path: { id } })
        );

        if (data) {
            this.#currentItem.setValue(data);
        }

        this.#isLoading.setValue(false);
    }

    getCurrentItem(): AutoDictionariesModel | undefined {
        return this.#currentItem.getValue();
    }

    getEntityType(): string {
        return "auto-dictionaries-item";
    }
}

export default AutoDictionariesItemWorkspaceContext;