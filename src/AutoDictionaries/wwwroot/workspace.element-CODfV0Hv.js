var j = Object.create;
var v = Object.defineProperty;
var A = Object.getOwnPropertyDescriptor;
var F = (t, o) => (o = Symbol[t]) ? o : Symbol.for("Symbol." + t), h = (t) => {
  throw TypeError(t);
};
var G = (t, o, e) => o in t ? v(t, o, { enumerable: !0, configurable: !0, writable: !0, value: e }) : t[o] = e;
var q = (t, o) => v(t, "name", { value: o, configurable: !0 });
var L = (t) => [, , , j(t?.[F("metadata")] ?? null)], M = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"], l = (t) => t !== void 0 && typeof t != "function" ? h("Function expected") : t, H = (t, o, e, a, r) => ({ kind: M[t], name: o, metadata: a, addInitializer: (s) => e._ ? h("Already initialized") : r.push(l(s || null)) }), J = (t, o) => G(o, F("metadata"), t[3]), N = (t, o, e, a) => {
  for (var r = 0, s = t[o >> 1], m = s && s.length; r < m; r++) o & 1 ? s[r].call(e) : a = s[r].call(e, a);
  return a;
}, S = (t, o, e, a, r, s) => {
  var m, c, B, p, b, i = o & 7, k = !!(o & 8), d = !!(o & 16), w = i > 3 ? t.length + 1 : i ? k ? 1 : 2 : 0, C = M[i + 5], U = i > 3 && (t[w - 1] = []), _ = t[w] || (t[w] = []), n = i && (!d && !k && (r = r.prototype), i < 5 && (i > 3 || !d) && A(i < 4 ? r : { get [e]() {
    return z(this, s);
  }, set [e](u) {
    return E(this, s, u);
  } }, e));
  i ? d && i < 4 && q(s, (i > 2 ? "set " : i > 1 ? "get " : "") + e) : q(r, e);
  for (var f = a.length - 1; f >= 0; f--)
    p = H(i, e, B = {}, t[3], _), i && (p.static = k, p.private = d, b = p.access = { has: d ? (u) => K(r, u) : (u) => e in u }, i ^ 3 && (b.get = d ? (u) => (i ^ 1 ? z : O)(u, r, i ^ 4 ? s : n.get) : (u) => u[e]), i > 2 && (b.set = d ? (u, x) => E(u, r, x, i ^ 4 ? s : n.set) : (u, x) => u[e] = x)), c = (0, a[f])(i ? i < 4 ? d ? s : n[C] : i > 4 ? void 0 : { get: n.get, set: n.set } : r, p), B._ = 1, i ^ 4 || c === void 0 ? l(c) && (i > 4 ? U.unshift(c) : i ? d ? s = c : n[C] = c : r = c) : typeof c != "object" || c === null ? h("Object expected") : (l(m = c.get) && (n.get = m), l(m = c.set) && (n.set = m), l(m = c.init) && U.unshift(m));
  return i || J(t, r), n && v(r, e, n), d ? i ^ 4 ? s : n : r;
};
var $ = (t, o, e) => o.has(t) || h("Cannot " + e), K = (t, o) => Object(o) !== o ? h('Cannot use the "in" operator on this value') : t.has(o), z = (t, o, e) => ($(t, o, "read from private field"), e ? e.call(t) : o.get(t));
var E = (t, o, e, a) => ($(t, o, "write to private field"), a ? a.call(t, e) : o.set(t, e), e), O = (t, o, e) => ($(t, o, "access private method"), e);
import { UmbElementMixin as P } from "@umbraco-cms/backoffice/element-api";
import { LitElement as Q, html as R, css as V, customElement as X } from "@umbraco-cms/backoffice/external/lit";
import { AutoDictionariesItemWorkspaceContext as Y } from "./workspace.context-Cm7KB7bD.js";
var T, y, Z;
T = [X("auto-dictionaries-item-workspace")];
class g extends (Z = P(Q)) {
  #t;
  #o = [
    {
      path: "edit/:id",
      component: () => import("./item.element-ToZaovRu.js"),
      setup: (o, e) => {
        const a = e.match.params.id;
        a && this.#t.setUnique(a);
      }
    },
    {
      path: "",
      redirectTo: "edit/"
    }
  ];
  constructor() {
    super(), this.#t = new Y(this);
  }
  #e() {
    window.history.pushState({}, "", "/umbraco/section/translation/workspace/auto-dictionaries-root/");
  }
  render() {
    return R`
            <umb-workspace-editor .enforceNoFooter=${!0}>
                <uui-button id="back-button" slot="header" compact @click=${this.#e} label="Back">
                    <uui-icon name="icon-arrow-left"></uui-icon>
                </uui-button>
                <div slot="header">Auto dictionaries</div>
                
                <umb-router-slot id="router-slot" .routes=${this.#o}></umb-router-slot>
            </umb-workspace-editor>
        `;
  }
  static styles = [
    V`
            #back-button {
                order: -1;
                margin-right: var(--uui-size-space-4);
            }
        `
  ];
}
y = L(Z), g = S(y, 0, "AutoDictionariesItemWorkspaceElement", T, g), N(y, 1, g);
export {
  g as AutoDictionariesItemWorkspaceElement,
  g as default
};
//# sourceMappingURL=workspace.element-CODfV0Hv.js.map
