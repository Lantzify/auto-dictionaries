angular.module("umbraco").controller("autoDictionaries.app.settings.overview.controller", function ($q, $http) {

	var vm = this;

    $q.all({
        translate: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetTranslateSetting"),
        translator: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetTranslatorSetting"),
        apiKey: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetApiKey"),
        apiEndpoint: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetApiEndpoint"),
        apiRegion: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetApiRegion")
    }).then(function (promises) {
     
        var currentAppSettings = {
            AutoBlockList: {
                Translate: promises.translate.data,
                Translator: promises.translator.data,
                ApiKey: promises.apiKey.data,
                ApiEndpoint: promises.apiEndpoint.data,
                ApiRegion: promises.apiRegion.data,
            }
        };

        vm.currentAppSettings = JSON.stringify(currentAppSettings, null, 4);
    });

    var appSettings = {
        AutoBlockList: {
            Translate: false,
            Translator: "DeepL",
            ApiKey: "***************",
            ApiEndpoint: "",
            ApiRegion: "",
        }
    };

    vm.appSettings = JSON.stringify(appSettings, null, 4);
});
