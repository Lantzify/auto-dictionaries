var R = Object.create;
var C = Object.defineProperty;
var q = Object.getOwnPropertyDescriptor;
var S = (t, e) => (e = Symbol[t]) ? e : Symbol.for("Symbol." + t), p = (t) => {
  throw TypeError(t);
};
var v = (t, e, i) => e in t ? C(t, e, { enumerable: !0, configurable: !0, writable: !0, value: i }) : t[e] = i;
var E = (t, e) => C(t, "name", { value: e, configurable: !0 });
var U = (t) => [, , , R(t?.[S("metadata")] ?? null)], j = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], h = (t) => t !== void 0 && typeof t != "function" ? p("Function expected") : t, K = (t, e, i, o, a) => ({ kind: j[t], name: e, metadata: o, addInitializer: (u) => i._ ? p("Already initialized") : a.push(h(u || null)) }), L = (t, e) => v(e, S("metadata"), t[3]), B = (t, e, i, o) => {
  for (var a = 0, u = t[e >> 1], b = u && u.length; a < b; a++) e & 1 ? u[a].call(i) : o = u[a].call(i, o);
  return o;
}, P = (t, e, i, o, a, u) => {
  var b, n, I, g, y, l = e & 7, w = !!(e & 8), s = !!(e & 16), A = l > 3 ? t.length + 1 : l ? w ? 1 : 2 : 0, _ = j[l + 5], D = l > 3 && (t[A - 1] = []), N = t[A] || (t[A] = []), d = l && (!s && !w && (a = a.prototype), l < 5 && (l > 3 || !s) && q(l < 4 ? a : { get [i]() {
    return V(this, u);
  }, set [i](r) {
    return k(this, u, r);
  } }, i));
  l ? s && l < 4 && E(u, (l > 2 ? "set " : l > 1 ? "get " : "") + i) : E(a, i);
  for (var T = o.length - 1; T >= 0; T--)
    g = K(l, i, I = {}, t[3], N), l && (g.static = w, g.private = s, y = g.access = { has: s ? (r) => W(a, r) : (r) => i in r }, l ^ 3 && (y.get = s ? (r) => (l ^ 1 ? V : X)(r, a, l ^ 4 ? u : d.get) : (r) => r[i]), l > 2 && (y.set = s ? (r, f) => k(r, a, f, l ^ 4 ? u : d.set) : (r, f) => r[i] = f)), n = (0, o[T])(l ? l < 4 ? s ? u : d[_] : l > 4 ? void 0 : { get: d.get, set: d.set } : a, g), I._ = 1, l ^ 4 || n === void 0 ? h(n) && (l > 4 ? D.unshift(n) : l ? s ? u = n : d[_] = n : a = n) : typeof n != "object" || n === null ? p("Object expected") : (h(b = n.get) && (d.get = b), h(b = n.set) && (d.set = b), h(b = n.init) && D.unshift(b));
  return l || L(t, a), d && C(a, i, d), s ? l ^ 4 ? u : d : a;
};
var G = (t, e, i) => e.has(t) || p("Cannot " + i), W = (t, e) => Object(e) !== e ? p('Cannot use the "in" operator on this value') : t.has(e), V = (t, e, i) => (G(t, e, "read from private field"), i ? i.call(t) : e.get(t));
var k = (t, e, i, o) => (G(t, e, "write to private field"), o ? o.call(t, i) : e.set(t, i), i), X = (t, e, i) => (G(t, e, "access private method"), i);
import { UmbElementMixin as z } from "@umbraco-cms/backoffice/element-api";
import { tryExecute as F } from "@umbraco-cms/backoffice/resources";
import { LitElement as H, html as m, repeat as J, customElement as O } from "@umbraco-cms/backoffice/external/lit";
import { UMB_ROUTE_CONTEXT as Q } from "@umbraco-cms/backoffice/router";
import { c } from "./index-DuXokHgq.js";
class Y {
  static postAddExistingDictionaryItem(e) {
    return (e?.client ?? c).post({
      url: "/add-existing-dictionary-item",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e?.headers
      }
    });
  }
  static postAddNewDictionaryItem(e) {
    return (e?.client ?? c).post({
      url: "/add-new-dictionary-item",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e?.headers
      }
    });
  }
  static getGetAllDictionaryItems(e) {
    return (e?.client ?? c).get({
      url: "/get-all-dictionary-items",
      ...e
    });
  }
  static getGetAllViews(e) {
    return (e?.client ?? c).get({
      url: "/get-all-views",
      ...e
    });
  }
  static getGetApiEndpoint(e) {
    return (e?.client ?? c).get({
      url: "/get-api-endpoint",
      ...e
    });
  }
  static getGetApiKey(e) {
    return (e?.client ?? c).get({
      url: "/get-api-key",
      ...e
    });
  }
  static getGetApiRegion(e) {
    return (e?.client ?? c).get({
      url: "/get-api-region",
      ...e
    });
  }
  static getGetPreviewById(e) {
    return (e.client ?? c).get({
      url: "/get-preview/{id}",
      ...e
    });
  }
  static getGetTranslateSetting(e) {
    return (e?.client ?? c).get({
      url: "/get-translate-setting",
      ...e
    });
  }
  static getGetTranslatorSetting(e) {
    return (e?.client ?? c).get({
      url: "/get-translator-setting",
      ...e
    });
  }
  static getGetViewById(e) {
    return (e.client ?? c).get({
      url: "/get-view/{id}",
      ...e
    });
  }
  static postPreviewAddExistingDictionaryItem(e) {
    return (e?.client ?? c).post({
      url: "/preview-add-existing-dictionary-item",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e?.headers
      }
    });
  }
  static postPreviewAddNewDictionaryItem(e) {
    return (e?.client ?? c).post({
      url: "/preview-add-new-dictionary-item",
      ...e,
      headers: {
        "Content-Type": "application/json",
        ...e?.headers
      }
    });
  }
  static getTranslateDictionaryItemById(e) {
    return (e.client ?? c).get({
      url: "/translate-dictionary-item/{id}",
      ...e
    });
  }
}
var M, x, Z;
M = [O("auto-dictionaries-overview")];
class $ extends (Z = z(H)) {
  _views = [];
  _router;
  constructor() {
    super(), this.consumeContext(Q, (e) => this._router = e);
  }
  connectedCallback() {
    super.connectedCallback(), this.getAllViews();
  }
  async getAllViews() {
    const { data: e, error: i } = await F(this, Y.getGetAllViews());
    console.log(e), this._views = e ?? [], this.requestUpdate();
  }
  _renderView(e) {
    if (e)
      return m`<uui-table-row>
						<uui-table-cell>${e.name}</uui-table-cell>
						<uui-table-cell>${e.type}</uui-table-cell>
						<uui-table-cell>${e.path}</uui-table-cell>
						<uui-table-cell>
							${e.staticContent}
							${e.staticContent ? m`<uui-icon name="icon-check"></uui-icon>` : m`${e.staticContent}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>

							${e.dictionaries ? m`<uui-icon name="icon-check"></uui-icon>` : m`${e.dictionaries}<uui-icon name="icon-alert"></uui-icon>`}
						</uui-table-cell>
						<uui-table-cell>${e.matchDictionaries > 0 ? e.matchDictionaries : ""}</uui-table-cell>
					</uui-table-row>`;
  }
  render() {
    return m`

		
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
	
					${J(this._views, (e) => e.id, (e) => this._renderView(e))}
				</uui-table>
			


			</umb-body-layout>
		`;
  }
}
x = U(Z), $ = P(x, 0, "autoDictionariesOverviewViewElement", M, $), B(x, 1, $);
export {
  $ as autoDictionariesOverviewViewElement,
  $ as default
};
//# sourceMappingURL=overview.element-BH2rorHA.js.map
