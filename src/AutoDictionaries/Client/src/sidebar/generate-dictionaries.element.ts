import { UmbElementMixin } from "@umbraco-cms/backoffice/element-api";
import { LitElement, css, customElement, html, repeat, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalContext } from "@umbraco-cms/backoffice/modal";
import AutoDictionariesRepository from "../repository/auto-dictionaries.repository";
import { AddNewDictionaryItemToViewDto, PreviewAddNewDictionaryItemToViewDto, StaticContentDto } from "../api";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";


@customElement('generate-dictionaries-modal')
export class GenerateDictionariesElement extends UmbElementMixin(LitElement) {

    modalContext?: UmbModalContext;

    #repository: AutoDictionariesRepository;

    @state()
    private data: PreviewAddNewDictionaryItemToViewDto | undefined;

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
                        const notification = { data: { message: "All dictionaries were created and added to the template successfully!" } };
                        notificationContext?.peek('positive', notification);
                        this.isGenerating = false;
                        this.modalContext?.submit();
                    }
                } else {
                    throw new Error("Failed to add dictionary to template");
                }
            } catch (error) {
                const notification = { data: { message: "Failed to add dictionary to template" } };
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
                        ? html` <span>with parent<strong> ${staticContent.parent}</strong></span>` 
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
                                <h2>Generating dictionaries... ${Math.round(this.generatingPercentage)}%</h2>
                                <p>Currently processing: <strong>${this.currentlyGenerating}</strong></p>
                                <uui-progress-bar progress=${Math.round(this.generatingPercentage)}></uui-progress-bar>
                            </div>
                        ` :
                        html`
                        
                            <p>Are you sure you want to generate dictionaries for:</p>
                            <ul>
                                ${repeat(this.data?.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this.#renderSelectedContent(staticContent))}
                            </ul>
                            <p>This action can not be undone.</p>

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
                                label="Generate"
						        @click="${() => this.#submit(false)}"></uui-button>

                                <uui-button
						            label="Generate and Translate"
                                    look="primary"
                                    color="positive"
                                    @click="${() => this.#submit(true)}"></uui-button>
                        ` : html`
                        <uui-button
						    label=${this.localize.term("general_submit")}
                            look="primary"
                            color="positive"
						    @click="${this.#close}"></uui-button>`}

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