var module = angular.module('ACG.Client');

module.controller('mainCtrl', function ($scope) {
  $scope.model = {};
  $scope.callback = {};
  $scope.model.shoes = 12;

  var AlchemyChatServer = new Alchemy({
    Server: "45.79.186.117",
    DebugMode: false
  });

  AlchemyChatServer.Connected = function () {
    console.log("connected");

  };

  AlchemyChatServer.Disconnected = function () {
    console.log("disconnected");
  };

  AlchemyChatServer.MessageReceived = function (event) {
    console.log(event.data);
    AlchemyChatServer.Send({foo: 12, bar: 15});
  };

  AlchemyChatServer.Start();

});

module.config(function ($locationProvider, $stateProvider, $urlRouterProvider) {
  $stateProvider
    .state('main', {
      //abstract: true,
      url: '/',
      controller: 'mainCtrl',
      templateUrl: 'components/main/main.tpl.html'
    })
});