angular.module("umbraco").controller("autoDictionaries.overview.controller", function ($scope, $http, $location) {

	var vm = this;

	vm.loading = true;
	vm.reverse = false;

	vm.page = {
		title: "Auto dictionaries",
		description: "Overview of all avaible views to genarte dictionaries to."
	};

	//Table
	vm.views = [];

	vm.options = {
		includeProperties: [
			{ alias: "staticcontent.Count", header: "Static content" },
			{ alias: "dictionaries.Count", header: "Dictionary Values" }
		]
	};

	vm.clickedView = function (id) {
		$location.path("/translation/autoDictionaries/edit/" + id);
	};

	vm.sort = function (property, reverseFirstSort) {

		if (property === vm.sorted) {
			vm.reverse = !vm.reverse;
		} else {
			vm.reverse = reverseFirstSort;
		}

		vm.sorted = property;
	}

	vm.matchDictionary = function (arr) {

		var retVal = [];

		arr.forEach(function (obj) {
			if (obj.Dictionary !== null) {
				retVal.push(obj.Dictionary)
			}
		});

		return retVal;
	};

	$http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetAllViews").then(function (response) {
		vm.loading = false;
		vm.views = response.data;
	});
});