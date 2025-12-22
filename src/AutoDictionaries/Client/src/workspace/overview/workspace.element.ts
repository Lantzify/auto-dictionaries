import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement,css,
	customElement,
	html,
} from '@umbraco-cms/backoffice/external/lit';

import { autoDictionariesWorkspaceContext } from './workspace.context'
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';


@customElement("auto-dictionaries-root")
export class autoDictionariesWorkspaceRootElement extends UmbElementMixin(LitElement) {
	_workspaceContext: autoDictionariesWorkspaceContext;

	constructor() {
		super();

		this._workspaceContext = new autoDictionariesWorkspaceContext(this);
	}

	render() {
		return html`
			<umb-workspace-editor headline="Auto Dictionaries" alias="autoDictionaries.workspace" .enforceNoFooter=${true}>
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