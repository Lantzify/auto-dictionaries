import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import AutoDictionariesRepository from "../repository/auto-dictionaries.repository";
import { AddExistingDictionaryItemToViewDto } from "../api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";


@customElement('match-dictionaries-modal')
export class MatchDictionariesElement extends UmbModalBaseElement<AddExistingDictionaryItemToViewDto, string[]> {

    #repository: AutoDictionariesRepository;

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
        const payload: AddExistingDictionaryItemToViewDto = {
            autoDictionariesModel: this.data?.autoDictionariesModel,
            dictionaryKey: this.data?.dictionaryKey ?? "",
            staticContent: this.data?.staticContent
        };

        const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

        try {
            const response = await this.#repository.postAddExistingDictionaryItem(payload);
            if (response) {
                const notification = {
                    data: {
                        message: this.localize.term('autoDictionaries_success_to_match', payload.staticContent)
                    }
                };
                notificationContext?.peek('positive', notification);
            } else {
                throw new Error(this.localize.term('autoDictionaries_failed_to_match', payload.staticContent));
            }
        } catch (error) {
            const notification = { data: { message: error as string } };
            notificationContext?.peek('danger', notification);
        }
   
        this.modalContext?.submit();
    }

    #close() {
        this.modalContext?.reject();
    }

    render() {
        return html`
            <umb-body-layout>

                <uui-box>
                    <p><umb-localize key="autoDictionaries_cannot_undo"></umb-localize></p>
                    <auto-dictionaries-code .diffCode=${this._diff}></auto-dictionaries-code>                               
                </uui-box>
              
                <div slot="actions">
      
					<uui-button
						label=${this.localize.term('general_close')}
						@click="${this.#close}"></uui-button>

                <uui-button
					label=${this.localize.term("general_submit")}
                    look="primary"
                    color="positive"
					@click=${this.#submit}></uui-button>

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