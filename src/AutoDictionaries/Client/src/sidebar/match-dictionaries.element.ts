import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, css, customElement, html, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalContext } from "@umbraco-cms/backoffice/modal";
import AutoDictionariesRepository from "../repository/auto-dictionaries.repository";
import { AddExistingDictionaryItemToViewDto } from "../api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";


@customElement('match-dictionaries-modal')
export class MatchDictionariesElement extends UmbElementMixin(LitElement) {

    modalContext?: UmbModalContext;

    #repository: AutoDictionariesRepository;

    @state()
    private data: AddExistingDictionaryItemToViewDto | undefined;

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
                this._diff = await this.#repository.postPreviewAddExistingDictionaryItem(this.data)
            }
        }
    }

    async #submit() {

    }

    #close() {
        this.modalContext?.reject();
    }

    render() {
        return html`
            <umb-body-layout>

                <uui-box>
                    <p>This action can not be undone.</p>
                    <auto-dictionaries-code .diffCode=${this._diff}></auto-dictionaries-code>                               
                  </uui-box>
              


                <div slot="actions">
      
					<uui-button
						label=${this.localize.term('general_close')}
						@click="${this.#close}"></uui-button>

                <uui-button
					label="general_submit"
                    look="primary"
                    color="positive"
					@click="${this.#close}"></uui-button>

				</div>
            </umb-body-layout>`;
    }

    static styles = [
        css`
            .progress-container {
                text-align:center;   
            }

            .progress-container h2{
                margin-bottom:0;
            }

            .progress-container p {
                margin-top:0;
            }
        `
    ];
}


export default MatchDictionariesElement;