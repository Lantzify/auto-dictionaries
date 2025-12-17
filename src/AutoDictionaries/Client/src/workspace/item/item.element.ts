import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, customElement, html, css, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_CONTEXT, UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { type AutoDictionariesModel, type DictionaryModel, type StaticContentModel } from '../../api';
import type { AutoDictionariesItemWorkspaceContext } from './workspace.context';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_TEMPLATE_ENTITY_TYPE } from '@umbraco-cms/backoffice/template';
import { UmbModalRouteRegistrationController, type UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '@umbraco-cms/backoffice/partial-view';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { UMB_DICTIONARY_ENTITY_TYPE } from '@umbraco-cms/backoffice/dictionary';


@customElement("auto-dictionaries-item-edit")
export class autoDictionariesItemViewElement extends UmbElementMixin(LitElement) {
	private _modalContext?: UmbModalManagerContext;
	private _routeBuilder?: UmbModalRouteBuilder;

	#workspaceContext?: AutoDictionariesItemWorkspaceContext;
	#serverFilePathUniqueSerializer = new UmbServerFilePathUniqueSerializer();

	@state()
	private _translationSetting: Boolean = false

    @state()
	private _allDictionaries: DictionaryModel[] = [];

	@state()
	private _item?: AutoDictionariesModel;

	@state()
	private _isLoading = false;

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
	}

	private _renderStaticContent(staticContent: StaticContentModel) {
		if (!staticContent) return;
		const options = this._allDictionaries.map(dict => ({
				value: dict.key ?? '',
				name: dict.key ?? ''
			}));
		return html`<uui-table-row>
						<uui-table-cell style="--uui-table-cell-padding: 0; text-align: center;"><uui-checkbox /></uui-table-cell>
						<uui-table-cell>${staticContent.staticContent}</uui-table-cell>
						<uui-table-cell>${staticContent.used}</uui-table-cell>
						<uui-table-cell>
							<uui-select
								label="Parent"
								placeholder="None"
								.options=${options}>
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
							${dictionary.translated ?
							html`<uui-icon name="icon-check"></uui-icon>` :
							html`<uui-icon name="icon-alert"></uui-icon>`}
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
								<uui-table aria-label="Random Umbraco Words" aria-describedby="table-description">
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
								<uui-table aria-label="Random Umbraco Words" aria-describedby="table-description">
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>


									<uui-table-head>
										<uui-table-head-cell style="--uui-table-cell-padding: 0; text-align: center;"><uui-checkbox /></uui-table-head-cell>
										<uui-table-head-cell>Content</uui-table-head-cell>
										<uui-table-head-cell>Used in view</uui-table-head-cell>
										<uui-table-head-cell>Parent</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${repeat(this._item.staticContent ?? [], (staticContent) => staticContent.staticContent, (staticContent) => this._renderStaticContent(staticContent))}
								</uui-table>` :
							html`There is no static content in this view`}

		
						</uui-box>
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
							<strong>Id</strong>
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
		`,
	];
}

export default autoDictionariesItemViewElement;