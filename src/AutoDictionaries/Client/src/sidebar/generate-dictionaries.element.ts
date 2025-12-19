import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, css, customElement, html, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalContext } from "@umbraco-cms/backoffice/modal";
import { diffWords } from '@umbraco-cms/backoffice/utils';
import AutoDictionariesRepository from "../repository/auto-dictionaries.repository";
import { AddNewDictionaryItemToViewDto, PreviewAddNewDictionaryItemToViewDto, StaticContentDto } from "../api";


@customElement('generate-dictionaries-modal')
export class GenerateDictionariesElement extends UmbElementMixin(LitElement) {

    modalContext?: UmbModalContext;

    #repository: AutoDictionariesRepository;

    @state()
    private data: PreviewAddNewDictionaryItemToViewDto | undefined;

    @state()
    private _diff: string[] | undefined;

    constructor() {
        super();
        this.#repository = new AutoDictionariesRepository(this);
    }

    async connectedCallback() {
        super.connectedCallback();

        if (this.#repository) {
            if (this.data) {
                this._diff = await this.#repository.postPreviewAddNewDictionaryItem(this.data)
            }
        }
    }

    #close() {
        this.modalContext?.reject();
    }

    #renderSelectedContent(staticContent: StaticContentDto) {
        return html`
            <li>
                 <strong>${staticContent.staticContent}</strong>
                 ${staticContent.parent !== ""
                        ? html` <span>with parent<strong> ${staticContent.parent}</strong></span>` 
                        : null}
           </li>`;
    }

    render() {
        const changes = diffWords(this._diff[0], this._diff[1]);

        const changeHtml = changes.map((change: any) => {
            if (change.added) {
                return html`<ins>${change.value}</ins>`;
            } else if (change.removed) {
                return html`<del>${change.value}</del>`;
            } else {
                return html`<span>${change.value}</span>`;
            }
        });

        return html`
            <umb-body-layout>

                <uui-box>
                    <p>Are you sure you want to generate dictionaries for:</p>
                    <ul>
                        ${repeat(this.data?.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this.#renderSelectedContent(staticContent))}
                    </ul>
                    <p>This action can not be undone.</p>

                  <pre><code>${changeHtml}</code></pre>
                </uui-box>
              


                <div slot="actions">
      
					<uui-button
						label=${this.localize.term('general_close')}
						@click="${this.#close}"></uui-button>

       
                    ${this.data.canTranslate ?
                        html`
                            <uui-button
                                look="secondary"
                                label="Generate"
						        @click="${this.#close}"></uui-button>

                                <uui-button
						            label="Generate and Translate"
                                    look="primary"
                                    color="positive"
                                    @click="${this.#close}"></uui-button>
                        ` : html`
                        <uui-button
						    label="general_submit"
                            look="primary"
                            color="positive"
						    @click="${this.#close}"></uui-button>`}

				</div>
            </umb-body-layout>`;
    }

    static styles = [
        css`
            pre ins{
                color: #2bc37c;
            }

            pre del {
	            color: #d42054;
            }

            pre {
                background-color: var(--uui-color-surface-alt);
                padding: var(--uui-size-space-4);
                border: 1px solid #d8d7d9;
                border-radius: 3px;
                overflow-x: auto;
                margin: 0;
                white-space: pre-wrap;
            }

            code {
                font-family: var(--uui-font-family-monospace);
                font-size: 14px;
                line-height: 1.5;
                color: var(--uui-color-text);
            }
            #footer{
                
            }
            
        `
    ];
}


export default GenerateDictionariesElement;