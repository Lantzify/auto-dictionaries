import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { type AutoDictionariesModel } from '../../../../api';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';
import autoDictionariesWorkspaceContext from '../../workspace.context';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';


@customElement("auto-dictionaries-overview")
export class autoDictionariesOverviewViewElement extends UmbElementMixin(LitElement) {

	#options: Array<Option> = [
		{
			name: "All",
			value: "all"
		},
		{
			name: "Templates",
			value: "Template"
		},
		{
			name: "Partial views",
			value: "Partial view"
		}
	];

	#workspaceContext?: autoDictionariesWorkspaceContext;

	@state()
	private _views?: AutoDictionariesModel[] = [];

	@state()
	private _filterdViews?: AutoDictionariesModel[] = [];

	@state()
	private _isLoading = false;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (context) => {
			this.#workspaceContext = context as autoDictionariesWorkspaceContext;
			this.#observeContext();
		});
	}

	#observeContext() {
		if (!this.#workspaceContext) return;

		this.observe(this.#workspaceContext.views, (view) => {
			this._views = view;
			this._filterdViews = view;
		});

		this.observe(this.#workspaceContext.isLoading, (isLoading) => {
			this._isLoading = isLoading;
		});
	}

	connectedCallback() {
		super.connectedCallback();

		if (this.#workspaceContext) {
			this.#workspaceContext.load();
		}
	}

	private _openView(view: AutoDictionariesModel) {
		if (!view?.id) return;

		const id = view.type === "Template" ? view.key.toString() : view.id.toString();
		window.history.pushState({}, '', `/umbraco/section/translation/workspace/auto-dictionaries-item/edit/${id}`);
	
	};

	private _filter(e: InputEvent) {
		const query = (e.target as HTMLInputElement).value;

		if (query) {
			this._filterdViews = this._views?.filter(view => view.name.toLowerCase().includes(query.toLowerCase()));
		} else {
			this._filterdViews = this._views;
		}
	}

	private _filterByType(e: UUISelectEvent) {
		const type = e.target.value as string;

		if (type !== this.#options[0].value) {
			this._filterdViews = this._views?.filter(view => view.type == type);
		} else {
			this._filterdViews = this._views;
		}
	}

	private _renderView(view: AutoDictionariesModel) {
		if (!view) return;

		return html`<uui-table-row @click=${() => this._openView(view)}>
						<uui-table-cell>${view.name}</uui-table-cell>
						<uui-table-cell>${view.type}</uui-table-cell>
						<uui-table-cell>${view.path}</uui-table-cell>
						<uui-table-cell>

							${view.staticContent?.length  == 0 ?
									html`<uui-icon name="icon-check"></uui-icon>` :
									html`${view.staticContent?.length}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>

							${view.dictionaries && view.dictionaries.length > 0 ?
								view.dictionaries.length :
								""
							}

							${view.staticContent?.length == 0 && view.dictionaries?.every((dictionary) => dictionary.translated) ?
								html`<uui-icon name="icon-check"></uui-icon>` :
								html`<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>${view.matchDictionaries > 0 ? view.matchDictionaries : ""}</uui-table-cell>
					</uui-table-row>`;
	}

	render() {
		return html`

		
			<umb-body-layout header-transparent>
				<umb-collection-toolbar slot="header">
					<div id="toolbar">
						<div>
							<uui-input 
								label="Search"
								placeholder="Type to search..."
								@input=${this._filter}/>
						</div>

						<uui-select label="Select type..."
								placeholder="Select type..."
								.options=${this.#options}
								@change=${this._filterByType}></uui-select>
					</div>
				</umb-collection-toolbar>
			
				<uui-table aria-label="Random Umbraco Words" aria-describedby="table-description">
					<uui-table-column></uui-table-column>
		

				
					<uui-table-head>
						<uui-table-head-cell>View name</uui-table-head-cell>
						<uui-table-head-cell>Type</uui-table-head-cell>
						<uui-table-head-cell>Path</uui-table-head-cell>
						<uui-table-head-cell>Static content</uui-table-head-cell>
						<uui-table-head-cell>Dictionaries</uui-table-head-cell>
						<uui-table-head-cell>Match dictionaries</uui-table-head-cell>
					</uui-table-head>

					${repeat(this._filterdViews ?? [], (view) => view.id, (view) => this._renderView(view))}
				</uui-table>
			
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-table-head {
				position: sticky;
				top: 0;
				z-index: 1;
				background-color: var(--uui-color-surface, #fff);
			}

			#toolbar { 
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between; 
				align-items: center; 
			}

			#toolbar > div{
				display: inline-flex;
			}

			#toolbar > div, uui-input, uui-select{
				width:100%;
			}
		`,
	];
}

export default autoDictionariesOverviewViewElement;