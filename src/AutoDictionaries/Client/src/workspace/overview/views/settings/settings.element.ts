import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import autoDictionariesWorkspaceContext from '../../workspace.context';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';


@customElement("auto-dictionaries-settings")
export class autoDictionariesSettingsViewElement extends UmbElementMixin(LitElement) {

    #workspaceContext?: autoDictionariesWorkspaceContext;

    @state()
    private _translate: boolean = false;

    @state()
    private _translator?: string;

    @state()
    private _apiKey?: string;

    @state()
    private _apiEndpoint?: string;

    @state()
    private _apiRegion?: string;

	constructor() {
        super();

        this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
            this.#workspaceContext = context as autoDictionariesWorkspaceContext;
        });
    }

    async connectedCallback() {
        super.connectedCallback();

        this.#loadData();
    }

    async #loadData() {
        if (!this.#workspaceContext) return;

        const repository = this.#workspaceContext.getRepository();

        try {
            [
                this._translate,
                this._translator,
                this._apiKey,
                this._apiEndpoint,
                this._apiRegion
            ] = await Promise.all([
                repository.getTranslateSetting(),
                repository.getTranslatorSetting(),
                repository.getApiKeySetting(),
                repository.getApiEndpoint(),
                repository.getApiRegionSetting()
            ]);
        } catch (error) {
            const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
            const notification = { data: { message: this.localize.term("autoDictionaries_failed_load") } };
            notificationContext?.peek('danger', notification);
        }
    }

	render() {
		return html`
		<umb-body-layout>
        <div id="autoDictionaries-layout">
			<uui-box headline=${this.localize.term("sections_settings")}>
                <div class="settings-item">
                    <div>
                        <strong>Translate</strong><br />
                         <i><umb-localize key="autoDictionaries_settings_translate"></umb-localize></i>
                    </div>
                    <div>
                        <strong>${this._translate}</strong>
                    </div>
                </div>

                <div class="settings-item">
                    <div>
                        <strong>Translator</strong><br />
                        <i><umb-localize key="autoDictionaries_settings_translator"></umb-localize>:</i>
                        <ul>
                            <li>DeepL</li>
                            <li>MicrosoftTranslation</li>
                        </ul>
                    </div>
                    <div>
                        <strong>${this._translator}</strong>
                    </div>
                </div>

                <div class="settings-item">
                    <div>
                        <strong>ApiKey</strong><br />
                        <i><umb-localize key="autoDictionaries_settings_apiKey"></umb-localize></i>
                    </div>
                    <div>
                        <strong>${this._apiKey}</strong>
                    </div>
                </div>
   
                <div class="settings-item">
                    <div>
                        <strong>ApiEndpoint (Microsoft Translation)</strong><br />
                        <i><umb-localize key="autoDictionaries_settings_apiEndpoint"></umb-localize></i>
                    </div>
                    <div>
                        <strong>${this._apiEndpoint}</strong>
                    </div>
                </div>

                <div class="settings-item">
                    <div>
                        <strong>ApiRegion (Microsoft Translation)</strong><br />
                        <i><umb-localize key="autoDictionaries_settings_apiRegion"></umb-localize></i>
                    </div>
                    <div>
                        <strong>${this._apiRegion}</strong>
                    </div>
                </div>
            </uui-box>

            <uui-box headline=${this.localize.term("autoDictionaries_default_settings")}>
			  <pre><code>{
  "AutoDictionaries": {
    "Translate": false,
    "Translator": "DeepL",
    "ApiKey": "",
    "ApiEndpoint": "",
    "ApiRegion": ""
  }
}
</code></pre>
       
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

            .settings-item {
                display: flex;
                justify-content: space-between;
                align-items: end;
            }

            .settings-item:not(:last-child){
                padding-bottom: var(--uui-size-space-5);
                margin-bottom: var(--uui-size-space-5);
                border-bottom: 1px solid var(--uui-color-border);

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
