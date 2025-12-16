var z = Object.create;
var C = Object.defineProperty;
var B = Object.getOwnPropertyDescriptor;
var L = (e, t) => (t = Symbol[e]) ? t : Symbol.for("Symbol." + e), b = (e) => {
  throw TypeError(e);
};
var G = (e, t, r) => t in e ? C(e, t, { enumerable: !0, configurable: !0, writable: !0, value: r }) : e[t] = r;
var D = (e, t) => C(e, "name", { value: t, configurable: !0 });
var M = (e) => [, , , z(e?.[L("metadata")] ?? null)], N = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], u = (e) => e !== void 0 && typeof e != "function" ? b("Function expected") : e, H = (e, t, r, c, i) => ({ kind: N[e], name: t, metadata: c, addInitializer: (s) => r._ ? b("Already initialized") : i.push(u(s || null)) }), I = (e, t) => G(t, L("metadata"), e[3]), S = (e, t, r, c) => {
  for (var i = 0, s = e[t >> 1], l = s && s.length; i < l; i++) t & 1 ? s[i].call(r) : c = s[i].call(r, c);
  return c;
}, $ = (e, t, r, c, i, s) => {
  var l, a, T, p, f, o = t & 7, h = !!(t & 8), d = !!(t & 16), w = o > 3 ? e.length + 1 : o ? h ? 1 : 2 : 0, U = N[o + 5], A = o > 3 && (e[w - 1] = []), q = e[w] || (e[w] = []), m = o && (!d && !h && (i = i.prototype), o < 5 && (o > 3 || !d) && B(o < 4 ? i : { get [r]() {
    return E(this, s);
  }, set [r](n) {
    return F(this, s, n);
  } }, r));
  o ? d && o < 4 && D(s, (o > 2 ? "set " : o > 1 ? "get " : "") + r) : D(i, r);
  for (var x = c.length - 1; x >= 0; x--)
    p = H(o, r, T = {}, e[3], q), o && (p.static = h, p.private = d, f = p.access = { has: d ? (n) => J(i, n) : (n) => r in n }, o ^ 3 && (f.get = d ? (n) => (o ^ 1 ? E : K)(n, i, o ^ 4 ? s : m.get) : (n) => n[r]), o > 2 && (f.set = d ? (n, k) => F(n, i, k, o ^ 4 ? s : m.set) : (n, k) => n[r] = k)), a = (0, c[x])(o ? o < 4 ? d ? s : m[U] : o > 4 ? void 0 : { get: m.get, set: m.set } : i, p), T._ = 1, o ^ 4 || a === void 0 ? u(a) && (o > 4 ? A.unshift(a) : o ? d ? s = a : m[U] = a : i = a) : typeof a != "object" || a === null ? b("Object expected") : (u(l = a.get) && (m.get = l), u(l = a.set) && (m.set = l), u(l = a.init) && A.unshift(l));
  return o || I(e, i), m && C(i, r, m), d ? o ^ 4 ? s : m : i;
};
var v = (e, t, r) => t.has(e) || b("Cannot " + r), J = (e, t) => Object(t) !== t ? b('Cannot use the "in" operator on this value') : e.has(t), E = (e, t, r) => (v(e, t, "read from private field"), r ? r.call(e) : t.get(e));
var F = (e, t, r, c) => (v(e, t, "write to private field"), c ? c.call(e, r) : t.set(e, r), r), K = (e, t, r) => (v(e, t, "access private method"), r);
import { UmbElementMixin as O } from "@umbraco-cms/backoffice/element-api";
import { LitElement as P, html as Q, css as V, customElement as X } from "@umbraco-cms/backoffice/external/lit";
import { autoDictionariesWorkspaceContext as Y } from "./workspace.context-B-0nVoUj.js";
import { UmbTextStyles as Z } from "@umbraco-cms/backoffice/style";
var j, g, _;
j = [X("auto-dictionaries-root")];
class y extends (_ = O(P)) {
  #e;
  #t = [
    {
      path: "edit/:id",
      component: () => import("./overview.element-CPsmODAI.js")
    },
    // Default route
    {
      path: "edit/:id",
      redirectTo: "overview"
    }
  ];
  constructor() {
    super(), this.#e = new Y(this);
  }
  connectedCallback() {
    super.connectedCallback();
  }
  render() {
    return Q`
			<umb-workspace-editor headline="Auto Dictionaries" alias="autoDictionaries.workspace.root" .enforceNoFooter=${!0}>
			</umb-workspace-editor>
		`;
  }
  static styles = [
    Z,
    V`
			umb-workspace-editor > div.header {
				display: flex;
				align-items: center;
				align-content: center;
			}
		`
  ];
}
g = M(_), y = $(g, 0, "autoDictionariesWorkspaceRootElement", j, y), S(g, 1, y);
export {
  y as autoDictionariesWorkspaceRootElement,
  y as default
};
//# sourceMappingURL=workspace.element-CJs3fs4h.js.map
