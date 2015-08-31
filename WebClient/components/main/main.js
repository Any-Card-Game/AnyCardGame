var module = angular.module('ACG.Client');

module.controller('mainCtrl', function ($scope) {
  $scope.model = {};
  $scope.callback = {};
  $scope.model.shoes = 12;

  var AlchemyChatServer = new Alchemy({
    //Server: "127.0.0.1",
    Server: "45.79.186.117",
    DebugMode: false
  });

  var dt;
  var sdt;
  AlchemyChatServer.Connected = function () {
    console.log("connected");
    dt = new Date() | 0;
    sdt = new Date() | 0;
    AlchemyChatServer.Send(JSON.parse('{"$type": "SocketServer.CreateNewGameRequestSocketMessage, SocketServer", "GameType": null}'));
  };

  AlchemyChatServer.Disconnected = function () {
    console.log("disconnected");
  };
  var count = 0;
  AlchemyChatServer.MessageReceived = function (event) {
    //console.log(event.data);
    count++;
    if ((new Date() | 0) > (dt + 1000)) {
      dt = new Date() | 0;
      console.log(count / ((dt - sdt) / 1000));
    }
    var message = JSON.parse(event.data);
    console.log(message);

    if (message.$type == "SocketServer.AskQuestionSocketMessage, SocketServer") {
      var answerIndex = 0;
      if (message.Answers.length > 0) {
        answerIndex = 1;
      }
      AlchemyChatServer.Send(JSON.parse('{"$type": "SocketServer.AnswerQuestionSocketMessage, SocketServer", "AnswerIndex":' + answerIndex + '}'));

    }

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