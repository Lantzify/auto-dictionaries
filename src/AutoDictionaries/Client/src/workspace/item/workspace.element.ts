import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { LitElement, html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import AutoDictionariesItemWorkspaceContext from './workspace.context';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('auto-dictionaries-item-workspace')
export class AutoDictionariesItemWorkspaceElement extends UmbElementMixin(LitElement) {

    #workspaceContext: AutoDictionariesItemWorkspaceContext;

    #routes: UmbRoute[] = [
        {
            path: 'edit/:id',
            component: () => import('./item.element'),
            setup: (_component, info) => {
                const id = info.match.params.id;
                if (id) {
                    this.#workspaceContext.setUnique(id);
                }
            },
        },
        {
            path: '',
            redirectTo: 'edit/',
        }
    ];

    constructor() {
        super();

        this.#workspaceContext = new AutoDictionariesItemWorkspaceContext(this);
    }

    #handleBack() {
        window.history.pushState({}, '', "/umbraco/section/translation/workspace/auto-dictionaries-root/");
    }

    render() {
        return html`
            <umb-workspace-editor .enforceNoFooter=${true}>
                <uui-button id="back-button" slot="header" compact @click=${this.#handleBack} label="Back">
                    <uui-icon name="icon-arrow-left"></uui-icon>
                </uui-button>
                <div slot="header">Auto dictionaries</div>
                
                <umb-router-slot id="router-slot" .routes=${this.#routes}></umb-router-slot>
            </umb-workspace-editor>
        `;
    }

    static styles = [
        css`
            #back-button {
                order: -1;
                margin-right: var(--uui-size-space-4);
            }
        `
    ];
}

export default AutoDictionariesItemWorkspaceElement;

declare global {
    interface HTMLElementTagNameMap {
        'auto-dictionaries-item-workspace': AutoDictionariesItemWorkspaceElement;
    }
}