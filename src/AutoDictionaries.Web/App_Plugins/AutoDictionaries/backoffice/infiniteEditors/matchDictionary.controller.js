angular.module("umbraco").controller("dictionaryItem.controller", function ($scope, $http, notificationsService) {

	var vm = this;

	vm.submit = function () {
		vm.buttonState = "busy";

		$http({
			url: "/umbraco/backoffice/api/AutoDictionariesApi/AddExistingDictionaryItemToView",
			method: "POST",
			data: {
				autoDictionariesModel: $scope.model.autoDictionariesModel,
				dictionaryId: $scope.model.staticContent.Dictionary.Id,
				staticContent: $scope.model.staticContent.StaticContent
			}
		}).then(function (response) {
			if (response.data) {
				notificationsService.success("Dictionaries", "Dictionary were added to template successfully!");
				vm.buttonState = "success";
			} else {
				notificationsService.error("Dictionaries", "Failed to add dictionary to template");
				vm.buttonState = "error";
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