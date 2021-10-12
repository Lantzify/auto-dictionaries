angular.module("umbraco").controller("partialView.edit.controller", function ($scope, $http, notificationsService) {

	var vm = this;
	vm.model = $scope.model;

    // ace configuration
	vm.aceOption = {
		mode: "razor",
		theme: "chrome",
		showPrintMargin: false,
		advanced: {
			fontSize: "14px"
		}
	};

	vm.submit = function () {
		vm.buttonState = "busy";

		$http({
			url: "/umbraco/backoffice/api/AutoDictionariesApi/SavePartialView",
			method: "POST",
			data: {
				content: vm.model.content,
				path: vm.model.path
			}
		}).then(function (response) {
			if (response.data) {
				notificationsService.success("Partial view", "Partial view were saved successfully!");
				vm.buttonState = "success";
			} else {
				notificationsService.error("Partial view", "Failed to save partial view");
				vm.buttonState = "error";
			}

			if ($scope.model.submit) {
				$scope.model.submit($scope.model);
			}
		});
	};

	vm.close = function () {
		if ($scope.model.close) {
			$scope.model.close();
		}
	}
});