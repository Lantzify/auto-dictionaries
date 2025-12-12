var I = Object.create;
var E = Object.defineProperty;
var J = Object.getOwnPropertyDescriptor;
var v = (t, o) => (o = Symbol[t]) ? o : Symbol.for("Symbol." + t), x = (t) => {
  throw TypeError(t);
};
var K = (t, o, e) => o in t ? E(t, o, { enumerable: !0, configurable: !0, writable: !0, value: e }) : t[o] = e;
var j = (t, o) => E(t, "name", { value: o, configurable: !0 });
var A = (t) => [, , , I(t?.[v("metadata")] ?? null)], B = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], c = (t) => t !== void 0 && typeof t != "function" ? x("Function expected") : t, N = (t, o, e, b, u) => ({ kind: B[t], name: o, metadata: b, addInitializer: (m) => e._ ? x("Already initialized") : u.push(c(m || null)) }), O = (t, o) => K(o, v("metadata"), t[3]), C = (t, o, e, b) => {
  for (var u = 0, m = t[o >> 1], i = m && m.length; u < i; u++) o & 1 ? m[u].call(e) : b = m[u].call(e, b);
  return b;
}, F = (t, o, e, b, u, m) => {
  var i, l, _, a, p, r = o & 7, y = !!(o & 8), n = !!(o & 16), f = r > 3 ? t.length + 1 : r ? y ? 1 : 2 : 0, $ = B[r + 5], g = r > 3 && (t[f - 1] = []), H = t[f] || (t[f] = []), d = r && (!n && !y && (u = u.prototype), r < 5 && (r > 3 || !n) && J(r < 4 ? u : { get [e]() {
    return k(this, m);
  }, set [e](s) {
    return q(this, m, s);
  } }, e));
  r ? n && r < 4 && j(m, (r > 2 ? "set " : r > 1 ? "get " : "") + e) : j(u, e);
  for (var h = b.length - 1; h >= 0; h--)
    a = N(r, e, _ = {}, t[3], H), r && (a.static = y, a.private = n, p = a.access = { has: n ? (s) => P(u, s) : (s) => e in s }, r ^ 3 && (p.get = n ? (s) => (r ^ 1 ? k : Q)(s, u, r ^ 4 ? m : d.get) : (s) => s[e]), r > 2 && (p.set = n ? (s, z) => q(s, u, z, r ^ 4 ? m : d.set) : (s, z) => s[e] = z)), l = (0, b[h])(r ? r < 4 ? n ? m : d[$] : r > 4 ? void 0 : { get: d.get, set: d.set } : u, a), _._ = 1, r ^ 4 || l === void 0 ? c(l) && (r > 4 ? g.unshift(l) : r ? n ? m = l : d[$] = l : u = l) : typeof l != "object" || l === null ? x("Object expected") : (c(i = l.get) && (d.get = i), c(i = l.set) && (d.set = i), c(i = l.init) && g.unshift(i));
  return r || O(t, u), d && E(u, e, d), n ? r ^ 4 ? m : d : u;
};
var L = (t, o, e) => o.has(t) || x("Cannot " + e), P = (t, o) => Object(o) !== o ? x('Cannot use the "in" operator on this value') : t.has(o), k = (t, o, e) => (L(t, o, "read from private field"), e ? e.call(t) : o.get(t));
var q = (t, o, e, b) => (L(t, o, "write to private field"), b ? b.call(t, e) : o.set(t, e), e), Q = (t, o, e) => (L(t, o, "access private method"), e);
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
//# sourceMappingURL=settings.element-Bxr6OyKv.js.map
