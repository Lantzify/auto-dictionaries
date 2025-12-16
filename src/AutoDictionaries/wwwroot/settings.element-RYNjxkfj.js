var I = Object.create;
var E = Object.defineProperty;
var J = Object.getOwnPropertyDescriptor;
var v = (o, t) => (t = Symbol[o]) ? t : Symbol.for("Symbol." + o), x = (o) => {
  throw TypeError(o);
};
var K = (o, t, e) => t in o ? E(o, t, { enumerable: !0, configurable: !0, writable: !0, value: e }) : o[t] = e;
var j = (o, t) => E(o, "name", { value: t, configurable: !0 });
var A = (o) => [, , , I(o?.[v("metadata")] ?? null)], B = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], a = (o) => o !== void 0 && typeof o != "function" ? x("Function expected") : o, N = (o, t, e, b, u) => ({ kind: B[o], name: t, metadata: b, addInitializer: (m) => e._ ? x("Already initialized") : u.push(a(m || null)) }), O = (o, t) => K(t, v("metadata"), o[3]), C = (o, t, e, b) => {
  for (var u = 0, m = o[t >> 1], c = m && m.length; u < c; u++) t & 1 ? m[u].call(e) : b = m[u].call(e, b);
  return b;
}, F = (o, t, e, b, u, m) => {
  var c, s, _, n, p, r = t & 7, y = !!(t & 8), i = !!(t & 16), f = r > 3 ? o.length + 1 : r ? y ? 1 : 2 : 0, $ = B[r + 5], g = r > 3 && (o[f - 1] = []), H = o[f] || (o[f] = []), l = r && (!i && !y && (u = u.prototype), r < 5 && (r > 3 || !i) && J(r < 4 ? u : { get [e]() {
    return k(this, m);
  }, set [e](d) {
    return q(this, m, d);
  } }, e));
  r ? i && r < 4 && j(m, (r > 2 ? "set " : r > 1 ? "get " : "") + e) : j(u, e);
  for (var h = b.length - 1; h >= 0; h--)
    n = N(r, e, _ = {}, o[3], H), r && (n.static = y, n.private = i, p = n.access = { has: i ? (d) => P(u, d) : (d) => e in d }, r ^ 3 && (p.get = i ? (d) => (r ^ 1 ? k : Q)(d, u, r ^ 4 ? m : l.get) : (d) => d[e]), r > 2 && (p.set = i ? (d, z) => q(d, u, z, r ^ 4 ? m : l.set) : (d, z) => d[e] = z)), s = (0, b[h])(r ? r < 4 ? i ? m : l[$] : r > 4 ? void 0 : { get: l.get, set: l.set } : u, n), _._ = 1, r ^ 4 || s === void 0 ? a(s) && (r > 4 ? g.unshift(s) : r ? i ? m = s : l[$] = s : u = s) : typeof s != "object" || s === null ? x("Object expected") : (a(c = s.get) && (l.get = c), a(c = s.set) && (l.set = c), a(c = s.init) && g.unshift(c));
  return r || O(o, u), l && E(u, e, l), i ? r ^ 4 ? m : l : u;
};
var L = (o, t, e) => t.has(o) || x("Cannot " + e), P = (o, t) => Object(t) !== t ? x('Cannot use the "in" operator on this value') : o.has(t), k = (o, t, e) => (L(o, t, "read from private field"), e ? e.call(o) : t.get(o));
var q = (o, t, e, b) => (L(o, t, "write to private field"), b ? b.call(o, e) : t.set(o, e), e), Q = (o, t, e) => (L(o, t, "access private method"), e);
import { UmbElementMixin as R } from "@umbraco-cms/backoffice/element-api";
import { LitElement as T, html as W, customElement as X } from "@umbraco-cms/backoffice/external/lit";
var G, U, Y;
G = [X("auto-dictionaries-settings")];
class M extends (Y = R(T)) {
  constructor() {
    super();
  }
  render() {
    return W`
		<umb-body-layout>
	      <uui-box headline=${this.localize.term("sections_settings")}>
		  <umb-code-editor>
		  </umb-code-editor>
            </uui-box>
			</umb-body-layout>
		`;
  }
}
U = A(Y), M = F(U, 0, "autoDictionariesSettingsViewElement", G, M), C(U, 1, M);
export {
  M as autoDictionariesSettingsViewElement,
  M as default
};
//# sourceMappingURL=settings.element-RYNjxkfj.js.map
