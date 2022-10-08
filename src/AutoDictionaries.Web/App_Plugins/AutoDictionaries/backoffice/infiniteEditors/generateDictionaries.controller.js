angular.module("umbraco").controller("generateDictionaries.controller", function ($scope, $http, entityResource, notificationsService) {

	var vm = this;
	var counter = 0;

	vm.currentlyGenerating = "";
	vm.generatingPercentage = 0;
	vm.percentage = 100 / $scope.model.selectedContent.length;

	vm.submit = function () {

		vm.buttonState = "busy";

		(function generateDictionary(staticContent) {
			vm.currentlyGenerating = staticContent.StaticContent;

			entityResource.getSafeAlias(vm.currentlyGenerating, true).then(function (safeAlias) {

				staticContent.SafeAlias = safeAlias.alias;

				$http({
					url: "/umbraco/backoffice/api/AutoDictionariesApi/AddNewDictionaryItemToView",
					method: "POST",
					data: {
						autoDictionariesModel: $scope.model.autoDictionariesModel,
						staticContent: staticContent
					}
				}).then(function (response) {
					if (response.data) {

						vm.generatingPercentage += vm.percentage;
						counter += 1;
						setTimeout(function () {
							if (counter !== $scope.model.selectedContent.length) {
								generateDictionary($scope.model.selectedContent[counter]);

							} else {
								notificationsService.success("Dictionaries", "All dictionaries were created and added to the template successfully!");

								if ($scope.model.submit) {
									$scope.model.submit($scope.model);
								}
							}
						}, 1000);
					} else {
						notificationsService.error("Dictionaries", "Failed to add dictionary to template");
						$scope.model.close();
					}
				});
			});
		})($scope.model.selectedContent[counter]);
	};

	vm.close = function () {
		if ($scope.model.close) {
			$scope.model.close();
		}
	}
});
