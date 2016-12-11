var module = angular.module('ACG.Client');

class Serializer {
    static fromBytes(buffer: ArrayBuffer) {

        var dv = new DataView(buffer, 0);

        let obj = Serializer.findObject(dv.getUint8(0));
        let propIndex = 1;
        let props = [];
        for (let property in obj) {
            if (obj.hasOwnProperty(property)) {
                props.push(property);
            }
        }

        for (let indexObj = {index: 1}; indexObj.index < dv.byteLength;) {
            let value = Serializer.toValue(dv, indexObj);
            obj[props[propIndex++]] = value;
        }
        return obj;
    }

    private static findObject(messageType: number): any {
        switch (messageType) {
            case SocketMessageType.CreateNewGameRequest:
                return {messageType: SocketMessageType.CreateNewGameRequest, gameType: null};
            case SocketMessageType.AskQuestion:
                return {messageType: SocketMessageType.AskQuestion, question: null, answers: null, user: null};
            case SocketMessageType.AnswerQuestion:
                return {messageType: SocketMessageType.AnswerQuestion, answerIndex: null};
            case SocketMessageType.GameOver:
                return {messageType: SocketMessageType.GameOver,};
            case SocketMessageType.GameStarted:
                return {messageType: SocketMessageType.GameStarted,};
        }
        throw new Error("Class not found: " + messageType);
    }

    private static toValue(view: DataView, indexObj: {index: number}) {
        switch (view.getUint8(indexObj.index++)) {
            case Types.Short:
                let val = view.getInt16(indexObj.index, true);
                indexObj.index += 2;
                return val;
            case Types.String:
                let length = view.getUint16(indexObj.index, true);
                indexObj.index += 2;
                var buff = view.buffer.slice(indexObj.index, indexObj.index + length)
                indexObj.index += length;
                let str = String.fromCharCode.apply(null, new Uint8Array(buff));
                return str;
            case Types.ArrayOfString:
                var strs = [];
                let strLength = view.getUint8(indexObj.index++);
                for (var i = 0; i < strLength; i++) {
                    strs.push(this.toValue(view, indexObj));
                }
                return strs;
            case Types.ArrayOfInt:
                break;
            case Types.Byte:
                return view.getUint8(indexObj.index++);
        }
    }

    private static fromValue(value: any): number[] {
        let s = typeof(value);

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

    }

    static toBytes(param: any) {
        var bytes = [];
        bytes.push(param.messageType);

        for (let property in param) {
            if (param.hasOwnProperty(property) && property !== 'messageType') {
                var value = param[property];
                bytes.push(...this.fromValue(value))
            }
        }

        return bytes;
    }
}
enum SocketMessageType
{
    CreateNewGameRequest,
    AskQuestion,
    AnswerQuestion,
    GameOver,
    GameStarted,
}

enum Types
{
    String = 0, ArrayOfString = 1, Short = 2, ArrayOfInt = 3,
    Byte = 4
}


module.controller('mainCtrl', function ($scope) {
    $scope.model = {};
    $scope.callback = {};
    $scope.model.gameInfos = [];
    $scope.model.totalGames = 0;
    var getGateway = () => {
        fetch('http://127.0.0.1:3579/api/gateway').then((resp) => {
            return resp.json();
        }).then(a => {
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
            var bytes = Serializer.toBytes({messageType: SocketMessageType.CreateNewGameRequest, gamepad: 'sevens'});
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
        })
});