import { UMB_AUTH_CONTEXT as X } from "@umbraco-cms/backoffice/auth";
const K = {
  bodySerializer: (r) => JSON.stringify(
    r,
    (e, t) => typeof t == "bigint" ? t.toString() : t
  )
}, Y = ({
  onRequest: r,
  onSseError: e,
  onSseEvent: t,
  responseTransformer: n,
  responseValidator: i,
  sseDefaultRetryDelay: l,
  sseMaxRetryAttempts: o,
  sseMaxRetryDelay: s,
  sseSleepFn: c,
  url: d,
  ...a
}) => {
  let f;
  const j = c ?? ((u) => new Promise((h) => setTimeout(h, u)));
  return { stream: async function* () {
    let u = l ?? 3e3, h = 0;
    const k = a.signal ?? new AbortController().signal;
    for (; !k.aborted; ) {
      h++;
      const x = a.headers instanceof Headers ? a.headers : new Headers(a.headers);
      f !== void 0 && x.set("Last-Event-ID", f);
      try {
        const A = {
          redirect: "follow",
          ...a,
          body: a.serializedBody,
          headers: x,
          signal: k
        };
        let b = new Request(d, A);
        r && (b = await r(d, A));
        const m = await (a.fetch ?? globalThis.fetch)(b);
        if (!m.ok)
          throw new Error(
            `SSE failed: ${m.status} ${m.statusText}`
          );
        if (!m.body) throw new Error("No body in SSE response");
        const w = m.body.pipeThrough(new TextDecoderStream()).getReader();
        let T = "";
        const v = () => {
          try {
            w.cancel();
          } catch {
          }
        };
        k.addEventListener("abort", v);
        try {
          for (; ; ) {
            const { done: F, value: M } = await w.read();
            if (F) break;
            T += M;
            const O = T.split(`

`);
            T = O.pop() ?? "";
            for (const G of O) {
              const Q = G.split(`
`), C = [];
              let q;
              for (const y of Q)
                if (y.startsWith("data:"))
                  C.push(y.replace(/^data:\s*/, ""));
                else if (y.startsWith("event:"))
                  q = y.replace(/^event:\s*/, "");
                else if (y.startsWith("id:"))
                  f = y.replace(/^id:\s*/, "");
                else if (y.startsWith("retry:")) {
                  const U = Number.parseInt(
                    y.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(U) || (u = U);
                }
              let S, I = !1;
              if (C.length) {
                const y = C.join(`
`);
                try {
                  S = JSON.parse(y), I = !0;
                } catch {
                  S = y;
                }
              }
              I && (i && await i(S), n && (S = await n(S))), t?.({
                data: S,
                event: q,
                id: f,
                retry: u
              }), C.length && (yield S);
            }
          }
        } finally {
          k.removeEventListener("abort", v), w.releaseLock();
        }
        break;
      } catch (A) {
        if (e?.(A), o !== void 0 && h >= o)
          break;
        const b = Math.min(
          u * 2 ** (h - 1),
          s ?? 3e4
        );
        await j(b);
      }
    }
  }() };
}, Z = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, ee = (r) => {
  switch (r) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, te = (r) => {
  switch (r) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, P = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: n,
  value: i
}) => {
  if (!e) {
    const s = (r ? i : i.map((c) => encodeURIComponent(c))).join(ee(n));
    switch (n) {
      case "label":
        return `.${s}`;
      case "matrix":
        return `;${t}=${s}`;
      case "simple":
        return s;
      default:
        return `${t}=${s}`;
    }
  }
  const l = Z(n), o = i.map((s) => n === "label" || n === "simple" ? r ? s : encodeURIComponent(s) : z({
    allowReserved: r,
    name: t,
    value: s
  })).join(l);
  return n === "label" || n === "matrix" ? l + o : o;
}, z = ({
  allowReserved: r,
  name: e,
  value: t
}) => {
  if (t == null)
    return "";
  if (typeof t == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${e}=${r ? t : encodeURIComponent(t)}`;
}, H = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: n,
  value: i,
  valueOnly: l
}) => {
  if (i instanceof Date)
    return l ? i.toISOString() : `${t}=${i.toISOString()}`;
  if (n !== "deepObject" && !e) {
    let c = [];
    Object.entries(i).forEach(([a, f]) => {
      c = [
        ...c,
        a,
        r ? f : encodeURIComponent(f)
      ];
    });
    const d = c.join(",");
    switch (n) {
      case "form":
        return `${t}=${d}`;
      case "label":
        return `.${d}`;
      case "matrix":
        return `;${t}=${d}`;
      default:
        return d;
    }
  }
  const o = te(n), s = Object.entries(i).map(
    ([c, d]) => z({
      allowReserved: r,
      name: n === "deepObject" ? `${t}[${c}]` : c,
      value: d
    })
  ).join(o);
  return n === "label" || n === "matrix" ? o + s : s;
}, re = /\{[^{}]+\}/g, ae = ({ path: r, url: e }) => {
  let t = e;
  const n = e.match(re);
  if (n)
    for (const i of n) {
      let l = !1, o = i.substring(1, i.length - 1), s = "simple";
      o.endsWith("*") && (l = !0, o = o.substring(0, o.length - 1)), o.startsWith(".") ? (o = o.substring(1), s = "label") : o.startsWith(";") && (o = o.substring(1), s = "matrix");
      const c = r[o];
      if (c == null)
        continue;
      if (Array.isArray(c)) {
        t = t.replace(
          i,
          P({ explode: l, name: o, style: s, value: c })
        );
        continue;
      }
      if (typeof c == "object") {
        t = t.replace(
          i,
          H({
            explode: l,
            name: o,
            style: s,
            value: c,
            valueOnly: !0
          })
        );
        continue;
      }
      if (s === "matrix") {
        t = t.replace(
          i,
          `;${z({
            name: o,
            value: c
          })}`
        );
        continue;
      }
      const d = encodeURIComponent(
        s === "label" ? `.${c}` : c
      );
      t = t.replace(i, d);
    }
  return t;
}, ie = ({
  baseUrl: r,
  path: e,
  query: t,
  querySerializer: n,
  url: i
}) => {
  const l = i.startsWith("/") ? i : `/${i}`;
  let o = (r ?? "") + l;
  e && (o = ae({ path: e, url: o }));
  let s = t ? n(t) : "";
  return s.startsWith("?") && (s = s.substring(1)), s && (o += `?${s}`), o;
};
function se(r) {
  const e = r.body !== void 0;
  if (e && r.bodySerializer)
    return "serializedBody" in r ? r.serializedBody !== void 0 && r.serializedBody !== "" ? r.serializedBody : null : r.body !== "" ? r.body : null;
  if (e)
    return r.body;
}
const ne = async (r, e) => {
  const t = typeof e == "function" ? await e(r) : e;
  if (t)
    return r.scheme === "bearer" ? `Bearer ${t}` : r.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, V = ({
  allowReserved: r,
  array: e,
  object: t
} = {}) => (i) => {
  const l = [];
  if (i && typeof i == "object")
    for (const o in i) {
      const s = i[o];
      if (s != null)
        if (Array.isArray(s)) {
          const c = P({
            allowReserved: r,
            explode: !0,
            name: o,
            style: "form",
            value: s,
            ...e
          });
          c && l.push(c);
        } else if (typeof s == "object") {
          const c = H({
            allowReserved: r,
            explode: !0,
            name: o,
            style: "deepObject",
            value: s,
            ...t
          });
          c && l.push(c);
        } else {
          const c = z({
            allowReserved: r,
            name: o,
            value: s
          });
          c && l.push(c);
        }
    }
  return l.join("&");
}, oe = (r) => {
  if (!r)
    return "stream";
  const e = r.split(";")[0]?.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json"))
      return "json";
    if (e === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (t) => e.startsWith(t)
    ))
      return "blob";
    if (e.startsWith("text/"))
      return "text";
  }
}, ce = (r, e) => e ? !!(r.headers.has(e) || r.query?.[e] || r.headers.get("Cookie")?.includes(`${e}=`)) : !1, le = async ({
  security: r,
  ...e
}) => {
  for (const t of r) {
    if (ce(e, t.name))
      continue;
    const n = await ne(t, e.auth);
    if (!n)
      continue;
    const i = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        e.query || (e.query = {}), e.query[i] = n;
        break;
      case "cookie":
        e.headers.append("Cookie", `${i}=${n}`);
        break;
      case "header":
      default:
        e.headers.set(i, n);
        break;
    }
  }
}, B = (r) => ie({
  baseUrl: r.baseUrl,
  path: r.path,
  query: r.query,
  querySerializer: typeof r.querySerializer == "function" ? r.querySerializer : V(r.querySerializer),
  url: r.url
}), W = (r, e) => {
  const t = { ...r, ...e };
  return t.baseUrl?.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = R(r.headers, e.headers), t;
}, ue = (r) => {
  const e = [];
  return r.forEach((t, n) => {
    e.push([n, t]);
  }), e;
}, R = (...r) => {
  const e = new Headers();
  for (const t of r) {
    if (!t)
      continue;
    const n = t instanceof Headers ? ue(t) : Object.entries(t);
    for (const [i, l] of n)
      if (l === null)
        e.delete(i);
      else if (Array.isArray(l))
        for (const o of l)
          e.append(i, o);
      else l !== void 0 && e.set(
        i,
        typeof l == "object" ? JSON.stringify(l) : l
      );
  }
  return e;
};
class E {
  fns = [];
  clear() {
    this.fns = [];
  }
  eject(e) {
    const t = this.getInterceptorIndex(e);
    this.fns[t] && (this.fns[t] = null);
  }
  exists(e) {
    const t = this.getInterceptorIndex(e);
    return !!this.fns[t];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
  }
  update(e, t) {
    const n = this.getInterceptorIndex(e);
    return this.fns[n] ? (this.fns[n] = t, e) : !1;
  }
  use(e) {
    return this.fns.push(e), this.fns.length - 1;
  }
}
const de = () => ({
  error: new E(),
  request: new E(),
  response: new E()
}), fe = V({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), pe = {
  "Content-Type": "application/json"
}, L = (r = {}) => ({
  ...K,
  headers: pe,
  parseAs: "auto",
  querySerializer: fe,
  ...r
}), he = (r = {}) => {
  let e = W(L(), r);
  const t = () => ({ ...e }), n = (d) => (e = W(e, d), t()), i = de(), l = async (d) => {
    const a = {
      ...e,
      ...d,
      fetch: d.fetch ?? e.fetch ?? globalThis.fetch,
      headers: R(e.headers, d.headers),
      serializedBody: void 0
    };
    a.security && await le({
      ...a,
      security: a.security
    }), a.requestValidator && await a.requestValidator(a), a.body !== void 0 && a.bodySerializer && (a.serializedBody = a.bodySerializer(a.body)), (a.body === void 0 || a.serializedBody === "") && a.headers.delete("Content-Type");
    const f = B(a);
    return { opts: a, url: f };
  }, o = async (d) => {
    const { opts: a, url: f } = await l(d), j = {
      redirect: "follow",
      ...a,
      body: se(a)
    };
    let g = new Request(f, j);
    for (const p of i.request.fns)
      p && (g = await p(g, a));
    const D = a.fetch;
    let u = await D(g);
    for (const p of i.response.fns)
      p && (u = await p(u, g, a));
    const h = {
      request: g,
      response: u
    };
    if (u.ok) {
      const p = (a.parseAs === "auto" ? oe(u.headers.get("Content-Type")) : a.parseAs) ?? "json";
      if (u.status === 204 || u.headers.get("Content-Length") === "0") {
        let w;
        switch (p) {
          case "arrayBuffer":
          case "blob":
          case "text":
            w = await u[p]();
            break;
          case "formData":
            w = new FormData();
            break;
          case "stream":
            w = u.body;
            break;
          case "json":
          default:
            w = {};
            break;
        }
        return a.responseStyle === "data" ? w : {
          data: w,
          ...h
        };
      }
      let m;
      switch (p) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          m = await u[p]();
          break;
        case "stream":
          return a.responseStyle === "data" ? u.body : {
            data: u.body,
            ...h
          };
      }
      return p === "json" && (a.responseValidator && await a.responseValidator(m), a.responseTransformer && (m = await a.responseTransformer(m))), a.responseStyle === "data" ? m : {
        data: m,
        ...h
      };
    }
    const k = await u.text();
    let x;
    try {
      x = JSON.parse(k);
    } catch {
    }
    const A = x ?? k;
    let b = A;
    for (const p of i.error.fns)
      p && (b = await p(A, u, g, a));
    if (b = b || {}, a.throwOnError)
      throw b;
    return a.responseStyle === "data" ? void 0 : {
      error: b,
      ...h
    };
  }, s = (d) => (a) => o({ ...a, method: d }), c = (d) => async (a) => {
    const { opts: f, url: j } = await l(a);
    return Y({
      ...f,
      body: f.body,
      headers: f.headers,
      method: d,
      onRequest: async (g, D) => {
        let u = new Request(g, D);
        for (const h of i.request.fns)
          h && (u = await h(u, f));
        return u;
      },
      url: j
    });
  };
  return {
    buildUrl: B,
    connect: s("CONNECT"),
    delete: s("DELETE"),
    get: s("GET"),
    getConfig: t,
    head: s("HEAD"),
    interceptors: i,
    options: s("OPTIONS"),
    patch: s("PATCH"),
    post: s("POST"),
    put: s("PUT"),
    request: o,
    setConfig: n,
    sse: {
      connect: c("CONNECT"),
      delete: c("DELETE"),
      get: c("GET"),
      head: c("HEAD"),
      options: c("OPTIONS"),
      patch: c("PATCH"),
      post: c("POST"),
      put: c("PUT"),
      trace: c("TRACE")
    },
    trace: s("TRACE")
  };
}, N = he(L({
  baseUrl: "https://localhost:44360"
})), _ = {
  type: "repository",
  alias: "autoDictionaries.tree.repository",
  name: "AutoDictionaries Repository Settings",
  api: () => import("./auto-dictionaries.repository-BJCJ7jtm.js")
}, $ = {
  type: "menu",
  alias: "autoDictionaries.menu",
  name: "Auto dictionaries",
  meta: {
    label: "Auto dictionaries",
    icon: "icon-book",
    entityType: "auto-dictionaries-root"
  }
}, J = {
  type: "tree",
  kind: "default",
  alias: "autoDictionaries.tree",
  name: "Auto Dictionaries Tree Settings",
  meta: {
    repositoryAlias: _.alias
  }
}, me = {
  type: "menuItem",
  //kind: "tree",
  alias: "autoDictionaries.tree.menu.item",
  name: "Auto Dictionarie Tree Item",
  meta: {
    label: "Auto dictionaries",
    icon: "icon-book",
    entityType: "auto-dictionaries-root",
    menus: [
      $.alias
    ],
    treeAlias: J.alias
  }
}, ye = {
  type: "sectionSidebarApp",
  kind: "menu",
  alias: "autoDictionaries.sidebarapp",
  name: "Auto Dictionarie sidebar menu",
  weight: 100,
  meta: {
    label: "Auto Dictionary",
    menu: $.alias
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Translation"
    }
  ]
}, be = [
  ye,
  $,
  _,
  J,
  me
], we = {
  type: "dashboard",
  alias: "autoDictionaries.dashboard",
  name: "Auto dictionaries dashboard",
  js: () => import("./overview.element-CPsmODAI.js"),
  weight: 90,
  meta: {
    label: "Auto Dictionaries overview",
    pathname: "overview"
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Translation"
    }
  ]
}, ge = {
  type: "workspace",
  kind: "default",
  alias: "autoDictionaries.workspace",
  name: "Auto dictionaries workspace",
  js: () => import("./workspace.element-CJs3fs4h.js"),
  meta: {
    entityType: "auto-dictionaries-root"
  }
}, ke = {
  type: "workspaceContext",
  alias: "autoDictionaries.workspace.context",
  name: "Auto dictionaries workspace context",
  js: () => import("./workspace.context-B-0nVoUj.js"),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "autoDictionaries.workspace"
    }
  ]
}, Ae = [
  {
    type: "workspaceView",
    alias: "autoDictionaries.workspaceView.overview",
    name: "Auto dictionaries workspace overview view",
    js: () => import("./overview.element-CPsmODAI.js"),
    weight: 20,
    meta: {
      label: "#autoDictionaries_overview",
      pathname: "overview",
      icon: "icon-book"
    },
    conditions: [
      {
        alias: "Umb.Condition.WorkspaceAlias",
        match: "autoDictionaries.workspace"
      }
    ]
  },
  {
    type: "workspaceView",
    alias: "autoDictionaries.workspaceView.settings",
    name: "Auto dictionaries workspace settings view",
    js: () => import("./settings.element-RYNjxkfj.js"),
    weight: 10,
    meta: {
      label: "#sections_settings",
      pathname: "settings",
      icon: "icon-settings"
    },
    conditions: [
      {
        alias: "Umb.Condition.WorkspaceAlias",
        match: "autoDictionaries.workspace"
      }
    ]
  }
], Se = [
  ge,
  ke,
  we,
  ...Ae
], je = {
  type: "workspace",
  alias: "autoDictionaries.item.workspace",
  name: "Auto dictionaries item workspace",
  js: () => import("./workspace.element-CODfV0Hv.js"),
  meta: {
    entityType: "auto-dictionaries-item"
  }
}, xe = {
  type: "workspaceContext",
  alias: "autoDictionaries.item.workspace.context",
  name: "Auto dictionaries item workspace context",
  js: () => import("./workspace.context-Cm7KB7bD.js"),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "autoDictionaries.item.workspace"
    }
  ]
}, De = [
  je,
  xe
], Ce = [
  {
    type: "localization",
    alias: "autoDictionarie.lang.ja",
    name: "Japanese",
    weight: 0,
    meta: {
      culture: "ja"
    },
    js: () => import("./ja-2aDueh1u.js")
  },
  {
    type: "localization",
    alias: "autoDictionarie.lang.en",
    name: "English",
    weight: 1,
    meta: {
      culture: "en"
    },
    js: () => import("./en-By8zblft.js")
  }
], ze = [...Ce], Te = [
  ...be,
  ...Se,
  ...De,
  ...ze
], $e = (r, e) => {
  e.registerMany(Te), r.consumeContext(X, (t) => {
    if (t) {
      var n = t.getOpenApiConfiguration();
      N.setConfig({
        auth: n.token,
        baseUrl: n.base,
        credentials: n.credentials
      }), N.interceptors.request.use(async (i, l) => {
        const o = await t.getLatestToken();
        return i.headers.set("Authorization", `Bearer ${o}`), i;
      });
    }
  });
};
export {
  N as c,
  $e as o
};
//# sourceMappingURL=index-icOLYXQp.js.map
