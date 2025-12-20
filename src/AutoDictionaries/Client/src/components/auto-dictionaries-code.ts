import { LitElement, css, html } from "lit";
import { diffWords } from '@umbraco-cms/backoffice/utils';
import { customElement, property } from "lit/decorators.js";

@customElement('auto-dictionaries-code')
export class autoDictionariesCode extends LitElement{

    @property({ type: Array })
    diffCode: string[] = [];

    render() {
        const changes = this.diffCode?.length === 2 ? diffWords(this.diffCode[0], this.diffCode[1]) : [];

        const changeHtml = changes.map((change: any) => {
            if (change.added) {
                return html`<ins>${change.value}</ins>`;
            } else if (change.removed) {
                return html`<del>${change.value}</del>`;
            } else {
                return html`<span>${change.value}</span>`;
            }
        });

        return html`<pre><code>${changeHtml}</code></pre>   `;
    }

    static styles = css`
            pre ins {
                color: #2bc37c;
            }

            pre del {
	            color: #d42054;
            }

            pre {
                background-color: var(--uui-color-surface-alt);
                padding: var(--uui-size-space-4);
                border: 1px solid #d8d7d9;
                border-radius: 3px;
                overflow-x: auto;
                margin: 0;
                white-space: pre-wrap;
            }

            code {
                font-family: var(--uui-font-family-monospace);
                font-size: 14px;
                line-height: 1.5;
                color: var(--uui-color-text);
            }
    `;
}

export default autoDictionariesCode;