angular.module("umbraco").controller("dictionaryItem.controller", function ($scope, $http, notificationsService) {

	var vm = this;

	vm.submit = function () {
		$http({
			url: "/umbraco/backoffice/api/AutoDictionariesApi/AddExistingDictionaryItemToTemplate",
			method: "POST",
			data: {
				template: $scope.model.template,
				dictionaryId: $scope.model.staticContent.Dictionary.Id,
				staticContent: $scope.model.staticContent.StaticContent
			}
		}).then(function (response) {
			if (response.data) {
				notificationsService.success("Dictionaries", "Dictionary were added to template successfully!");
			} else {
				notificationsService.error("Dictionaries", "Failed to add dictionary to template");
			}

			if ($scope.model.submit) {
				$scope.model.submit($scope.model);
			}
		});
	}

	vm.close = function () {
		if ($scope.model.close) {
			$scope.model.close();
		}
	}
});