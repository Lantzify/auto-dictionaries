import { c as e } from "./index-icOLYXQp.js";
class a {
  static postAddExistingDictionaryItem(t) {
    return (t?.client ?? e).post({
      url: "/add-existing-dictionary-item",
      ...t,
      headers: {
        "Content-Type": "application/json",
        ...t?.headers
      }
    });
  }
  static postAddNewDictionaryItem(t) {
    return (t?.client ?? e).post({
      url: "/add-new-dictionary-item",
      ...t,
      headers: {
        "Content-Type": "application/json",
        ...t?.headers
      }
    });
  }
  static getChildren(t) {
    return (t?.client ?? e).get({
      url: "/Children",
      ...t
    });
  }
  static getGetAllDictionaryItems(t) {
    return (t?.client ?? e).get({
      url: "/get-all-dictionary-items",
      ...t
    });
  }
  static getGetAllViews(t) {
    return (t?.client ?? e).get({
      url: "/get-all-views",
      ...t
    });
  }
  static getGetApiEndpoint(t) {
    return (t?.client ?? e).get({
      url: "/get-api-endpoint",
      ...t
    });
  }
  static getGetApiKey(t) {
    return (t?.client ?? e).get({
      url: "/get-api-key",
      ...t
    });
  }
  static getGetApiRegion(t) {
    return (t?.client ?? e).get({
      url: "/get-api-region",
      ...t
    });
  }
  static getGetPreviewById(t) {
    return (t.client ?? e).get({
      url: "/get-preview/{id}",
      ...t
    });
  }
  static getGetTranslateSetting(t) {
    return (t?.client ?? e).get({
      url: "/get-translate-setting",
      ...t
    });
  }
  static getGetTranslatorSetting(t) {
    return (t?.client ?? e).get({
      url: "/get-translator-setting",
      ...t
    });
  }
  static getGetViewById(t) {
    return (t.client ?? e).get({
      url: "/get-view/{id}",
      ...t
    });
  }
  static postPreviewAddExistingDictionaryItem(t) {
    return (t?.client ?? e).post({
      url: "/preview-add-existing-dictionary-item",
      ...t,
      headers: {
        "Content-Type": "application/json",
        ...t?.headers
      }
    });
  }
  static postPreviewAddNewDictionaryItem(t) {
    return (t?.client ?? e).post({
      url: "/preview-add-new-dictionary-item",
      ...t,
      headers: {
        "Content-Type": "application/json",
        ...t?.headers
      }
    });
  }
  static getTranslateDictionaryItemById(t) {
    return (t.client ?? e).get({
      url: "/translate-dictionary-item/{id}",
      ...t
    });
  }
}
export {
  a as A
};
//# sourceMappingURL=sdk.gen-BwK88sZe.js.map
