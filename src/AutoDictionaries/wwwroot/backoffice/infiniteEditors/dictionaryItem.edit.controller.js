angular.module("umbraco").controller("dictionaryItem.edit.controller", function ($http, $scope, notificationsService, dictionaryResource) {

	var vm = this;
	vm.properties = [];
	vm.dictionaryItem = {};
	vm.model = $scope.model;

	dictionaryResource.getById(vm.model.dictionaryId).then(function (response) {
		vm.dictionaryItem = response;

		vm.dictionaryItem.translations.forEach(function (translation) {
			vm.properties.push( {
				alias: translation.isoCode,
				label: translation.displayName,
				value: translation.translation,
				view: "textarea"
			});
		});
	});

	vm.translate = function () {
		$http.get("/umbraco/backoffice/api/AutoDictionariesApi/TranslateDictionaryItem?id=" + vm.dictionaryItem.id).then(function (response) {
			if (response.data) {
				notificationsService.success("Dictionaries", "Successfully translated dictionary item!");
				if ($scope.model.submit) {
					$scope.model.submit($scope.model);
				}
			} else {
				notificationsService.error("Dictionaries", "Failed to translate dictionary item");
				$scope.model.close();
			}
		});
	}

	vm.submit = function () {
		vm.dictionaryItem.translations.forEach(function (translation, index) {
			translation.translation = vm.properties[index].value;
		});

		dictionaryResource.save(vm.dictionaryItem, false).then(function (response) {
			if ($scope.model.submit) {
				$scope.model.submit($scope.model);
			}
		})
	}

	vm.close = function () {
		if ($scope.model.close) {
			$scope.model.close();
		}
	}
});