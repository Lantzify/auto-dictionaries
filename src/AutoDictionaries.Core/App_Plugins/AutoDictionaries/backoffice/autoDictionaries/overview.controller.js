angular.module("umbraco").controller("autoDictionaries.overview.controller", function ($http, $filter, $location) {

	var vm = this;

	vm.loading = true;
	vm.sortOrder = {};

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

	vm.sort = function (columnName) {
		vm.sortOrder.column = columnName;
		vm.sortOrder.reverse = !vm.sortOrder.reverse;

		vm.views = $filter("orderBy")(vm.views, vm.sortOrder.column, vm.sortOrder.reverse);
	};

	$http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetAllViews").then(function (response) {
		vm.loading = false;
		vm.views = response.data;

		console.log(vm.views);
	});
});