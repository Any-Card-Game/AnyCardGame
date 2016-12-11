"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t;
    return { next: verb(0), "throw": verb(1), "return": verb(2) };
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};


var lib_1 = require("./lib");
var Main = (function () {
    function Main() {
    }
    Main.run = function () {
        var cardGame = new lib_1.CardGame();
        cardGame.init();
        var sevens = new Sevens(cardGame);
        sevens.runGame().then(function () {
            shuff.setWinner(cardGame.winner);
        }, function (ex) {
            shuff.error(ex);
        });
    };
    return Main;
}());
exports.Main = Main;
var Sevens = (function () {
    function Sevens(cardGame) {
        this.cardGame = cardGame;
        this.spades = new lib_1.Pile('spades');
        this.clubs = new lib_1.Pile('clubs');
        this.hearts = new lib_1.Pile('hearts');
        this.diamonds = new lib_1.Pile('diamonds');
        this.cardGame.getSpaceByName('clubs').assignPile(this.clubs);
        this.cardGame.getSpaceByName('spades').assignPile(this.spades);
        this.cardGame.getSpaceByName('hearts').assignPile(this.hearts);
        this.cardGame.getSpaceByName('diamonds').assignPile(this.diamonds);
    }
    Sevens.prototype.runGame = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            var _i, _a, num, _b, _c, user, spaceByName, assignPile, _d, _e, user, _f, _g, user, cardTypes, cardNames, _h, _j, user, usable, answers, _k, usable_1, card, _l, _m, space, _o, _p, card, j, m, answerIndex, usableCard, _q, _r, space_1;
            return __generator(this, function (_s) {
                switch (_s.label) {
                    case 0:
                        for (_i = 0, _a = lib_1._.numbers(1, 20); _i < _a.length; _i++) {
                            num = _a[_i];
                            this.cardGame.deck.cards = this.shuffle(this.cardGame.deck.cards);
                        }
                        for (_b = 0, _c = this.cardGame.users; _b < _c.length; _b++) {
                            user = _c[_b];
                            spaceByName = this.cardGame.getSpaceByName('User' + user.id);
                            assignPile = spaceByName.assignPile(user.cards);
                            assignPile.assignUser(user);
                            this.cardGame.getTextByName('User' + user.id).text = user.userName;
                        }
                        while (this.cardGame.deck.cards.length > 0) {
                            for (_d = 0, _e = this.cardGame.users; _d < _e.length; _d++) {
                                user = _e[_d];
                                if (this.cardGame.deck.cards.length > 0) {
                                    this.cardGame.deck.cards[0].state = lib_1.CardState.FaceUpIfOwned;
                                    user.cards.cards.push(this.cardGame.deck.cards[0]);
                                    this.cardGame.deck.cards.splice(this.cardGame.deck.cards.indexOf(this.cardGame.deck.cards[0]), 1);
                                }
                            }
                        }
                        for (_f = 0, _g = this.cardGame.users; _f < _g.length; _f++) {
                            user = _g[_f];
                            user.cards.sortCards();
                        }
                        cardTypes = ['Diamonds', 'Clubs', 'Hearts', 'Spades'];
                        cardNames = ['Ace', 'Deuce', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Jack', 'Queen', 'King'];
                        _s.label = 1;
                    case 1:
                        if (!true)
                            return [3 /*break*/, 6];
                        _h = 0, _j = this.cardGame.users;
                        _s.label = 2;
                    case 2:
                        if (!(_h < _j.length))
                            return [3 /*break*/, 5];
                        user = _j[_h];
                        usable = user.cards.cards.filter(function (c) {
                            return (c.type === 1 && (c.value === 6 || _this.clubs.cards.filter(function (dCard) { return dCard.value === c.value + 1 || dCard.value === c.value - 1; }).length > 0)) ||
                                (c.type === 2 && (c.value === 6 || _this.hearts.cards.filter(function (dCard) { return dCard.value === c.value + 1 || dCard.value === c.value - 1; }).length > 0)) ||
                                (c.type === 3 && (c.value === 6 || _this.spades.cards.filter(function (dCard) { return dCard.value === c.value + 1 || dCard.value === c.value - 1; }).length > 0)) ||
                                (c.type === 0 && (c.value === 6 || _this.diamonds.cards.filter(function (dCard) { return dCard.value === c.value + 1 || dCard.value === c.value - 1; }).length > 0));
                        });
                        answers = [];
                        answers.push('Skip');
                        for (_k = 0, usable_1 = usable; _k < usable_1.length; _k++) {
                            card = usable_1[_k];
                            answers.push(cardNames[card.value] + ' Of ' + cardTypes[card.type]);
                        }
                        for (_l = 0, _m = this.cardGame.spaces; _l < _m.length; _l++) {
                            space = _m[_l];
                            space.effects = [];
                            if (space.user === user) {
                                if (usable.length === 0) {
                                    space.effects.push("CurrentPlayerNoCards");
                                }
                                else {
                                    space.effects.push("CurrentPlayer");
                                }
                            }
                            else if (space.user) {
                                space.effects.push("InactivePlayer");
                            }
                            if (!space.user) {
                                space.effects.push("CenterPiles");
                            }
                            else {
                                space.effects.push("Bend");
                            }
                            for (_o = 0, _p = space.pile.cards; _o < _p.length; _o++) {
                                card = _p[_o];
                                card.effects = [];
                                if (card.value === 6 && !space.user) {
                                    card.effects.push("Seven");
                                }
                                for (j = 0; j < usable.length; j++) {
                                    m = usable[j];
                                    if (m.value == card.value && m.type == card.type) {
                                        card.effects.push("PlayableCard");
                                        break;
                                    }
                                }
                            }
                        }
                        return [4 /*yield*/, this.cardGame.askQuestion(user, 'Which card would you like to play?', answers)];
                    case 3:
                        answerIndex = _s.sent();
                        //this.cardGame.log('asked question: ' + de);
                        if (answerIndex > 0 && usable.length >= answerIndex) {
                            usableCard = usable[answerIndex - 1];
                            usableCard.state = lib_1.CardState.FaceUp;
                            lib_1._.remove(user.cards.cards, usableCard);
                            switch (usableCard.type) {
                                case 3:
                                    this.spades.cards.push(usableCard);
                                    this.spades.sortCards();
                                    this.spades.reverseCards();
                                    break;
                                case 1:
                                    this.clubs.cards.push(usableCard);
                                    this.clubs.sortCards();
                                    this.clubs.reverseCards();
                                    break;
                                case 2:
                                    this.hearts.cards.push(usableCard);
                                    this.hearts.sortCards();
                                    this.hearts.reverseCards();
                                    break;
                                case 0:
                                    this.diamonds.cards.push(usableCard);
                                    this.diamonds.sortCards();
                                    this.diamonds.reverseCards();
                                    break;
                            }
                            if (user.cards.cards.length === 0) {
                                for (_q = 0, _r = this.cardGame.spaces; _q < _r.length; _q++) {
                                    space_1 = _r[_q];
                                    if (space_1.user === user) {
                                        space_1.effects.push("PlayerWon");
                                        break;
                                    }
                                }
                                this.cardGame.declareWinner(user);
                                return [2 /*return*/];
                            }
                        }
                        _s.label = 4;
                    case 4:
                        _h++;
                        return [3 /*break*/, 2];
                    case 5: return [3 /*break*/, 1];
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    Sevens.prototype.shuffle = function (cards) {
        var index = 0;
        var clonedCards = lib_1._.cloneArray(cards);
        for (var _i = 0, clonedCards_1 = clonedCards; _i < clonedCards_1.length; _i++) {
            var fs = clonedCards_1[_i];
            var vm = lib_1._.floor(lib_1._.random(clonedCards.length));
            clonedCards[index] = clonedCards[vm];
            index++;
            clonedCards[vm] = fs;
        }
        cards = clonedCards;
        return cards;
    };
    return Sevens;
}());
exports.Sevens = Sevens;
