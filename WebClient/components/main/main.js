var module = angular.module('ACG.Client');
var Serializer = (function () {
    function Serializer() {
    }
    Serializer.fromBytes = function (buffer) {
        var dv = new DataView(buffer, 0);
        var obj = Serializer.findObject(dv.getUint8(0));
        var propIndex = 1;
        var props = [];
        for (var property in obj) {
            if (obj.hasOwnProperty(property)) {
                props.push(property);
            }
        }
        for (var indexObj = { index: 1 }; indexObj.index < dv.byteLength;) {
            var value = Serializer.toValue(dv, indexObj);
            obj[props[propIndex++]] = value;
        }
        return obj;
    };
    Serializer.findObject = function (messageType) {
        switch (messageType) {
            case SocketMessageType.CreateNewGameRequest:
                return { messageType: SocketMessageType.CreateNewGameRequest, gameType: null };
            case SocketMessageType.AskQuestion:
                return { messageType: SocketMessageType.AskQuestion, question: null, answers: null, user: null };
            case SocketMessageType.AnswerQuestion:
                return { messageType: SocketMessageType.AnswerQuestion, answerIndex: null };
            case SocketMessageType.GameOver:
                return { messageType: SocketMessageType.GameOver, };
            case SocketMessageType.GameStarted:
                return { messageType: SocketMessageType.GameStarted, };
        }
        throw new Error("Class not found: " + messageType);
    };
    Serializer.toValue = function (view, indexObj) {
        switch (view.getUint8(indexObj.index++)) {
            case Types.Short:
                var val = view.getInt16(indexObj.index, true);
                indexObj.index += 2;
                return val;
            case Types.String:
                var length_1 = view.getUint16(indexObj.index, true);
                indexObj.index += 2;
                var buff = view.buffer.slice(indexObj.index, indexObj.index + length_1);
                indexObj.index += length_1;
                var str = String.fromCharCode.apply(null, new Uint8Array(buff));
                return str;
            case Types.ArrayOfString:
                var strs = [];
                var strLength = view.getUint8(indexObj.index++);
                for (var i = 0; i < strLength; i++) {
                    strs.push(this.toValue(view, indexObj));
                }
                return strs;
            case Types.ArrayOfInt:
                break;
            case Types.Byte:
                return view.getUint8(indexObj.index++);
        }
    };
    Serializer.fromValue = function (value) {
        var s = typeof (value);
        if (s === 'string') {
            var utf8 = unescape(encodeURIComponent(value));
            var arr = [];
            arr.push(Types.String);
            var dv = new DataView(new ArrayBuffer(2), 0);
            dv.setInt16(0, value.length, true);
            arr.push(dv.getUint8(0));
            arr.push(dv.getUint8(1));
            for (var i = 0; i < utf8.length; i++) {
                arr.push(utf8.charCodeAt(i));
            }
            return arr;
        }
        if (s === 'number') {
            var arr = [];
            arr.push(Types.Short);
            var dv = new DataView(new ArrayBuffer(2), 0);
            dv.setInt16(0, value, true);
            arr.push(dv.getUint8(0));
            arr.push(dv.getUint8(1));
            return arr;
        }
    };
    Serializer.toBytes = function (param) {
        var bytes = [];
        bytes.push(param.messageType);
        for (var property in param) {
            if (param.hasOwnProperty(property) && property !== 'messageType') {
                var value = param[property];
                bytes.push.apply(bytes, this.fromValue(value));
            }
        }
        return bytes;
    };
    return Serializer;
}());
var SocketMessageType;
(function (SocketMessageType) {
    SocketMessageType[SocketMessageType["CreateNewGameRequest"] = 0] = "CreateNewGameRequest";
    SocketMessageType[SocketMessageType["AskQuestion"] = 1] = "AskQuestion";
    SocketMessageType[SocketMessageType["AnswerQuestion"] = 2] = "AnswerQuestion";
    SocketMessageType[SocketMessageType["GameOver"] = 3] = "GameOver";
    SocketMessageType[SocketMessageType["GameStarted"] = 4] = "GameStarted";
})(SocketMessageType || (SocketMessageType = {}));
var Types;
(function (Types) {
    Types[Types["String"] = 0] = "String";
    Types[Types["ArrayOfString"] = 1] = "ArrayOfString";
    Types[Types["Short"] = 2] = "Short";
    Types[Types["ArrayOfInt"] = 3] = "ArrayOfInt";
    Types[Types["Byte"] = 4] = "Byte";
})(Types || (Types = {}));
module.controller('mainCtrl', function ($scope) {
    $scope.model = {};
    $scope.callback = {};
    $scope.model.gameInfos = [];
    $scope.model.totalGames = 0;
    console.log(Serializer.toBytes({ messageType: SocketMessageType.CreateNewGameRequest, gamepad: 'sevens' }));
    var getGateway = function () {
        fetch('http://127.0.0.1:3579/api/gateway').then(function (resp) {
            return resp.json();
        }).then(function (a) {
            doit(a.data.gatewayUrl);
        });
    };
    getGateway();
    function doit(url) {
        var socket = new WebSocket("ws://" + url);
        socket.binaryType = 'arraybuffer';
        var dt;
        var sdt;
        socket.onopen = function () {
            console.log("connected");
            dt = new Date() | 0;
            sdt = new Date() | 0;
            var bytes = Serializer.toBytes({ messageType: SocketMessageType.CreateNewGameRequest, gamepad: 'sevens' });
            socket.send(Uint8Array.from(bytes).buffer);
        };
        socket.onclose = function () {
            console.log("disconnected");
        };
        var count = 0;
        socket.onmessage = function (event) {
            //console.log(event.data);
            count++;
            if ((new Date() | 0) > (dt + 1000)) {
                dt = new Date() | 0;
                console.log(count / ((dt - sdt) / 1000));
            }
            var message = Serializer.fromBytes(event.data);
            if (message.messageType === SocketMessageType.AskQuestion) {
                console.log(message);
                var answerIndex = 0;
                if (message.answers.length > 0) {
                    answerIndex = 1;
                }
                var bytes = Serializer.toBytes({
                    messageType: SocketMessageType.AnswerQuestion,
                    answerIndex: answerIndex
                });
                socket.send(Uint8Array.from(bytes).buffer);
                // socket.send('{"$type": "SocketServer.AnswerQuestionSocketMessage, SocketServer", "AnswerIndex":' + answerIndex + '}');
                $scope.model.gameInfos.push(message);
            }
            if (message.messageType === SocketMessageType.GameOver) {
                $scope.model.gameStarted = false;
                $scope.model.gameInfos = [];
                socket.close();
                getGateway();
                $scope.$digest();
            }
            if (message.messageType === SocketMessageType.GameStarted) {
                $scope.model.totalGames++;
                $scope.model.gameStarted = true;
                $scope.$digest();
            }
            $scope.$digest();
        };
    }
});
module.config(function ($locationProvider, $stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('main', {
        //abstract: true,
        url: '/',
        controller: 'mainCtrl',
        templateUrl: 'components/main/main.tpl.html'
    });
});
