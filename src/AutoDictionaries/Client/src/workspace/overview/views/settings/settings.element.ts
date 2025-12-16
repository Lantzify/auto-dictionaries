import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
	LitElement,
	customElement,
	html,
} from '@umbraco-cms/backoffice/external/lit';




@customElement("auto-dictionaries-settings")
export class autoDictionariesSettingsViewElement extends UmbElementMixin(LitElement) {

	constructor() {
		super();
	}

	render() {
		return html`
		<umb-body-layout>
	      <uui-box headline=${this.localize.term("sections_settings")}>
		  <umb-code-editor>
		  </umb-code-editor>
            </uui-box>
			</umb-body-layout>
		`;
	}
}

export default autoDictionariesSettingsViewElement;