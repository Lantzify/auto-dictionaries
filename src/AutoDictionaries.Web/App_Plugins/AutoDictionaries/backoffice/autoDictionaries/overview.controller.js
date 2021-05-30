angular.module("umbraco").controller("autoDictionaries.overview.controller", function ($scope, $http, $location) {

	var vm = this;

	vm.loading = true;

	vm.page = {
		title: "Auto dictionaries",
		description: "Overview of all avaible templates to genarte dictionaries to."
	};

	//Table
	vm.templates = [];

	vm.options = {
		includeProperties: [
			{ alias: "staticcontent.Count", header: "Static content" },
			{ alias: "dictionaries.Count", header: "Dictionary Values" }
		]
	};

	vm.clickedTemplate = function (id) {
		$location.path("/translation/autoDictionaries/edit/" + id);
	};

	vm.matchDictionary = function (arr) {

		var retVal = [];

		arr.forEach(function (obj) {
			if (obj.Dictionary !== null) {
				retVal.push(obj.Dictionary)
			}
		});

		return retVal;
	};

	$http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetAllTemplates").then(function (response) {
		vm.loading = false;
		vm.templates = response.data;
	});
});