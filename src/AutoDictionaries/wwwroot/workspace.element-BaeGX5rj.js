var z = Object.create;
var y = Object.defineProperty;
var B = Object.getOwnPropertyDescriptor;
var M = (e, t) => (t = Symbol[e]) ? t : Symbol.for("Symbol." + e), f = (e) => {
  throw TypeError(e);
};
var G = (e, t, r) => t in e ? y(e, t, { enumerable: !0, configurable: !0, writable: !0, value: r }) : e[t] = r;
var E = (e, t) => y(e, "name", { value: t, configurable: !0 });
var N = (e) => [, , , z(e?.[M("metadata")] ?? null)], S = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], u = (e) => e !== void 0 && typeof e != "function" ? f("Function expected") : e, H = (e, t, r, m, i) => ({ kind: S[e], name: t, metadata: m, addInitializer: (s) => r._ ? f("Already initialized") : i.push(u(s || null)) }), I = (e, t) => G(t, M("metadata"), e[3]), T = (e, t, r, m) => {
  for (var i = 0, s = e[t >> 1], d = s && s.length; i < d; i++) t & 1 ? s[i].call(r) : m = s[i].call(r, m);
  return m;
}, $ = (e, t, r, m, i, s) => {
  var d, a, v, p, x, o = t & 7, w = !!(t & 8), l = !!(t & 16), b = o > 3 ? e.length + 1 : o ? w ? 1 : 2 : 0, A = S[o + 5], D = o > 3 && (e[b - 1] = []), q = e[b] || (e[b] = []), c = o && (!l && !w && (i = i.prototype), o < 5 && (o > 3 || !l) && B(o < 4 ? i : { get [r]() {
    return F(this, s);
  }, set [r](n) {
    return L(this, s, n);
  } }, r));
  o ? l && o < 4 && E(s, (o > 2 ? "set " : o > 1 ? "get " : "") + r) : E(i, r);
  for (var h = m.length - 1; h >= 0; h--)
    p = H(o, r, v = {}, e[3], q), o && (p.static = w, p.private = l, x = p.access = { has: l ? (n) => J(i, n) : (n) => r in n }, o ^ 3 && (x.get = l ? (n) => (o ^ 1 ? F : K)(n, i, o ^ 4 ? s : c.get) : (n) => n[r]), o > 2 && (x.set = l ? (n, k) => L(n, i, k, o ^ 4 ? s : c.set) : (n, k) => n[r] = k)), a = (0, m[h])(o ? o < 4 ? l ? s : c[A] : o > 4 ? void 0 : { get: c.get, set: c.set } : i, p), v._ = 1, o ^ 4 || a === void 0 ? u(a) && (o > 4 ? D.unshift(a) : o ? l ? s = a : c[A] = a : i = a) : typeof a != "object" || a === null ? f("Object expected") : (u(d = a.get) && (c.get = d), u(d = a.set) && (c.set = d), u(d = a.init) && D.unshift(d));
  return o || I(e, i), c && y(i, r, c), l ? o ^ 4 ? s : c : i;
};
var g = (e, t, r) => t.has(e) || f("Cannot " + r), J = (e, t) => Object(t) !== t ? f('Cannot use the "in" operator on this value') : e.has(t), F = (e, t, r) => (g(e, t, "read from private field"), r ? r.call(e) : t.get(e));
var L = (e, t, r, m) => (g(e, t, "write to private field"), m ? m.call(e, r) : t.set(e, r), r), K = (e, t, r) => (g(e, t, "access private method"), r);
import { UmbElementMixin as O } from "@umbraco-cms/backoffice/element-api";
import { LitElement as P, html as Q, css as V, customElement as X } from "@umbraco-cms/backoffice/external/lit";
import { autoDictionariesWorkspaceContext as Y } from "./workspace.context-Do5BNhnz.js";
import { UmbTextStyles as Z } from "@umbraco-cms/backoffice/style";
var j, U, _;
j = [X("auto-dictionaries-root")];
class C extends (_ = O(P)) {
  #e;
  constructor() {
    super(), this.#e = new Y(this);
  }
  render() {
    return Q`
			<umb-workspace-editor  headline="Auto Dictionaries" alias="autoDictionaries.workspace.root" .enforceNoFooter=${!0}>
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
U = N(_), C = $(U, 0, "autoDictionariesWorkspaceRootElement", j, C), T(U, 1, C);
export {
  C as autoDictionariesWorkspaceRootElement,
  C as default
};
//# sourceMappingURL=workspace.element-BaeGX5rj.js.map
