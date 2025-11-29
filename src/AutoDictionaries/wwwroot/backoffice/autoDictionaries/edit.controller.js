angular.module("umbraco").controller("autoDictionaries.edit.controller", function ($q,
	$http,
	$route,
	$routeParams,
	$location,
	editorService) {

	var vm = this;
	vm.loading = true;

	vm.overlay = {};
	vm.parent = {};
	vm.selectedContent = [];
	vm.allDictionaryItems = []

	$q.all({
		getTranslateSetting: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetTranslateSetting"),
		getView: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetView?id=" + $routeParams.id),
		getAllDictionaryItems: $http.get("/umbraco/backoffice/api/AutoDictionariesApi/GetAllDictionaryItems"),
	}).then(function (promises) {

		vm.view = promises.getView.data;
		vm.loading = false;
		vm.page = {
			title: "Edit " + vm.view.Name + " dictionaries",
			description: "Edit view dictionaries."
		};

		vm.allowTranslate = promises.getTranslateSetting.data;
		vm.allDictionaryItems = promises.getAllDictionaryItems.data;
	});

	vm.clearSelection = function () {
		vm.selectedContent = [];
	};

	vm.toggleSelectAll = function () {
		if (vm.selectedContent.length !== vm.view.StaticContent.length) {
			vm.view.StaticContent.forEach(function (content) {
				if (vm.selectedContent.indexOf(content.StaticContent) === -1) {
					vm.toggleSelect(content.StaticContent)
				}
			});
		} else {
			vm.selectedContent = [];
		}
	};

	vm.toggleSelect = function (content, parent) {
		var obj = {
			StaticContent: content,
			Parent: parent
		};

		var pos = vm.findIndex(vm.selectedContent, content);

		if (pos !== -1) {
			vm.selectedContent.splice(pos, 1);
		} else {
			vm.selectedContent.push(obj);
		}
	};

	vm.changeAllParent = function (parent) {
		vm.selectedContent.forEach(function (obj) {
			obj.Parent = parent;
			vm.parent[vm.findIndex(vm.view.StaticContent, obj.StaticContent)] = parent;
		});
	}

	vm.changeParent = function (staticContent, parent) {
		var pos = vm.findIndex(vm.selectedContent, staticContent);

		if (pos !== -1) {
			vm.selectedContent[pos].Parent = parent;
		}
	};

	vm.generateDictionaries = function () {

		vm.generateDictionaries.editor = {
			view: "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/generateDictionaries.html",
			title: "Generate dictionaries",
			size: "medium",
			selectedContent: vm.selectedContent,
			autoDictionariesModel: vm.view,
			allowTranslate: vm.allowTranslate,
			submit: function () {
				editorService.close();
				$route.reload();
			},
			close: function () {
				editorService.close();
			}
		};

		editorService.open(vm.generateDictionaries.editor)
	};

	vm.matchDictionary = function (staticContent) {
		vm.matchDictionary.editor = {
			view: "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/matchDictionary.html",
			title: "Match dictionary item",
			size: "medium",
			staticContent: staticContent,
			autoDictionariesModel: vm.view,
			submit: function () {
				editorService.close();
				$route.reload();
			},
			close: function () {
				editorService.close();
			}
		};

		editorService.open(vm.matchDictionary.editor);
	};

	vm.openTemplate = function () {

		var infiniteOptions = {
			id: $routeParams.id,
			submit: function () {
				editorService.close();
				$route.reload();
			},
			close: function () {
				editorService.close();
			}
		};

		editorService.templateEditor(infiniteOptions);
	};

	vm.openPartialView = function () {

		var infiniteOptions = {
			view: "views/partialViews/edit.html",
			id: encodeURIComponent(vm.view.Path),
			submit: function () {
				editorService.close();
				$route.reload();
			},
			close: function () {
				editorService.close();
			}
		};

		editorService.open(infiniteOptions);
	};

	vm.openDictionary = function (dictionaryId) {
		vm.openDictionary.editor = {
			view: "/App_Plugins/AutoDictionaries/backoffice/infiniteEditors/dictionaryItem.html",
			title: "Edit dictionary item",
			size: "small",
			dictionaryId: dictionaryId,
			submit: function () {
				editorService.close();
				$route.reload();
			},
			close: function () {
				editorService.close();
			}
		};

		editorService.open(vm.openDictionary.editor);
	};

	vm.goBack = function () {
		$location.path("/translation/autoDictionaries/overview");
	};

	vm.findIndex = function (arr, content) {
		return arr.findIndex(function (index) {
			return index.StaticContent === content;
		});
	};
});