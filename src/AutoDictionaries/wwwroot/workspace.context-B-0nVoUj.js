import { UmbControllerBase as o } from "@umbraco-cms/backoffice/class-api";
import { UMB_WORKSPACE_CONTEXT as r } from "@umbraco-cms/backoffice/workspace";
class a extends o {
  workspaceAlias = "autoDictionaries.workspace";
  constructor(t) {
    super(t), this.provideContext(r, this);
  }
  getEntityType() {
    return "auto-dictionaries-root";
  }
}
export {
  a as autoDictionariesWorkspaceContext,
  a as default
};
//# sourceMappingURL=workspace.context-B-0nVoUj.js.map
