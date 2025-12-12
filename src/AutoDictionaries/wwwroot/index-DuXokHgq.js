import { UMB_AUTH_CONTEXT as M } from "@umbraco-cms/backoffice/auth";
const G = {
  bodySerializer: (r) => JSON.stringify(
    r,
    (e, t) => typeof t == "bigint" ? t.toString() : t
  )
}, Q = ({
  onRequest: r,
  onSseError: e,
  onSseEvent: t,
  responseTransformer: n,
  responseValidator: s,
  sseDefaultRetryDelay: l,
  sseMaxRetryAttempts: o,
  sseMaxRetryDelay: i,
  sseSleepFn: c,
  url: d,
  ...a
}) => {
  let f;
  const A = c ?? ((u) => new Promise((h) => setTimeout(h, u)));
  return { stream: async function* () {
    let u = l ?? 3e3, h = 0;
    const k = a.signal ?? new AbortController().signal;
    for (; !k.aborted; ) {
      h++;
      const x = a.headers instanceof Headers ? a.headers : new Headers(a.headers);
      f !== void 0 && x.set("Last-Event-ID", f);
      try {
        const S = {
          redirect: "follow",
          ...a,
          body: a.serializedBody,
          headers: x,
          signal: k
        };
        let b = new Request(d, S);
        r && (b = await r(d, S));
        const m = await (a.fetch ?? globalThis.fetch)(b);
        if (!m.ok)
          throw new Error(
            `SSE failed: ${m.status} ${m.statusText}`
          );
        if (!m.body) throw new Error("No body in SSE response");
        const w = m.body.pipeThrough(new TextDecoderStream()).getReader();
        let D = "";
        const $ = () => {
          try {
            w.cancel();
          } catch {
          }
        };
        k.addEventListener("abort", $);
        try {
          for (; ; ) {
            const { done: L, value: _ } = await w.read();
            if (L) break;
            D += _;
            const O = D.split(`

`);
            D = O.pop() ?? "";
            for (const J of O) {
              const F = J.split(`
`), E = [];
              let q;
              for (const y of F)
                if (y.startsWith("data:"))
                  E.push(y.replace(/^data:\s*/, ""));
                else if (y.startsWith("event:"))
                  q = y.replace(/^event:\s*/, "");
                else if (y.startsWith("id:"))
                  f = y.replace(/^id:\s*/, "");
                else if (y.startsWith("retry:")) {
                  const I = Number.parseInt(
                    y.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(I) || (u = I);
                }
              let j, v = !1;
              if (E.length) {
                const y = E.join(`
`);
                try {
                  j = JSON.parse(y), v = !0;
                } catch {
                  j = y;
                }
              }
              v && (s && await s(j), n && (j = await n(j))), t?.({
                data: j,
                event: q,
                id: f,
                retry: u
              }), E.length && (yield j);
            }
          }
        } finally {
          k.removeEventListener("abort", $), w.releaseLock();
        }
        break;
      } catch (S) {
        if (e?.(S), o !== void 0 && h >= o)
          break;
        const b = Math.min(
          u * 2 ** (h - 1),
          i ?? 3e4
        );
        await A(b);
      }
    }
  }() };
}, X = (r) => {
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
}, K = (r) => {
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
}, Y = (r) => {
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
  value: s
}) => {
  if (!e) {
    const i = (r ? s : s.map((c) => encodeURIComponent(c))).join(K(n));
    switch (n) {
      case "label":
        return `.${i}`;
      case "matrix":
        return `;${t}=${i}`;
      case "simple":
        return i;
      default:
        return `${t}=${i}`;
    }
  }
  const l = X(n), o = s.map((i) => n === "label" || n === "simple" ? r ? i : encodeURIComponent(i) : C({
    allowReserved: r,
    name: t,
    value: i
  })).join(l);
  return n === "label" || n === "matrix" ? l + o : o;
}, C = ({
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
}, W = ({
  allowReserved: r,
  explode: e,
  name: t,
  style: n,
  value: s,
  valueOnly: l
}) => {
  if (s instanceof Date)
    return l ? s.toISOString() : `${t}=${s.toISOString()}`;
  if (n !== "deepObject" && !e) {
    let c = [];
    Object.entries(s).forEach(([a, f]) => {
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
  const o = Y(n), i = Object.entries(s).map(
    ([c, d]) => C({
      allowReserved: r,
      name: n === "deepObject" ? `${t}[${c}]` : c,
      value: d
    })
  ).join(o);
  return n === "label" || n === "matrix" ? o + i : i;
}, Z = /\{[^{}]+\}/g, ee = ({ path: r, url: e }) => {
  let t = e;
  const n = e.match(Z);
  if (n)
    for (const s of n) {
      let l = !1, o = s.substring(1, s.length - 1), i = "simple";
      o.endsWith("*") && (l = !0, o = o.substring(0, o.length - 1)), o.startsWith(".") ? (o = o.substring(1), i = "label") : o.startsWith(";") && (o = o.substring(1), i = "matrix");
      const c = r[o];
      if (c == null)
        continue;
      if (Array.isArray(c)) {
        t = t.replace(
          s,
          P({ explode: l, name: o, style: i, value: c })
        );
        continue;
      }
      if (typeof c == "object") {
        t = t.replace(
          s,
          W({
            explode: l,
            name: o,
            style: i,
            value: c,
            valueOnly: !0
          })
        );
        continue;
      }
      if (i === "matrix") {
        t = t.replace(
          s,
          `;${C({
            name: o,
            value: c
          })}`
        );
        continue;
      }
      const d = encodeURIComponent(
        i === "label" ? `.${c}` : c
      );
      t = t.replace(s, d);
    }
  return t;
}, te = ({
  baseUrl: r,
  path: e,
  query: t,
  querySerializer: n,
  url: s
}) => {
  const l = s.startsWith("/") ? s : `/${s}`;
  let o = (r ?? "") + l;
  e && (o = ee({ path: e, url: o }));
  let i = t ? n(t) : "";
  return i.startsWith("?") && (i = i.substring(1)), i && (o += `?${i}`), o;
};
function re(r) {
  const e = r.body !== void 0;
  if (e && r.bodySerializer)
    return "serializedBody" in r ? r.serializedBody !== void 0 && r.serializedBody !== "" ? r.serializedBody : null : r.body !== "" ? r.body : null;
  if (e)
    return r.body;
}
const ae = async (r, e) => {
  const t = typeof e == "function" ? await e(r) : e;
  if (t)
    return r.scheme === "bearer" ? `Bearer ${t}` : r.scheme === "basic" ? `Basic ${btoa(t)}` : t;
}, H = ({
  allowReserved: r,
  array: e,
  object: t
} = {}) => (s) => {
  const l = [];
  if (s && typeof s == "object")
    for (const o in s) {
      const i = s[o];
      if (i != null)
        if (Array.isArray(i)) {
          const c = P({
            allowReserved: r,
            explode: !0,
            name: o,
            style: "form",
            value: i,
            ...e
          });
          c && l.push(c);
        } else if (typeof i == "object") {
          const c = W({
            allowReserved: r,
            explode: !0,
            name: o,
            style: "deepObject",
            value: i,
            ...t
          });
          c && l.push(c);
        } else {
          const c = C({
            allowReserved: r,
            name: o,
            value: i
          });
          c && l.push(c);
        }
    }
  return l.join("&");
}, se = (r) => {
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
}, ie = (r, e) => e ? !!(r.headers.has(e) || r.query?.[e] || r.headers.get("Cookie")?.includes(`${e}=`)) : !1, ne = async ({
  security: r,
  ...e
}) => {
  for (const t of r) {
    if (ie(e, t.name))
      continue;
    const n = await ae(t, e.auth);
    if (!n)
      continue;
    const s = t.name ?? "Authorization";
    switch (t.in) {
      case "query":
        e.query || (e.query = {}), e.query[s] = n;
        break;
      case "cookie":
        e.headers.append("Cookie", `${s}=${n}`);
        break;
      case "header":
      default:
        e.headers.set(s, n);
        break;
    }
  }
}, U = (r) => te({
  baseUrl: r.baseUrl,
  path: r.path,
  query: r.query,
  querySerializer: typeof r.querySerializer == "function" ? r.querySerializer : H(r.querySerializer),
  url: r.url
}), B = (r, e) => {
  const t = { ...r, ...e };
  return t.baseUrl?.endsWith("/") && (t.baseUrl = t.baseUrl.substring(0, t.baseUrl.length - 1)), t.headers = V(r.headers, e.headers), t;
}, oe = (r) => {
  const e = [];
  return r.forEach((t, n) => {
    e.push([n, t]);
  }), e;
}, V = (...r) => {
  const e = new Headers();
  for (const t of r) {
    if (!t)
      continue;
    const n = t instanceof Headers ? oe(t) : Object.entries(t);
    for (const [s, l] of n)
      if (l === null)
        e.delete(s);
      else if (Array.isArray(l))
        for (const o of l)
          e.append(s, o);
      else l !== void 0 && e.set(
        s,
        typeof l == "object" ? JSON.stringify(l) : l
      );
  }
  return e;
};
class T {
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
const ce = () => ({
  error: new T(),
  request: new T(),
  response: new T()
}), le = H({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), ue = {
  "Content-Type": "application/json"
}, R = (r = {}) => ({
  ...G,
  headers: ue,
  parseAs: "auto",
  querySerializer: le,
  ...r
}), de = (r = {}) => {
  let e = B(R(), r);
  const t = () => ({ ...e }), n = (d) => (e = B(e, d), t()), s = ce(), l = async (d) => {
    const a = {
      ...e,
      ...d,
      fetch: d.fetch ?? e.fetch ?? globalThis.fetch,
      headers: V(e.headers, d.headers),
      serializedBody: void 0
    };
    a.security && await ne({
      ...a,
      security: a.security
    }), a.requestValidator && await a.requestValidator(a), a.body !== void 0 && a.bodySerializer && (a.serializedBody = a.bodySerializer(a.body)), (a.body === void 0 || a.serializedBody === "") && a.headers.delete("Content-Type");
    const f = U(a);
    return { opts: a, url: f };
  }, o = async (d) => {
    const { opts: a, url: f } = await l(d), A = {
      redirect: "follow",
      ...a,
      body: re(a)
    };
    let g = new Request(f, A);
    for (const p of s.request.fns)
      p && (g = await p(g, a));
    const z = a.fetch;
    let u = await z(g);
    for (const p of s.response.fns)
      p && (u = await p(u, g, a));
    const h = {
      request: g,
      response: u
    };
    if (u.ok) {
      const p = (a.parseAs === "auto" ? se(u.headers.get("Content-Type")) : a.parseAs) ?? "json";
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
    const S = x ?? k;
    let b = S;
    for (const p of s.error.fns)
      p && (b = await p(S, u, g, a));
    if (b = b || {}, a.throwOnError)
      throw b;
    return a.responseStyle === "data" ? void 0 : {
      error: b,
      ...h
    };
  }, i = (d) => (a) => o({ ...a, method: d }), c = (d) => async (a) => {
    const { opts: f, url: A } = await l(a);
    return Q({
      ...f,
      body: f.body,
      headers: f.headers,
      method: d,
      onRequest: async (g, z) => {
        let u = new Request(g, z);
        for (const h of s.request.fns)
          h && (u = await h(u, f));
        return u;
      },
      url: A
    });
  };
  return {
    buildUrl: U,
    connect: i("CONNECT"),
    delete: i("DELETE"),
    get: i("GET"),
    getConfig: t,
    head: i("HEAD"),
    interceptors: s,
    options: i("OPTIONS"),
    patch: i("PATCH"),
    post: i("POST"),
    put: i("PUT"),
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
    trace: i("TRACE")
  };
}, N = de(R({
  baseUrl: "https://localhost:44360"
})), fe = {
  type: "sectionSidebarApp",
  kind: "menu",
  alias: "autoDictionaries.sidebarapp",
  name: "Auto Dictionarie sidebar menu",
  weight: 10,
  meta: {
    label: "Auto Dictionary",
    menu: "autoDictionaries.menu"
  },
  conditions: [
    {
      alias: "Umb.Condition.SectionAlias",
      match: "Umb.Section.Translation"
    }
  ]
}, pe = {
  type: "menu",
  alias: "autoDictionaries.menu",
  name: "Auto dictionaries",
  meta: {
    label: "Auto dictionaries",
    icon: "icon-book"
    //entityType: "auto-dictionaries-menu",
  }
}, he = {
  type: "menuItem",
  alias: "autoDictionaries.menu.item",
  name: "Auto Dictionarie item",
  //element: "auto-dictionaries-menu",
  meta: {
    label: "Auto dictionaries",
    icon: "icon-book",
    entityType: "auto-dictionaries",
    menus: [
      "autoDictionaries.menu"
    ]
  }
}, me = [
  pe,
  he,
  fe
], ye = {
  type: "workspace",
  alias: "autoDictionaries.workspace",
  name: "Auto dictionaries workspace",
  js: () => import("./workspace.element-BaeGX5rj.js"),
  meta: {
    entityType: "auto-dictionaries"
  }
}, be = {
  type: "workspaceContext",
  alias: "autoDictionaries.workspace.context",
  name: "Auto dictionaries workspace context",
  js: () => import("./workspace.context-Do5BNhnz.js"),
  conditions: [
    {
      alias: "Umb.Condition.WorkspaceAlias",
      match: "autoDictionaries.workspace"
    }
  ]
}, we = [
  {
    type: "workspaceView",
    alias: "autoDictionaries.workspaceView.overview",
    name: "Auto dictionaries workspace overview view",
    js: () => import("./overview.element-BH2rorHA.js"),
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
    js: () => import("./settings.element-Bxr6OyKv.js"),
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
], ge = [
  ye,
  be,
  ...we
], ke = [
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
], Se = [...ke], je = [
  ...me,
  ...ge,
  ...Se
], xe = (r, e) => {
  e.registerMany(je), r.consumeContext(M, (t) => {
    if (t) {
      var n = t.getOpenApiConfiguration();
      N.setConfig({
        auth: n.token,
        baseUrl: n.base,
        credentials: n.credentials
      }), N.interceptors.request.use(async (s, l) => {
        const o = await t.getLatestToken();
        return s.headers.set("Authorization", `Bearer ${o}`), s;
      });
    }
  });
};
export {
  N as c,
  xe as o
};
//# sourceMappingURL=index-DuXokHgq.js.map
