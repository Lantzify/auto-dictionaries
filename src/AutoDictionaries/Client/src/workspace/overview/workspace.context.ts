import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_WORKSPACE_CONTEXT, type UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import AutoDictionariesRepository from '../../repository/auto-dictionaries.repository';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { AutoDictionariesModel } from '../../api';

export class autoDictionariesWorkspaceContext extends UmbControllerBase implements UmbWorkspaceContext {
    public readonly workspaceAlias: string = "autoDictionaries.workspace";

    #repository: AutoDictionariesRepository;

    #views = new UmbObjectState<AutoDictionariesModel[] | undefined>(undefined);
    public readonly views = this.#views.asObservable();

    #isLoading = new UmbObjectState<boolean>(false);
    public readonly isLoading = this.#isLoading.asObservable();

    constructor(host: UmbControllerHostElement) {
        super(host);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this); 
        this.#repository = new AutoDictionariesRepository(this);
    }

    async load() {
        this.#isLoading.setValue(true);

        const data = await this.#repository.getAllViews();

        if (data) {
            this.#views.setValue(data);
        }

        this.#isLoading.setValue(false);
    }

    getRepository(): AutoDictionariesRepository {
        return this.#repository;
    }

	getEntityType(): string {
       return "auto-dictionaries-root";
    }
}

export default autoDictionariesWorkspaceContext;