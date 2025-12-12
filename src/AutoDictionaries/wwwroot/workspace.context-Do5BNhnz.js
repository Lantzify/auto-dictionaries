import { UmbControllerBase as o } from "@umbraco-cms/backoffice/class-api";
import "@umbraco-cms/backoffice/element-api";
import "@umbraco-cms/backoffice/external/lit";
import { UMB_WORKSPACE_CONTEXT as r } from "@umbraco-cms/backoffice/workspace";
class n extends o {
  workspaceAlias = "autoDictionaries.workspace";
  constructor(t) {
    super(t), this.provideContext(r, this);
  }
  getEntityType() {
    return "auto-dictionaries-root";
  }
}
export {
  n as autoDictionariesWorkspaceContext,
  n as default
};
//# sourceMappingURL=workspace.context-Do5BNhnz.js.map
