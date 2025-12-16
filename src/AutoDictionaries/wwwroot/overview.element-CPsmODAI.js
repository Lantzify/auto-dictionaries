var j = Object.create;
var _ = Object.defineProperty;
var z = Object.getOwnPropertyDescriptor;
var M = (l, e) => (e = Symbol[l]) ? e : Symbol.for("Symbol." + l), p = (l) => {
  throw TypeError(l);
};
var B = (l, e, t) => e in l ? _(l, e, { enumerable: !0, configurable: !0, writable: !0, value: t }) : l[e] = t;
var U = (l, e) => _(l, "name", { value: e, configurable: !0 });
var q = (l) => [, , , j(l?.[M("metadata")] ?? null)], G = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], h = (l) => l !== void 0 && typeof l != "function" ? p("Function expected") : l, F = (l, e, t, n, i) => ({ kind: G[l], name: e, metadata: n, addInitializer: (a) => t._ ? p("Already initialized") : i.push(h(a || null)) }), H = (l, e) => B(e, M("metadata"), l[3]), L = (l, e, t, n) => {
  for (var i = 0, a = l[e >> 1], d = a && a.length; i < d; i++) e & 1 ? a[i].call(t) : n = a[i].call(t, n);
  return n;
}, P = (l, e, t, n, i, a) => {
  var d, o, C, m, y, u = e & 7, f = !!(e & 8), b = !!(e & 16), g = u > 3 ? l.length + 1 : u ? f ? 1 : 2 : 0, x = G[u + 5], T = u > 3 && (l[g - 1] = []), W = l[g] || (l[g] = []), r = u && (!b && !f && (i = i.prototype), u < 5 && (u > 3 || !b) && z(u < 4 ? i : { get [t]() {
    return D(this, a);
  }, set [t](c) {
    return E(this, a, c);
  } }, t));
  u ? b && u < 4 && U(a, (u > 2 ? "set " : u > 1 ? "get " : "") + t) : U(i, t);
  for (var $ = n.length - 1; $ >= 0; $--)
    m = F(u, t, C = {}, l[3], W), u && (m.static = f, m.private = b, y = m.access = { has: b ? (c) => I(i, c) : (c) => t in c }, u ^ 3 && (y.get = b ? (c) => (u ^ 1 ? D : J)(c, i, u ^ 4 ? a : r.get) : (c) => c[t]), u > 2 && (y.set = b ? (c, k) => E(c, i, k, u ^ 4 ? a : r.set) : (c, k) => c[t] = k)), o = (0, n[$])(u ? u < 4 ? b ? a : r[x] : u > 4 ? void 0 : { get: r.get, set: r.set } : i, m), C._ = 1, u ^ 4 || o === void 0 ? h(o) && (u > 4 ? T.unshift(o) : u ? b ? a = o : r[x] = o : i = o) : typeof o != "object" || o === null ? p("Object expected") : (h(d = o.get) && (r.get = d), h(d = o.set) && (r.set = d), h(d = o.init) && T.unshift(d));
  return u || H(l, i), r && _(i, t, r), b ? u ^ 4 ? a : r : i;
};
var S = (l, e, t) => e.has(l) || p("Cannot " + t), I = (l, e) => Object(e) !== e ? p('Cannot use the "in" operator on this value') : l.has(e), D = (l, e, t) => (S(l, e, "read from private field"), t ? t.call(l) : e.get(l));
var E = (l, e, t, n) => (S(l, e, "write to private field"), n ? n.call(l, t) : e.set(l, t), t), J = (l, e, t) => (S(l, e, "access private method"), t);
import { UmbElementMixin as K } from "@umbraco-cms/backoffice/element-api";
import { tryExecute as N } from "@umbraco-cms/backoffice/resources";
import { LitElement as Q, html as s, repeat as X, customElement as Y } from "@umbraco-cms/backoffice/external/lit";
import { A as Z } from "./sdk.gen-BwK88sZe.js";
var R, A, w;
R = [Y("auto-dictionaries-overview")];
class V extends (w = K(Q)) {
  _views = [];
  constructor() {
    super();
  }
  connectedCallback() {
    super.connectedCallback(), this.getAllViews();
  }
  async getAllViews() {
    const { data: e, error: t } = await N(this, Z.getGetAllViews());
    this._views = e ?? [], this.requestUpdate();
  }
  _openView(e) {
    if (!e?.id) return;
    const t = e.type === "Template" ? e.key.toString() : e.id.toString();
    window.history.pushState({}, "", `/umbraco/section/translation/workspace/auto-dictionaries-item/edit/${t}`);
  }
  _renderView(e) {
    if (e)
      return s`<uui-table-row @click=${() => this._openView(e)}>
						<uui-table-cell>${e.name}</uui-table-cell>
						<uui-table-cell>${e.type}</uui-table-cell>
						<uui-table-cell>${e.path}</uui-table-cell>
						<uui-table-cell>

							${e.staticContent?.length == 0 ? s`<uui-icon name="icon-check"></uui-icon>` : s`${e.staticContent?.length}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>

							${e.dictionaries && e.dictionaries.length > 0 ? e.dictionaries.length : ""}

							${e.staticContent?.length == 0 ? s`<uui-icon name="icon-check"></uui-icon>` : s`<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>${e.matchDictionaries > 0 ? e.matchDictionaries : ""}</uui-table-cell>
					</uui-table-row>`;
  }
  render() {
    return s`

		
			<umb-body-layout header-transparent>
				<umb-collection-toolbar slot="header">
					<umb-collection-filter-field>
						<uui-input label="Search" 
							placeholder="Type to search..."
							@change=/>
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
	
					${X(this._views, (e) => e.id, (e) => this._renderView(e))}
				</uui-table>
			
			</umb-body-layout>
		`;
  }
}
A = q(w), V = P(A, 0, "autoDictionariesOverviewViewElement", R, V), L(A, 1, V);
export {
  V as autoDictionariesOverviewViewElement,
  V as default
};
//# sourceMappingURL=overview.element-CPsmODAI.js.map
