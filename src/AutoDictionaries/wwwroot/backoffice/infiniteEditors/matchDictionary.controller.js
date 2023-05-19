angular.module("umbraco").controller("dictionaryItem.controller", function ($scope, $http, notificationsService, assetsService) {

	var vm = this;

	assetsService.loadJs("lib/jsdiff/diff.js", $scope).then(function () {
		$http({
			url: "/umbraco/backoffice/api/AutoDictionariesApi/PreviewAddExistingDictionaryItemToView",
			method: "POST",
			data: {
				autoDictionariesModel: $scope.model.autoDictionariesModel,
				dictionaryId: $scope.model.staticContent.Dictionary.Id,
				staticContent: $scope.model.staticContent.StaticContent
			}
		}).then(function (response) {
			$scope.changes = Diff.diffWords(response.data[0], response.data[1]);
		});
	});

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