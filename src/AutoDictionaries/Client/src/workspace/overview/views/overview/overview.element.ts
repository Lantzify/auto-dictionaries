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
			name: this.localize.term("general_all"),
			value: "all"
		},
		{
			name: this.localize.term("treeHeaders_templates"),
			value: "Template"
		},
		{
			name: this.localize.term("autoDictionaries_partial_view"),
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
			this._filterdViews = this._views?.filter(view => view?.name?.toLowerCase().includes(query.toLowerCase()));
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
						<uui-table-cell>
						<umb-localize key=${view.type === "Template" ? "template_template" : "autoDictionaries_partial_view"}></umb-localize>
						
						</uui-table-cell>
						<uui-table-cell>${view.path}</uui-table-cell>
						<uui-table-cell>

							${view.staticContent?.length  == 0 ?
									html`<uui-icon name="icon-check" title=${this.localize.term("autoDictionaries_no_static_content")}></uui-icon>` :
									html`${view.staticContent?.length}<uui-icon name="icon-alert"  title=${this.localize.term("autoDictionaries_has_static_content")}></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>

							${view.dictionaries && view.dictionaries.length > 0 ?
								view.dictionaries.length :
								""
							}

							${view.dictionaries?.every((dictionary) => dictionary.translated) ?
								html`<uui-icon name="icon-check" title=${this.localize.term("autoDictionaries_fully_translated")}></uui-icon>` :
								html`<uui-icon name="icon-alert" title=${this.localize.term("autoDictionaries_not_fully_translated")}></uui-icon>`}
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
								placeholder=${this.localize.term("placeholders_search")}
								@input=${this._filter}/>
						</div>

						<uui-select label="Select type..."
								placeholder="Select type..."
								.options=${this.#options}
								@change=${this._filterByType}></uui-select>
					</div>
				</umb-collection-toolbar>
			
				<uui-table aria-label="Views" aria-describedby="table-description">
					<uui-table-column></uui-table-column>
						
					<uui-table-head>
						<uui-table-head-cell>
							<umb-localize key="autoDictionaries_viewName"></umb-localize>
						</uui-table-head-cell>
						<uui-table-head-cell>
							<umb-localize key="autoDictionaries_type"></umb-localize>					
						</uui-table-head-cell>
						<uui-table-head-cell>
							<umb-localize key="general_path"></umb-localize>
						</uui-table-head-cell>
						<uui-table-head-cell>
							<umb-localize key="autoDictionaries_static_content"></umb-localize>
						</uui-table-head-cell>
						<uui-table-head-cell>
							<umb-localize key="autoDictionaries_dictionaries"></umb-localize>
						</uui-table-head-cell>
						<uui-table-head-cell>
							<umb-localize key="autoDictionaries_match_dictionaries"></umb-localize>	
						</uui-table-head-cell>
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

			uui-table-row:hover{
				cursor:pointer;
			}
		`,
	];
}

export default autoDictionariesOverviewViewElement;