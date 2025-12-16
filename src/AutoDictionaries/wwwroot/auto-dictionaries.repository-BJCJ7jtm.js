import { UmbTreeServerDataSourceBase as o, UmbTreeRepositoryBase as r } from "@umbraco-cms/backoffice/tree";
import { A as n } from "./sdk.gen-BwK88sZe.js";
class a extends o {
  constructor(t) {
    super(t, {
      getRootItems: i,
      getChildrenOf: c,
      getAncestorsOf: s,
      mapper: u
    });
  }
}
const s = async (e) => [], i = async (e) => await n.getChildren(), c = async (e) => [], u = (e) => ({
  unique: e?.id ?? null,
  parent: { unique: null, entityType: "auto-dictionaries-root" },
  name: e.name ?? "unknown",
  entityType: "auto-dictionaries-item",
  hasChildren: !1,
  isFolder: !1,
  icon: "icon-book"
});
class p extends r {
  constructor(t) {
    super(t, a);
  }
  async requestTreeRoot() {
    return { data: {
      unique: null,
      entityType: "auto-dictionaries-root",
      name: "Auto Dictionaries",
      icon: "icon-book",
      hasChildren: !1,
      isFolder: !0
    } };
  }
}
export {
  p as api,
  p as autoDictionariesRepository
};
//# sourceMappingURL=auto-dictionaries.repository-BJCJ7jtm.js.map
