import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import type { UmbDataSourceResponse } from "@umbraco-cms/backoffice/repository";
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { LitElement, customElement, html, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UMB_ROUTE_CONTEXT } from '@umbraco-cms/backoffice/router';
import { AutoDictionariesService, type AutoDictionariesModel } from '../../../api';




@customElement("auto-dictionaries-overview")
export class autoDictionariesOverviewViewElement extends UmbElementMixin(LitElement) {


	private _views: AutoDictionariesModel[] = [];

	private _router?: typeof UMB_ROUTE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ROUTE_CONTEXT, (router) => (this._router = router));
	}

	connectedCallback(): void {
		super.connectedCallback();
		void this.getAllViews();
	}

	private async getAllViews(): Promise<void> {
		const { data, error } = await tryExecute(this, AutoDictionariesService.getGetAllViews());
		console.log(data)
		this._views = data ?? [];
		this.requestUpdate();
	};


	private _renderView(view: AutoDictionariesModel) {
		if (!view) return;

		return html`<uui-table-row>
						<uui-table-cell>${view.name}</uui-table-cell>
						<uui-table-cell>${view.type}</uui-table-cell>
						<uui-table-cell>${view.path}</uui-table-cell>
						<uui-table-cell>
							${view.staticContent}
							${view.staticContent ?
									html`<uui-icon name="icon-check"></uui-icon>` :
									html`${view.staticContent}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>

							${view.dictionaries ?
								html`<uui-icon name="icon-check"></uui-icon>` :
								html`${view.dictionaries}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>${view.matchDictionaries > 0 ? view.matchDictionaries : ""}</uui-table-cell>
					</uui-table-row>`;
	}

	render() {
		return html`

		
			<umb-body-layout header-transparent>
				<umb-collection-toolbar slot="header">
					<umb-collection-filter-field>
						<uui-input label="Search" placeholder="Type to search..." />
					</umb-collection-filter-field>
				</umb-collection-toolbar>
			
				<uui-table aria-label="Random Umbraco Words" aria-describedby="table-description">
					<uui-table-column></uui-table-column>
					<uui-table-column></uui-table-column>
					<uui-table-column></uui-table-column>
					<uui-table-column></uui-table-column>
					<uui-table-column></uui-table-column>
					<uui-table-column></uui-table-column>

				
					<uui-table-head>
						<uui-table-head-cell>View name</uui-table-head-cell>
						<uui-table-head-cell>Type</uui-table-head-cell>
						<uui-table-head-cell>Path</uui-table-head-cell>
						<uui-table-head-cell>Static content</uui-table-head-cell>
						<uui-table-head-cell>Dictionaries</uui-table-head-cell>
						<uui-table-head-cell>Match dictionaries</uui-table-head-cell>
					</uui-table-head>
	
					${repeat(this._views, (view) => view.id, (view) => this._renderView(view))}
				</uui-table>
			


			</umb-body-layout>
		`;
	}
}

export default autoDictionariesOverviewViewElement;