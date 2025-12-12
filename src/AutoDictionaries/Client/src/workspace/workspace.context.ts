import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
	LitElement,
	customElement,
	html,
} from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_CONTEXT, type UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';





export class autoDictionariesWorkspaceContext extends UmbControllerBase implements UmbWorkspaceContext {
    public readonly workspaceAlias: string = "autoDictionaries.workspace";

    constructor(host: UmbControllerHostElement) {
        super(host);
        this.provideContext(UMB_WORKSPACE_CONTEXT, this); 
     //   this.provideContext(AUTO_DICTIONARIES_WORKSPACE_CONTEXT, this); 
    }

	getEntityType(): string {
       return "auto-dictionaries-root";
    }


	






}

export default autoDictionariesWorkspaceContext;