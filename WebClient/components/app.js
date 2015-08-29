var module=angular.module('ACG.Client',[
  'ui.router'
]);


module.config(function ($locationProvider, $stateProvider, $urlRouterProvider) {
  $urlRouterProvider.otherwise('/');
});