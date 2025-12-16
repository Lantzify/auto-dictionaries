var L = Object.create;
var T = Object.defineProperty;
var N = Object.getOwnPropertyDescriptor;
var D = (t, e) => (e = Symbol[t]) ? e : Symbol.for("Symbol." + t), p = (t) => {
  throw TypeError(t);
};
var W = (t, e, i) => e in t ? T(t, e, { enumerable: !0, configurable: !0, writable: !0, value: i }) : t[e] = i;
var A = (t, e) => T(t, "name", { value: e, configurable: !0 });
var w = (t) => [, , , L(t?.[D("metadata")] ?? null)], B = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], h = (t) => t !== void 0 && typeof t != "function" ? p("Function expected") : t, O = (t, e, i, n, l) => ({ kind: B[t], name: e, metadata: n, addInitializer: (a) => i._ ? p("Already initialized") : l.push(h(a || null)) }), Y = (t, e) => W(e, D("metadata"), t[3]), M = (t, e, i, n) => {
  for (var l = 0, a = t[e >> 1], c = a && a.length; l < c; l++) e & 1 ? a[l].call(i) : n = a[l].call(i, n);
  return n;
}, R = (t, e, i, n, l, a) => {
  var c, r, C, m, y, u = e & 7, g = !!(e & 8), d = !!(e & 16), f = u > 3 ? t.length + 1 : u ? g ? 1 : 2 : 0, k = B[u + 5], E = u > 3 && (t[f - 1] = []), q = t[f] || (t[f] = []), s = u && (!d && !g && (l = l.prototype), u < 5 && (u > 3 || !d) && N(u < 4 ? l : { get [i]() {
    return P(this, a);
  }, set [i](o) {
    return S(this, a, o);
  } }, i));
  u ? d && u < 4 && A(a, (u > 2 ? "set " : u > 1 ? "get " : "") + i) : A(l, i);
  for (var v = n.length - 1; v >= 0; v--)
    m = O(u, i, C = {}, t[3], q), u && (m.static = g, m.private = d, y = m.access = { has: d ? (o) => K(l, o) : (o) => i in o }, u ^ 3 && (y.get = d ? (o) => (u ^ 1 ? P : F)(o, l, u ^ 4 ? a : s.get) : (o) => o[i]), u > 2 && (y.set = d ? (o, x) => S(o, l, x, u ^ 4 ? a : s.set) : (o, x) => o[i] = x)), r = (0, n[v])(u ? u < 4 ? d ? a : s[k] : u > 4 ? void 0 : { get: s.get, set: s.set } : l, m), C._ = 1, u ^ 4 || r === void 0 ? h(r) && (u > 4 ? E.unshift(r) : u ? d ? a = r : s[k] = r : l = r) : typeof r != "object" || r === null ? p("Object expected") : (h(c = r.get) && (s.get = c), h(c = r.set) && (s.set = c), h(c = r.init) && E.unshift(c));
  return u || Y(t, l), s && T(l, i, s), d ? u ^ 4 ? a : s : l;
};
var _ = (t, e, i) => e.has(t) || p("Cannot " + i), K = (t, e) => Object(e) !== e ? p('Cannot use the "in" operator on this value') : t.has(e), P = (t, e, i) => (_(t, e, "read from private field"), i ? i.call(t) : e.get(t));
var S = (t, e, i, n) => (_(t, e, "write to private field"), n ? n.call(t, i) : e.set(t, i), i), F = (t, e, i) => (_(t, e, "access private method"), i);
import { UmbTextStyles as G } from "@umbraco-cms/backoffice/style";
import { UmbElementMixin as X } from "@umbraco-cms/backoffice/element-api";
import { LitElement as j, html as b, repeat as z, ifDefined as H, css as J, customElement as Q } from "@umbraco-cms/backoffice/external/lit";
import { UMB_WORKSPACE_MODAL as V, UMB_WORKSPACE_CONTEXT as Z } from "@umbraco-cms/backoffice/workspace";
import { A as ee } from "./sdk.gen-BwK88sZe.js";
import "@umbraco-cms/backoffice/modal";
import { UMB_TEMPLATE_ENTITY_TYPE as te } from "@umbraco-cms/backoffice/template";
import { UmbModalRouteRegistrationController as ie } from "@umbraco-cms/backoffice/router";
import { UMB_PARTIAL_VIEW_ENTITY_TYPE as ue } from "@umbraco-cms/backoffice/partial-view";
import { UmbServerFilePathUniqueSerializer as le } from "@umbraco-cms/backoffice/server-file-system";
import { tryExecute as ae } from "@umbraco-cms/backoffice/resources";
var I, $, ne;
I = [Q("auto-dictionaries-item-edit")];
class U extends (ne = X(j)) {
  _modalContext;
  _routeBuilder;
  #t;
  #i = [];
  #e;
  #u = !1;
  #l = new le();
  constructor() {
    super(), new ie(this, V).addAdditionalPath("general/:entityType").onSetup((e) => ({ data: { entityType: e.entityType, preset: {} } })).observeRouteBuilder((e) => {
      this._routeBuilder = e;
    }), this.consumeContext(Z, (e) => {
      this.#t = e, this.#a();
    });
  }
  async connectedCallback() {
    super.connectedCallback();
    const { data: e } = await ae(
      this,
      ee.getGetAllDictionaryItems()
    );
    e && (this.#i = e, this.requestUpdate());
  }
  #a() {
    this.#t && (this.observe(this.#t.currentItem, (e) => {
      this.#e = e, this.requestUpdate();
    }), this.observe(this.#t.isLoading, (e) => {
      this.#u = e, this.requestUpdate();
    }));
  }
  _renderStaticContent(e) {
    if (!e) return;
    const i = this.#i.map((n) => ({
      value: n.key ?? "",
      name: n.key ?? ""
    }));
    return b`<uui-table-row>
						<uui-table-cell style="--uui-table-cell-padding: 0; text-align: center;"><uui-checkbox /></uui-table-cell>
						<uui-table-cell>${e.staticContent}</uui-table-cell>
						<uui-table-cell>${e.used}</uui-table-cell>
						<uui-table-cell>
							<uui-select
								label="Parent"
								placeholder="None"
								.options=${i}>
						</uui-table-cell>
						<uui-table-cell>
						</uui-table-cell>
					
					</uui-table-row>`;
  }
  _renderDictionaries(e) {
    if (e)
      return b`<uui-table-row>
						<uui-table-cell>${e.key}</uui-table-cell>
						<uui-table-cell>${e.id}</uui-table-cell>
						<uui-table-cell>${e.used}</uui-table-cell>
						<uui-table-cell>

							${e.translated ? b`<uui-icon name="icon-check"></uui-icon>` : b`<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell></uui-select></uui-table-cell>
					

					</uui-table-row>`;
  }
  render() {
    return this.#u ? b`
				<umb-body-layout header-transparent>
					<uui-loader></uui-loader>
				</umb-body-layout>
			` : this.#e ? b`
			<umb-body-layout header-transparent>
				<div id="autoDictionaries-layout">
					<div id="autoDictionaries-main">
						<uui-box class="no-padding" headline="Dictionaries">
						
							<uui-table aria-label="Random Umbraco Words" aria-describedby="table-description">
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>
									<uui-table-column></uui-table-column>


									<uui-table-head>
										<uui-table-head-cell>Key</uui-table-head-cell>
										<uui-table-head-cell>Id</uui-table-head-cell>
										<uui-table-head-cell>Used in view</uui-table-head-cell>
										<uui-table-head-cell>Translated</uui-table-head-cell>
										<uui-table-head-cell></uui-table-head-cell>
									</uui-table-head>

									${z(this.#e.dictionaries ?? [], (e) => e.guid, (e) => this._renderDictionaries(e))}
								</uui-table>
						</uui-box>	
			
						<uui-box class="no-padding" headline="Static Content">
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

								${z(this.#e.staticContent ?? [], (e) => e.staticContent, (e) => this._renderStaticContent(e))}
							</uui-table>
						</uui-box>
					</div>
					
					<uui-box headline="General">

						<div class="general-item">
							<strong>Name</strong>
							<span>${this.#e.name}</span>
						</div>

						<div class="general-item">
							<strong>Alias</strong>
							<span>${this.#e.alias}</span>
						</div>

						<div class="general-item">
							<strong>Type</strong>
							<span>${this.#e.type}</span>
						</div>

					

						<div class="general-item">
							<strong>View</strong>
							<span>
								<uui-ref-node standalone name=${this.#e.name} detail="${this.#e.path}" 
									href=${H(
      this.#e.type === "Template" ? this._routeBuilder?.({ entityType: te }) + "edit/" + this.#e.key : this._routeBuilder?.({ entityType: ue }) + "edit/" + this.#l.toUnique("/" + this.#e.path)
    )}>
									<uui-icon slot="icon" name="icon-document-html" aria-hidden="true"></uui-icon>
								</uui-ref-node>
							</span>
						</div>

						<div class="general-item">
							<strong>Id</strong>
							<span>${this.#e.id}</span>
						</div>

						<div class="general-item">
							<strong>Id</strong>
							<span>${this.#e.key}</span>
						</div>

					</uui-box>
				</div>
			</umb-body-layout>
		` : b`
				<umb-body-layout header-transparent>
					<p>No item loaded</p>
				</umb-body-layout>
			`;
  }
  static styles = [
    G,
    J`
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
		`
  ];
}
$ = w(ne), U = R($, 0, "autoDictionariesItemViewElement", I, U), M($, 1, U);
export {
  U as autoDictionariesItemViewElement,
  U as default
};
//# sourceMappingURL=item.element-ToZaovRu.js.map
