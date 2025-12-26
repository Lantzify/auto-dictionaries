import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, customElement, html, css, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_CONTEXT, UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { PreviewAddNewDictionaryItemToViewDto, StaticContentDto, type AutoDictionariesModel, type DictionaryModel, type StaticContentModel, AddExistingDictionaryItemToViewDto } from '../../api';
import type { AutoDictionariesItemWorkspaceContext } from './workspace.context';
import { UMB_TEMPLATE_ENTITY_TYPE } from '@umbraco-cms/backoffice/template';
import { UmbModalRouteRegistrationController, type UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '@umbraco-cms/backoffice/partial-view';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UMB_DICTIONARY_ENTITY_TYPE } from '@umbraco-cms/backoffice/dictionary';
import { GENERATE_DICTIONARY_MODAL_TOKEN } from '../../sidebar/generate-dictionaries-modal-tokent';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { MATCH_DICTIONARY_MODAL_TOKEN } from '../../sidebar/match-dictionaries-modal-tokent';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from "@umbraco-cms/backoffice/notification";

@customElement("auto-dictionaries-item-edit")
export class autoDictionariesItemViewElement extends UmbElementMixin(LitElement) {
	private _routeBuilder?: UmbModalRouteBuilder;
	private _modalContext?: UmbModalManagerContext;
	private _modalManagerContext?: UmbModalManagerContext;

	#workspaceContext?: AutoDictionariesItemWorkspaceContext;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();
	#notificationContext?: UmbNotificationContext;

	@state()
	private _translationSetting: boolean = false

	@state()
	private _allDictionaries: DictionaryModel[] = [];

	@state()
	private _allDictionaryOptions: Array<Option>  = [];

	@state()
	private _item?: AutoDictionariesModel;

	@state()
	private _selectedContent: StaticContentDto[] = [];

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('general/:entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalContext = _instance;
		});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as AutoDictionariesItemWorkspaceContext;
			this.#observeContext();
		});
	}

	#observeContext() {
		if (!this.#workspaceContext) return;

		this.observe(this.#workspaceContext.currentItem, (item) => {
			this._item = item;
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalManagerContext = _instance;
		});
	}

	async connectedCallback() {
		super.connectedCallback();

		this.#loadData();
	}

	async #loadData() {
		this.#notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		if (!this.#workspaceContext) return;

		const repository = this.#workspaceContext.getRepository();

		[
			this._allDictionaries,
			this._translationSetting
		] = await Promise.all([
			repository.getAllDictionaryItems(),
			repository.getTranslateSetting()
		]);

		this._allDictionaryOptions = [
			{ value: '', name: this.localize.term("autoDictionaries_none"), selected: true },
				...this._allDictionaries.map(dict => ({
					value: dict.key ?? '',
					name: dict.key ?? '',
					selected: false
				}))]
	}

	//Selection
	#toggleSelect(staticContet: StaticContentModel) {
		const item = {
			staticContent: staticContet.staticContent,
			safeAlias: "",
			parent: ""
		} as StaticContentDto;

		var pos = this._selectedContent.map(x => x.staticContent).indexOf(item.staticContent);

		if (pos !== -1) {
			this._selectedContent = [
				...this._selectedContent.slice(0, pos),
				...this._selectedContent.slice(pos + 1)
			];
		} else {
			this._selectedContent = [...this._selectedContent, item];
		}
	}

	#changeParent(staticContet: StaticContentModel, parent: string) {
		var pos = this._selectedContent.map(x => x.staticContent).indexOf(staticContet.staticContent);
		if (pos !== -1) {
			this._selectedContent = [
				...this._selectedContent.slice(0, pos),
				{ ...this._selectedContent[pos], parent },
				...this._selectedContent.slice(pos + 1)
			];
		}
	}

	#toggleSelectAll() {
		if (this._selectedContent.length !== this._item?.staticContent?.length) {
			this._selectedContent = this._item?.staticContent?.map(sc => ({
					staticContent: sc.staticContent,
					safeAlias: "",
					parent: ""
				} as StaticContentDto)) ?? []
		} else {
			this._selectedContent = [];
		}
	}

	#changeAllParent(event: Event) {
		const select = event.target as HTMLSelectElement;
		this._selectedContent = this._selectedContent.map(sc => ({
			staticContent: sc.staticContent,
			parent: select.value
		}))
	}

	//

	async #openCreateDictionaryModal() {
		const modalContext = await this._modalContext?.open(this, GENERATE_DICTIONARY_MODAL_TOKEN, {
			data: {
                staticContent: this._selectedContent,
				autoDictionariesModel: this._item,
				canTranslate: this._translationSetting
			} as PreviewAddNewDictionaryItemToViewDto
		});

		await modalContext?.onSubmit().then(async () => {
			this._selectedContent = [];

			await this.#refreshItem();
		});
	}

	async #openMatchDictionaryModal(staticContent: StaticContentModel) {
		const modalContext = await this._modalContext?.open(this, MATCH_DICTIONARY_MODAL_TOKEN, {
			data: {
				staticContent: staticContent.staticContent,
				dictionaryKey: staticContent.dictionary?.guid,
				autoDictionariesModel: this._item
			} as AddExistingDictionaryItemToViewDto
		});

		await modalContext?.onSubmit().then(async () => {
			await this.#refreshItem();
		})
	}

	async #refreshItem() {
		if (!this.#workspaceContext) return;

		const repository = this.#workspaceContext.getRepository();

		const updatedItem = await repository.getViewById(this.#workspaceContext?.getUnique() ?? "");
		if (updatedItem) {
			this._item = updatedItem;
		}

		this._allDictionaries = await repository.getAllDictionaryItems();
		this._allDictionaryOptions = [
			{ value: '', name: this.localize.term("autoDictionaries_none"), selected: true },
			...this._allDictionaries.map(dict => ({
				value: dict.key ?? '',
				name: dict.key ?? '',
				selected: false
			}))
		];
	}

	#translateMissing(dictionary: DictionaryModel) {
		const modalContext = this._modalManagerContext?.open(
			this, UMB_CONFIRM_MODAL, {
			data: {
					headline: this.localize.term("autoDictionaries_translate_item", dictionary.key),
					content: this.localize.term("autoDictionaries_are_you_sure_translate"),
					color: "positive",
					confirmLabel: this.localize.term("actions_translate"),
					cancelLabel: this.localize.term("general_close") 
			}
		});

		modalContext?.onSubmit().then(async () => {
			const result = await this.#workspaceContext?.getRepository().getTranslateDictionaryItem(dictionary.guid);

			if (result) {
				const notification = { data: { message: this.localize.term("autoDictionaries_success_to_translate", dictionary.key) } };
				this.#notificationContext?.peek('positive', notification);
				if (this._item?.dictionaries) {
					const index = this._item.dictionaries.findIndex(d => d.guid === dictionary.guid);
					if (index !== -1) {
						const updatedDictionaries = [...this._item.dictionaries];
						updatedDictionaries[index] = {
							...updatedDictionaries[index],
							translated: true
						};

						this._item = {
							...this._item,
							dictionaries: updatedDictionaries
						};
					}
				}
			} else {
				const notification = { data: { message: this.localize.term("autoDictionaries_failed_to_translate", dictionary.key) } };
				this.#notificationContext?.peek('danger', notification);
			}
		});
	}


	private _renderDictionaries(dictionary: DictionaryModel) {
		if (!dictionary) return;

		return html`<uui-table-row>
						<uui-table-cell>${dictionary.key}</uui-table-cell>
						<uui-table-cell>${dictionary.guid}</uui-table-cell>
						<uui-table-cell class="text-center">${dictionary.used}</uui-table-cell>
						<uui-table-cell class="text-center">
							<uui-icon name=${dictionary.translated ? "icon-check" : "icon-alert"} title=${this.localize.term(dictionary.translated ? "autoDictionaries_single_fully_translated" : "autoDictionaries_single_not_fully_translated") }></uui-icon>
						</uui-table-cell>

						<uui-table-cell style="text-align:right;">
							${!dictionary.translated && this._translationSetting ?
							html`	
								<uui-button label=${this.localize.term("actions_translate")}
								look="secondary"
								@click=${() => this.#translateMissing(dictionary)}></uui-button>
							` :
							null}
						</uui-table-cell>
						<uui-table-cell> 
							<uui-button label=${this.localize.term("general_open")}
								look="link"
								href=${this._routeBuilder?.({ entityType: UMB_DICTIONARY_ENTITY_TYPE }) + 'edit/' + dictionary.guid}></uui-button>
						</uui-table-cell>
					
					</uui-table-row>`;
	}

	private _renderStaticContent(staticContent: StaticContentModel) {
		if (!staticContent) return;

		const selectedItem = this._selectedContent.find(x => x.staticContent === staticContent.staticContent);

		return html`<uui-table-row selectable 
									?selected=${!!selectedItem}
									@selected=${() => this.#toggleSelect(staticContent)}
									@deselected=${() => this.#toggleSelect(staticContent)}>
						<uui-table-cell style="--uui-table-cell-padding: 0; text-align: center;">
							<uui-checkbox
									?checked=${!!selectedItem}
									@click=${(e: Event) => e.stopPropagation()}
									@change=${() => this.#toggleSelect(staticContent)}>
							</uui-checkbox>
						</uui-table-cell>
						<uui-table-cell>${staticContent.staticContent}</uui-table-cell>
						<uui-table-cell>${staticContent.used}</uui-table-cell>
						<uui-table-cell>
							<uui-select
								?disabled=${!!!selectedItem}
								.value=${ifDefined(selectedItem?.parent)}
								.options=${this._allDictionaryOptions}
								@click=${(e: Event) => e.stopPropagation()}
								@change=${(e: Event) => {
											const select = e.target as HTMLSelectElement;
											this.#changeParent(staticContent, select.value);
										}}></uui-select>
						</uui-table-cell>
						<uui-table-cell>
							${staticContent.dictionary ?
								html`<uui-button
										look="primary"
										label=${this.localize.term("autoDictionaries_convert_into_existing_dictionary")}
										@click=${() => this.#openMatchDictionaryModal(staticContent)}
									</uui-button>` : null}
						</uui-table-cell>
					
					</uui-table-row>`;
	}

	#renderSelectionActions() {
		if (this._selectedContent.length === 0) return;

		return html`
			<div id="selection-actions-bar">
				<div class="selection-info">
					<uui-button 
						label=${this.localize.term("autoDictionaries_clear_selection")}
						look="secondary"
						@click="${() => this._selectedContent = []}"></uui-button>

					<span>
						<umb-localize key="autoDictionaries_x_of_x_selected" args="[${this._selectedContent.length}, ${this._item?.staticContent?.length}]"></umb-localize>
					</span>
				</div>
				<div class="selection-actions">
					<uui-select
						label=${this.localize.term("autoDictionaries_none")}
						@change=${this.#changeAllParent}
						.options=${this._allDictionaryOptions}>
					</uui-select>
						
					<uui-button 
						label=${this.localize.term("autoDictionaries_generate_x_dictionaries", this._selectedContent.length)}
						look="secondary"
						@click=${this.#openCreateDictionaryModal}></uui-button>
				</div>
			</div>
		`;
	}

	render() {
		if (!this._item) {
			return html`
				<umb-body-layout header-transparent>
					<p>No item loaded</p>
				</umb-body-layout>
			`;
		}

		return html`
			<umb-body-layout header-transparent>
				<div id="autoDictionaries-layout">
					<div id="autoDictionaries-main">
						<uui-box class=${(this._item.dictionaries ?? []).length > 0 ? "no-padding" : ""} headline=${this.localize.term("autoDictionaries_dictionaries")}>

							${(this._item.dictionaries ?? []).length > 0 ?
							html`				
								<uui-table>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>

									<uui-table-head>
										<uui-table-head-cell>
											<umb-localize key="autoDictionaries_key"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell>
											<umb-localize key="template_id"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell class="text-center">
											<umb-localize key="autoDictionaries_used_in_view"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell class="text-center">
											<umb-localize key="autoDictionaries_translated"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${repeat(this._item.dictionaries ?? [], (dictionary) => dictionary.guid, (dictionary) => this._renderDictionaries(dictionary))}
								</uui-table>` :
							html`
								${(this._item.staticContent ?? []).length > 0 ?
								html`
									<uui-icon name="icon-alert-alt"></uui-icon>` : null}
									<umb-localize key="autoDictionaries_no_dictionaries"></umb-localize>`}		
						</uui-box>	
			
						<uui-box class=${(this._item.staticContent ?? []).length > 0 ? "no-padding" : ""} headline=${this.localize.term("autoDictionaries_static_content")}>

							${(this._item.staticContent ?? []).length > 0 ?
							html`
								<uui-table selectable>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>

									<uui-table-head>
										<uui-table-head-cell style="--uui-table-cell-padding: 0; text-align: center;">
											<uui-checkbox
												?checked=${this._selectedContent.length === this._item?.staticContent?.length}
												@change=${() => this.#toggleSelectAll()}>
											</uui-checkbox>
										</uui-table-head-cell>
										<uui-table-head-cell>
											<umb-localize key="sections_content"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell>
											<umb-localize key="autoDictionaries_used_in_view"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell>
											<umb-localize key="autoDictionaries_parent"></umb-localize>
										</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${repeat(this._item.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this._renderStaticContent(staticContent))}
								</uui-table>` :
							html`<umb-localize key="autoDictionaries_no_static_content"></umb-localize>`}

		
						</uui-box>

						${this.#renderSelectionActions()}
					</div>
					
					<uui-box headline="General">

						<div class="general-item">
							<strong>
								<umb-localize key="general_name"></umb-localize>
							</strong>
							<span>${this._item.name}</span>
						</div>

						<div class="general-item">
							<strong>
								<umb-localize key="content_alias"></umb-localize>
							</strong>
							<span>${this._item.alias}</span>
						</div>

						<div class="general-item">
							<strong>
								<umb-localize key="autoDictionaries_type"></umb-localize>
							</strong>
							<span>${this._item.type}</span>
						</div>

					

						<div class="general-item">
							<strong>
								<umb-localize key="autoDictionaries_view"></umb-localize>
							</strong>
							<span>
								<uui-ref-node standalone name=${this._item.name} detail="${this._item.path}" 
									href=${ifDefined(this._item.type === "Template" ?

									this._routeBuilder?.({ entityType: UMB_TEMPLATE_ENTITY_TYPE }) + 'edit/' + this._item.key
									:
									this._routeBuilder?.({ entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE }) + 'edit/' + this.#serverFilePathUniqueSerializer.toUnique("/" + this._item.path)

								)}>
									<uui-icon slot="icon" name="icon-document-html" aria-hidden="true"></uui-icon>
								</uui-ref-node>
							</span>
						</div>

						<div class="general-item">
							<strong>
								<umb-localize key="template_id"></umb-localize>
							</strong>
							<span>${this._item.id}</span>
						</div>

						<div class="general-item">
							<strong>
								<umb-localize key="autoDictionaries_key"></umb-localize>
							</strong>
							<span>${this._item.key}</span>
						</div>

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
				grid-template-columns: 7fr 2fr;
				gap: 20px 20px;
				align-items: flex-start; 
			}

			#autoDictionaries-main{
				display: flex;
				flex-direction: column;
				gap: 20px;
			}

			.property {
				margin-bottom: var(--uui-size-space-3);
			}

			uui-loader {
				margin: var(--uui-size-layout-2) auto;
				display: block;
			}

			.general-item {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-1);
			}

			.general-item:not(:last-child) {
				margin-bottom: var(--uui-size-space-6);
			}

		   uui-box.no-padding {
			  --uui-box-default-padding: 0;
			}

			.text-center{
				text-align:center;
			}

			uui-table-head-cell{
				--uui-table-cell-padding: 0 15px!important;
			}

			#selection-actions-bar {
				display: flex;
				justify-content: space-between;
				align-items: center;
				padding: var(--uui-size-space-3) var(--uui-size-space-5);
				margin-bottom: var(--uui-size-space-5);
				background-color: var(--uui-color-selected);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-selected-contrast);
				box-shadow: var(--uui-shadow-depth-1);
			}

			#selection-actions-bar .selection-info {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			#selection-actions-bar .selection-actions {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			#selection-actions-bar .selection-actions uui-select {
				min-width: 220px;
			}
		`,
	];
}

export default autoDictionariesItemViewElement;