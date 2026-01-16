import { css, customElement, html, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import AutoDictionariesRepository from "../repository/auto-dictionaries.repository";
import { AddNewDictionaryItemToViewDto, PreviewAddNewDictionaryItemToViewDto, StaticContentDto } from "../api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";


@customElement('generate-dictionaries-modal')
export class GenerateDictionariesElement extends UmbModalBaseElement<PreviewAddNewDictionaryItemToViewDto, string[]> {

    #repository: AutoDictionariesRepository;

    @state()
    private _diff: string[] | undefined;

    @state()
    private currentlyGenerating: string = "";

    @state()
    private generatingPercentage: number = 0;

    @state()
    private isGenerating: boolean = false;

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

    async #submit(shouldTranslate: boolean) {
        if (!this.data?.staticContent || this.data.staticContent.length === 0) {
            return;
        }

        this.isGenerating = true;
        this.generatingPercentage = 0;

        const percentage = 100 / this.data.staticContent.length;
        let counter = 0;

        const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

        const generateDictionary = async (staticContent: StaticContentDto): Promise<void> => {
            this.currentlyGenerating = staticContent.staticContent ?? "";

            try {
                const payload: AddNewDictionaryItemToViewDto = {
                    translate: shouldTranslate,
                    autoDictionariesModel: this.data?.autoDictionariesModel,
                    staticContent: staticContent
                };
              
                const response = await this.#repository.postAddNewDictionaryItem(payload);
                if (response) {
                    this.generatingPercentage += percentage;
                    counter += 1;

                    await new Promise(resolve => setTimeout(resolve, 1000));

                    if (counter !== this.data!.staticContent!.length) {
                        await generateDictionary(this.data!.staticContent![counter]);
                    } else {
                        const notification = { data: { message: this.localize.term('autoDictionaries_success_to_add_dictionary') } };
                        notificationContext?.peek('positive', notification);
                        this.isGenerating = false;
                        this.modalContext?.submit();
                    }
                } else {
                    throw new Error(this.localize.term('autoDictionaries_failed_to_add_dictionary'));
                }
            } catch (error) {
                const notification = { data: { message: error as string} };
                notificationContext?.peek('danger', notification);
                this.isGenerating = false;
                this.modalContext?.reject();
            }
        };

        await generateDictionary(this.data.staticContent[counter]);
    }

    #close() {
        this.modalContext?.reject();
    }

    #renderSelectedContent(staticContent: StaticContentDto) {
        return html`
            <li>
                 <strong>${staticContent.staticContent}</strong>
                 ${staticContent.parent !== ""
            ? html` <span> <umb-localize key="autoDictionaries_with_parent"></umb-localize> <strong> ${staticContent.parent}</strong></span>` 
                        : null}
           </li>`;
    }

    render() {
        return html`
            <umb-body-layout>

                <uui-box>
                    ${this.isGenerating ? 
                        html`
                            <div class="progress-container">
                                <uui-icon name="icon-globe" style="font-size: 50px;"></uui-icon>
                                <h2>
                                    <umb-localize key="autoDictionaries_generating_dictionaries"></umb-localize>...
                                    ${Math.round(this.generatingPercentage)}%
                                </h2>
                                <p><umb-localize key="autoDictionaries_currently_processing"></umb-localize>: <strong>${this.currentlyGenerating}</strong></p>
                                <uui-progress-bar progress=${Math.round(this.generatingPercentage)}></uui-progress-bar>
                            </div>
                        ` :
                        html`
                        
                            <p><umb-localize key="autoDictionaries_are_you_sure_generate_dictionaries_for"></umb-localize>:</p>
                            <ul>
                                ${repeat(this.data?.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this.#renderSelectedContent(staticContent))}
                            </ul>
                            <p><umb-localize key="autoDictionaries_cannot_undo"></umb-localize>.</p>

                            <auto-dictionaries-code .diffCode=${this._diff}></auto-dictionaries-code>
                        `}  
                  </uui-box>
              


                <div slot="actions">
      
					<uui-button
						label=${this.localize.term('general_close')}
						@click="${this.#close}"></uui-button>

                    ${this.data?.canTranslate ?
                        html`
                            <uui-button
                                look="secondary"
                                label=${this.localize.term("autoDictionaries_generate")}
						        @click="${() => this.#submit(false)}"></uui-button>

                            <uui-button
						        label=${this.localize.term("autoDictionaries_generate_and_translate")}
                                look="primary"
                                color="positive"
                                @click="${() => this.#submit(true)}"></uui-button>
                        ` : html`
                        <uui-button
						    label=${this.localize.term("general_submit")}
                            look="primary"
                            color="positive"
						    @click="${() => this.#submit(false)}"></uui-button>`}

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


export default GenerateDictionariesElement;