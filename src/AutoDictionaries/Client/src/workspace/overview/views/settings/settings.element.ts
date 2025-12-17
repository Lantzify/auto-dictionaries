import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import {
	LitElement,
	css,
	customElement,
	html,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';




@customElement("auto-dictionaries-settings")
export class autoDictionariesSettingsViewElement extends UmbElementMixin(LitElement) {

	constructor() {
		super();
	}

	render() {
		return html`
		<umb-body-layout>
        <div id="autoDictionaries-layout">
			<uui-box headline=${this.localize.term("sections_settings")}>
                <p>
                    <strong>Translate</strong>
                </p>
                <p>If auto dictionary should add the option to translate dictionary items.</p>
                <hr />
                <p>
                    <strong>Translator</strong>
                </p>
                <p>Which translator to use. Thease are currently available (Case sensitive):</p>
                <ul>
                    <li>DeepL</li>
                    <li>MicrosoftTranslation</li>
                </ul>
                 <hr />
                <p>
                    <strong>ApiKey</strong>
                </p>
                <p>If the translator needs a Api key.</p>
                 <hr />
                <p>
                    <strong>ApiEndpoint (Microsoft Translation)</strong>
                </p>
                <p>Endpoint used for the translation. Only used for Microsoft Translation.</p>
                 <hr />
                <p>
                    <strong>ApiRegion (Microsoft Translation)</strong>
                </p>
                <p>Api region. Only used for Microsoft Translation.</p>


            </uui-box>

            <uui-box headline="Default settings">
			  <pre><code>{
  "AutoDictionaries": {
    "Translate": false,
    "Translator": "DeepL",
    "ApiKey": "",
    "ApiEndpoint": "",
    "ApiRegion": ""
  }
}</code></pre>
       

            </uui-box>
            </div>
		</umb-body-layout>
		`;
    }

    static styles = [
        UmbTextStyles,
        css`
            #autoDictionaries-layout {
				padding-bottom: var(--uui-size-layout-1);
				display: grid;
				grid-template-columns: 7fr 7fr;
				gap: 20px 20px;
				align-items: flex-start; 
			}

            hr {
                border: 0;
                border-top: 1px solid var(--uui-color-border);
            }

            pre {
                background-color: var(--uui-color-surface-alt);
                padding: var(--uui-size-space-4);
                border-radius: var(--uui-border-radius);
                overflow-x: auto;
                margin: 0;
            }

            code {
                font-family: var(--uui-font-family-monospace);
                font-size: 14px;
                line-height: 1.5;
                color: var(--uui-color-text);
            }
		`,
    ];
}

export default autoDictionariesSettingsViewElement;