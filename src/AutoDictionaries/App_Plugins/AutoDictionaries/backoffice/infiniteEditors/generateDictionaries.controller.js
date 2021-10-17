angular.module("umbraco").controller("generateDictionaries.controller", function ($scope, $http, notificationsService) {

	var vm = this;

	vm.generating = false;
	vm.successGenerating = true;
	vm.currentlyGenerating = "";
	vm.percentage = 100 / $scope.model.selectedContent.length;
	vm.generatingPercentage = 0;

	vm.submit = function () {
		vm.buttonState = "busy";

		vm.generating = true;

		var generatingDictionaries = new Promise(function (resolve, reject) {
			$scope.model.selectedContent.forEach(function (staticContent, index, array) {

				vm.currentlyGenerating = staticContent.StaticContent;

				$http({
					url: "/umbraco/backoffice/api/AutoDictionariesApi/AddNewDictionaryItemToView",
					method: "POST",
					data: {
						autoDictionariesModel: $scope.model.autoDictionariesModel,
						staticContent: staticContent
					}
				}).then(function (response) {
					if (response.data) {
						vm.successGenerating = true;
						vm.generatingPercentage += vm.percentage;
					} else {
						vm.successGenerating = false;
					}

					if (index + 1 === array.length) {
						setTimeout(function () {
							resolve();
						}, 1000);
					}
				});
			});
		});

		generatingDictionaries.then(function () {
			if (vm.successGenerating) {
				notificationsService.success("Dictionaries", "All dictionaries were created adn added to the template successfully!");
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