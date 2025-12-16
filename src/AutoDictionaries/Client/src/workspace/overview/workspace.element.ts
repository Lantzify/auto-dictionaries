import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
	LitElement,
	css,
	customElement,
	html,
} from '@umbraco-cms/backoffice/external/lit';

import { autoDictionariesWorkspaceContext } from './workspace.context'
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {  UmbRoute } from '@umbraco-cms/backoffice/router';


@customElement("auto-dictionaries-root")
export class autoDictionariesWorkspaceRootElement extends UmbElementMixin(LitElement) {
	#workspaceContext: autoDictionariesWorkspaceContext;


	#routes: UmbRoute[] = [
		{
			path: 'edit/:id',
			component: () => import('./views/overview/overview.element')
		},
		// Default route
		{
			path: 'edit/:id',
			redirectTo: 'overview',
		},
	];

	constructor() {
		super();

		this.#workspaceContext = new autoDictionariesWorkspaceContext(this);
	}
	connectedCallback() {
		super.connectedCallback();
	}


	render() {
		return html`
			<umb-workspace-editor headline="Auto Dictionaries" alias="autoDictionaries.workspace.root" .enforceNoFooter=${true}>
			</umb-workspace-editor>
		`;
	}


	static styles = [
		UmbTextStyles,
		css`
			umb-workspace-editor > div.header {
				display: flex;
				align-items: center;
				align-content: center;
			}
		`,
	];
}

export default autoDictionariesWorkspaceRootElement;