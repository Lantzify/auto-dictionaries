angular.module("umbraco").controller("autoDictionaries.overview.controller", function ($http, $filter, $location) {

	var vm = this;

	vm.page = {
		title: "Auto dictionaries",
		description: "Overview of all avaible views to genarte dictionaries to.",
		navigation: [{
			name: "Overview",
			alias: "overview",
			icon: "icon-book",
			view: "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/apps/overview/overview.html",
			active: true
		},
		{
			name: "Settings",
			alias: "settings",
			icon: "icon-settings",
			view: "/App_Plugins/AutoDictionaries/backoffice/autoDictionaries/apps/settings/overview.html",
			active: false
		}]
	};
});