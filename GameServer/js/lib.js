"use strict";
var Pile = (function () {
    function Pile(name) {
        this.cards = [];
    }
    Pile.prototype.sortCards = function () {
    };
    Pile.prototype.reverseCards = function () {
    };
    return Pile;
}());
exports.Pile = Pile;
var TableSpace = (function () {
    function TableSpace(options) {
        this.name = options.name;
    }
    TableSpace.prototype.assignPile = function (pile) {
        this.pile = pile;
        return this;
    };
    TableSpace.prototype.assignUser = function (user) {
        this.user = user;
        return this;
    };
    return TableSpace;
}());
exports.TableSpace = TableSpace;
var TableText = (function () {
    function TableText(options) {
        this.name = options.name;
    }
    return TableText;
}());
exports.TableText = TableText;
var Card = (function () {
    function Card(value, type) {
        this.value = value;
        this.type = type;
    }
    return Card;
}());
exports.Card = Card;
var User = (function () {
    function User(userName) {
        this.userName = userName;
        this.id = ((Math.random() * 100000) | 0).toString();
        this.cards = new Pile(userName);
    }
    return User;
}());
exports.User = User;
var CardGame = (function () {
    function CardGame() {
        //        this.emulatedAnswers = [];
        this.spaces = [];
        this.texts = [];
        this.users = [];
        this.effects = [];
        this.deck = new Pile("deck");
    }
    CardGame.prototype.init = function () {
        this.spaces.push(new TableSpace({ name: 'clubs' }));
        this.spaces.push(new TableSpace({ name: 'spades' }));
        this.spaces.push(new TableSpace({ name: 'hearts' }));
        this.spaces.push(new TableSpace({ name: 'diamonds' }));
        this.texts.push(new TableText({ name: 'clubs' }));
        this.texts.push(new TableText({ name: 'spades' }));
        this.texts.push(new TableText({ name: 'hearts' }));
        this.texts.push(new TableText({ name: 'diamonds' }));
        this.users.push(new User("Joe"));
        this.users.push(new User("Mike"));
        this.users.push(new User("Chris"));
        this.users.push(new User("Steve"));
        for (var _i = 0, _a = this.users; _i < _a.length; _i++) {
            var user = _a[_i];
            this.spaces.push(new TableSpace({ name: "User" + user.id }));
            this.texts.push(new TableText({ name: "User" + user.id }));
        }
        this.numberOfCards = 52;
        this.numberOfJokers = 0;
        for (var i = 0; i < this.numberOfCards; i++) {
            this.deck.cards.push(new Card(i % 13, _.floor(i / 13)));
        }
        for (var i = 0; i < this.numberOfJokers; i++) {
            this.deck.cards.push(new Card(0, 0));
        }
    };
    CardGame.prototype.declareWinner = function (user) {
        this.winner = user;
    };
    CardGame.prototype.getSpaceByName = function (name) {
        for (var _i = 0, _a = this.spaces; _i < _a.length; _i++) {
            var space = _a[_i];
            if (space.name === name) {
                return space;
            }
        }
        return null;
    };
    CardGame.prototype.getTextByName = function (name) {
        for (var _i = 0, _a = this.texts; _i < _a.length; _i++) {
            var text = _a[_i];
            if (text.name === name) {
                return text;
            }
        }
        return null;
    };
    CardGame.prototype.askQuestion = function (user, question, answers) {
        return new Promise(function (resolve, reject) {
            shuff.questionAsked(user.userName, question, answers, resolve);
        });
    };
    return CardGame;
}());
exports.CardGame = CardGame;
var _ = (function () {
    function _() {
    }
    _.cloneArray = function (arr) {
        return arr.map(function (a) { return a; });
    };
    _.floor = function (val) {
        return val | 0;
    };
    _.random = function (top) {
        return (Math.random() * top) | 0;
    };
    _.numbers = function (min, max) {
        var numbers = [];
        for (var i = min; i < max; i++) {
            numbers.push(i);
        }
        return numbers;
    };
    _.remove = function (arr, item) {
        arr.splice(arr.indexOf(item), 1);
        return arr;
    };
    return _;
}());
exports._ = _;
var CardState;
(function (CardState) {
    CardState[CardState["FaceUp"] = 0] = "FaceUp";
    CardState[CardState["FaceDown"] = 1] = "FaceDown";
    CardState[CardState["FaceUpIfOwned"] = 2] = "FaceUpIfOwned";
})(CardState = exports.CardState || (exports.CardState = {}));
