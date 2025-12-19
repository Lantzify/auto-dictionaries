import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, customElement, html, css, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_CONTEXT, UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { PreviewAddNewDictionaryItemToViewDto, StaticContentDto, type AutoDictionariesModel, type DictionaryModel, type StaticContentModel } from '../../api';
import type { AutoDictionariesItemWorkspaceContext } from './workspace.context';
import { UMB_TEMPLATE_ENTITY_TYPE } from '@umbraco-cms/backoffice/template';
import { UmbModalRouteRegistrationController, type UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '@umbraco-cms/backoffice/partial-view';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UMB_DICTIONARY_ENTITY_TYPE } from '@umbraco-cms/backoffice/dictionary';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';
import { GENERATE_DICTIONARY_MODAL_TOKEN } from '../../sidebar/generate-dictionaries-modal-tokent';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalContext, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

@customElement("auto-dictionaries-item-edit")
export class autoDictionariesItemViewElement extends UmbElementMixin(LitElement) {
	private _routeBuilder?: UmbModalRouteBuilder;
	private _modalContext?: UmbModalManagerContext;

	#workspaceContext?: AutoDictionariesItemWorkspaceContext;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();
	#selectionManager = new UmbSelectionManager<string>(this);

	@state()
	private _translationSetting: boolean = false

	@state()
	private _allDictionaries: DictionaryModel[] = [];

	@state()
	private _allDictionaryOptions: Array<Option>  = [];

	@state()
	private _item?: AutoDictionariesModel;

	@state()
	private _isLoading = false;

	@state()
	private _selectedContent: StaticContentDto[] = [];

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (_instance) => {
			this._modalContext = _instance;
		});

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('general/:entityType')
			.onSetup((params) => {
				return { data: { entityType: params.entityType, preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._routeBuilder = routeBuilder;
			});

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as AutoDictionariesItemWorkspaceContext;
			this.#observeContext();
		});
	}

	#observeContext() {
		if (!this.#workspaceContext) return;

		// Observe the current item
		this.observe(this.#workspaceContext.currentItem, (item) => {
			this._item = item;
		});

		// Observe loading state
		this.observe(this.#workspaceContext.isLoading, (isLoading) => {
			this._isLoading = isLoading;
		});
	}

	async connectedCallback() {
		super.connectedCallback();

		this.#loadData();
	}

	async #loadData() {
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
			{ value: '', name: 'Set parent for selected...', selected: true },
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

		//const result = await modalContext?.onSubmit();
	}


	private _renderStaticContent(staticContent: StaticContentModel) {
		if (!staticContent) return;

		const isSelected = this._selectedContent.map(x => x.staticContent).indexOf(staticContent.staticContent) !== -1;

		return html`<uui-table-row selectable 
									?selected=${isSelected}
									@selected=${() => this.#toggleSelect(staticContent)}
									@deselected=${() => this.#toggleSelect(staticContent)}>
						<uui-table-cell style="--uui-table-cell-padding: 0; text-align: center;">
							<uui-checkbox
									?checked=${isSelected}
									@click=${(e: Event) => e.stopPropagation()}
									@change=${() => this.#toggleSelect(staticContent)}>
							</uui-checkbox>
						</uui-table-cell>
						<uui-table-cell>${staticContent.staticContent}</uui-table-cell>
						<uui-table-cell>${staticContent.used}</uui-table-cell>
						<uui-table-cell>
							<uui-select
								?disabled=${!isSelected}
								.options=${this._allDictionaryOptions}
								@click=${(e: Event) => e.stopPropagation()}
								@change=${(e: Event) => {
									const select = e.target as HTMLSelectElement;
									this.#changeParent(staticContent, select.value);}}></uui-select>
						</uui-table-cell>
						<uui-table-cell>
						</uui-table-cell>
					
					</uui-table-row>`;
	}

	private _renderDictionaries(dictionary: DictionaryModel) {
		if (!dictionary) return;

		return html`<uui-table-row>
						<uui-table-cell>${dictionary.key}</uui-table-cell>
						<uui-table-cell>${dictionary.guid}</uui-table-cell>
						<uui-table-cell class="text-center">${dictionary.used}</uui-table-cell>
						<uui-table-cell class="text-center">
							<uui-icon name=${dictionary.translated ? "icon-check" : "icon-alert"}"></uui-icon>
						</uui-table-cell>

						<uui-table-cell style="text-align:right;">
							${!dictionary.translated && this._translationSetting ?
							html`	
								<uui-button label="Translate missing" 
								look="secondary"></uui-button>
							` :
							null}
						</uui-table-cell>
						<uui-table-cell> 
							<uui-button label="Open dictionary item" 
								look="link"
								href=${this._routeBuilder?.({ entityType: UMB_DICTIONARY_ENTITY_TYPE }) + 'edit/' + dictionary.guid}></uui-button>
						</uui-table-cell>
					
					</uui-table-row>`;
	}

	#renderSelectionActions() {
		if (this._selectedContent.length === 0) return;

		return html`
			<div id="selection-actions-bar">
				<div class="selection-info">
					<uui-button 
						label="Clear selection" 
						look="secondary"
						@click="${() => this._selectedContent = []}"></uui-button>

					<span><strong>${this._selectedContent.length}</strong> of ${this._item?.staticContent?.length} selected</span>
				</div>
				<div class="selection-actions">
					<uui-select
						label="Set parent for all selected"
						@change=${this.#changeAllParent}
						.options=${this._allDictionaryOptions}>
					</uui-select>
						
					<uui-button 
						label="Generate (${this._selectedContent.length}) dictionaries"
						look="secondary"
						@click=${this.#openCreateDictionaryModal}></uui-button>
				</div>
			</div>
		`;
	}

	render() {
		if (this._isLoading) {
			return html`
				<umb-body-layout header-transparent>
					<uui-loader></uui-loader>
				</umb-body-layout>
			`;
		}

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
						<uui-box class=${(this._item.dictionaries ?? []).length > 0 ? "no-padding": ""} headline="Dictionaries">

							${(this._item.dictionaries ?? []).length > 0 ?
							html`				
								<uui-table aria-label="" aria-describedby="">
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>

									<uui-table-head>
										<uui-table-head-cell>Key</uui-table-head-cell>
										<uui-table-head-cell>Id</uui-table-head-cell>
										<uui-table-head-cell class="text-center">Used in view</uui-table-head-cell>
										<uui-table-head-cell class="text-center">Translated</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${repeat(this._item.dictionaries ?? [], (dictionary) => dictionary.guid, (dictionary) => this._renderDictionaries(dictionary))}
								</uui-table>` :
							html`
								${(this._item.staticContent ?? []).length > 0 ?
								html`
									<uui-icon name="icon-alert-alt"></uui-icon>` : null}
									There are no dictionaries in this view`}		
						</uui-box>	
			
						<uui-box class=${(this._item.staticContent ?? []).length > 0 ? "no-padding" : ""} headline="Static Content">

							${(this._item.staticContent ?? []).length > 0 ?
							html`
								<uui-table selectable aria-label="Static content" aria-describedby="table-description">
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
										<uui-table-head-cell>Content</uui-table-head-cell>
										<uui-table-head-cell>Used in view</uui-table-head-cell>
										<uui-table-head-cell>Parent</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${repeat(this._item.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this._renderStaticContent(staticContent))}
								</uui-table>` :
							html`There is no static content in this view`}

		
						</uui-box>

						${this.#renderSelectionActions()}
					</div>
					
					<uui-box headline="General">

						<div class="general-item">
							<strong>Name</strong>
							<span>${this._item.name}</span>
						</div>

						<div class="general-item">
							<strong>Alias</strong>
							<span>${this._item.alias}</span>
						</div>

						<div class="general-item">
							<strong>Type</strong>
							<span>${this._item.type}</span>
						</div>

					

						<div class="general-item">
							<strong>View</strong>
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
							<strong>Id</strong>
							<span>${this._item.id}</span>
						</div>

						<div class="general-item">
							<strong>Key</strong>
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