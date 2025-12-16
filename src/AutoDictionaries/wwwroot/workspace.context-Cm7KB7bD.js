import { UmbControllerBase as s } from "@umbraco-cms/backoffice/class-api";
import { UMB_WORKSPACE_CONTEXT as r } from "@umbraco-cms/backoffice/workspace";
import { UmbObjectState as e } from "@umbraco-cms/backoffice/observable-api";
import { tryExecute as a } from "@umbraco-cms/backoffice/resources";
import { A as o } from "./sdk.gen-BwK88sZe.js";
class d extends s {
  workspaceAlias = "autoDictionaries.item.workspace";
  #t = new e(void 0);
  unique = this.#t.asObservable();
  #e = new e(void 0);
  currentItem = this.#e.asObservable();
  #i = new e(!1);
  isLoading = this.#i.asObservable();
  constructor(t) {
    super(t), this.provideContext(r, this);
  }
  setUnique(t) {
    this.#t.setValue(t), t && this.load(t);
  }
  getUnique() {
    return this.#t.getValue();
  }
  async load(t) {
    this.#i.setValue(!0);
    const { data: i, error: u } = await a(
      this._host,
      o.getGetViewById({ path: { id: t } })
    );
    i && this.#e.setValue(i), this.#i.setValue(!1);
  }
  getCurrentItem() {
    return this.#e.getValue();
  }
  getEntityType() {
    return "auto-dictionaries-item";
  }
}
export {
  d as AutoDictionariesItemWorkspaceContext,
  d as default
};
//# sourceMappingURL=workspace.context-Cm7KB7bD.js.map
