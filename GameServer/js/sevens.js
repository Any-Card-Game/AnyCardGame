function Sevens() {
    var self = this; 
    self.constructor = function (cardGame) {

        self.cardGame = cardGame;

        self.spades = new Pile('spades');
        self.clubs = new Pile('clubs');
        self.hearts = new Pile('hearts');
        self.diamonds = new Pile('diamonds');

        self.cardGame.getSpaceByName('clubs').assignPile(self.clubs);
        self.cardGame.getSpaceByName('spades').assignPile(self.spades);
        self.cardGame.getSpaceByName('hearts').assignPile(self.hearts);
        self.cardGame.getSpaceByName('diamonds').assignPile(self.diamonds);

    }; 

    self.runGame = function () {
        if (!self.cardGame.users || self.cardGame.users.length == 0) {
            log("baaad");
            return true;
        }

        _.numbers(1, 20).forEach(function () {
            self.cardGame.deck.cards = self.shuffle(self.cardGame.deck.cards);
        });

        self.cardGame.users.forEach(function (u, ind,o) {
            //shuff.log('::' + u.userName);
            var spaceByName = self.cardGame.getSpaceByName('User' + ind);
            var assignPile = spaceByName.assignPile(u.cards);
            assignPile.assignUser(u);
            self.cardGame.getTextByName('User' + ind).text = u.userName; 
        });

        log(self.cardGame.deck.cards.length);
        while (self.cardGame.deck.cards.length > 0) {
            self.cardGame.users.forEach(function(u) {
                if (self.cardGame.deck.cards.length > 0) {
                    self.cardGame.deck.cards[0].state = 2;
                    u.cards.cards.push(self.cardGame.deck.cards[0]);
                    self.cardGame.deck.cards.splice(self.cardGame.deck.cards.indexOf(self.cardGame.deck.cards[0]), 1);
                }
            });
        }

        self.cardGame.users.forEach(function(u) {
            u.cards.sortCards();
        });

        var CardTypes = ['Diamonds', 'Clubs', 'Hearts', 'Spades'];
        var CardNames = ['Ace', 'Deuce', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Jack', 'Queen', 'King'];

        while (true) {
            var result = self.cardGame.users.forEach(function(u) {

                var usable = u.cards.cards.where(function (c) {
                    return (c.type == 3 && (c.value == 6 || self.spades.cards.any(function(_c) {
                        return _c.value == c.value + 1 || _c.value == c.value - 1;
                    }))) ||
                    (c.type == 1 && (c.value == 6 || self.clubs.cards.any(function(_c) {
                        return _c.value == c.value + 1 || _c.value == c.value - 1;
                    }))) ||
                    (c.type == 2 && (c.value == 6 || self.hearts.cards.any(function(_c) {
                        return _c.value == c.value + 1 || _c.value == c.value - 1;
                    }))) ||
                    (c.type == 0 && (c.value == 6 || self.diamonds.cards.any(function(_c) {
                        return _c.value == c.value + 1 || _c.value == c.value - 1;
                    })));
                });

                var answers = [];
                answers.push('Skip');
                usable.forEach(function(card) {
                    answers.push(CardNames[card.value] + ' Of ' + CardTypes[card.type]);
                });

              
                var sp = self.cardGame.spaces;
                for (var i = 0; i < sp.length; i++) {
                    //sp[i].rotate += 10;

                    sp[i].effects = [];


                    if (sp[i].user == u) {
                        if (usable.length == 0) {
                            sp[i].effects.push("CurrentPlayerNoCards");
                        } else {

                            sp[i].effects.push("CurrentPlayer");
                        }
                    } else if (sp[i].user) {
                        sp[i].effects.push("InactivePlayer");

                    }


                    if (!sp[i].user) {
                        //25 + sp[i].pile.cards.length * 2
                        sp[i].effects.push("CenterPiles");
                    } else {
                        sp[i].effects.push("Bend");
                    }

                    for (var ij = 0; ij < sp[i].pile.cards.length; ij++) {
                        var card = sp[i].pile.cards[ij];
                        card.effects = [];
                        /*

                        card.appearance.addEffect(new AnimatedEffect$Between(
                            {
                                from: { outer: { border: { all: "solid 2px black" }, padding: { all: "32px" }, } },
                                to: { outer: { border: { all: "solid 10px black" }, padding: { all: "12px" }, } }
                            }, 1000, "linear"
                        )).chainEffect(new AnimatedEffect$Between(
                            {
                                from: { outer: { rotate: 45, } },
                                to: { outer: { rotate: 97 } }
                            }, 1000, "linear"
                        )).chainEffect(new AnimatedEffect$Between(
                            {
                                from: { outer: { backColor: "#FFF", padding: { all: "52px" }, } },
                                to: { outer: { backColor: "#DD1", padding: { all: "12px" }, } }
                            }, 1000, "linear"
                        )).chainEffect(new Effect$Highlight({
                                radius: 19,
                                color: 'rgba(1,1,1,0.22)',
                                opacity: .55
                            }
                        )).chainEffect(new Effect$StyleProperty({ outer: { backColor: "#11F" } })).chainEffect(new AnimatedEffect$Between(
                            {
                                from: { outer: { rotate: 45, } },
                                to: { outer: { rotate:97 } }
                            }, 1000, "linear"
                        ));

                        card.addAction(new Action$CardDraggable({ droppableSpaces: [sp[i]], droppableLocations: [{ x: 0, y: 0, width: 1, height: 1 }] }));
                            
                            
                            

                        if(card.value==6 && !sp[i].user) {
                        card.appearance.effects.push(new Effect$StyleProperty({outer:{border:{all:"solid 2px black"},padding:{all:"2px"},}}));
                             
                        }*/
                        //shuff.log(sp[i].user);
                        if (card.value == 6 && !sp[i].user) {

                            card.effects.push("Seven");
                        }

                        for (var j = 0; j < usable.length; j++) {
                            var m = usable[j];
                            if (m.value == card.value && m.type == card.type) {
                                card.effects.push("PlayableCard");
                                break;
                            }
                        }
                    }
                }
                //shuff.log('asking question');
                var de = shuff.askQuestion(u, 'Which card would you like to play?', answers, self.cardGame);
                //shuff.log('asked question: ' + de);

                if (de > 0 && usable.length >= de) {
                    var rm = usable[de - 1]; 
                    rm.state = 0;

                    switch (rm.type) {
                    case 3:
                        u.cards.cards.remove(rm);
                        self.spades.cards.push(rm);
                        self.spades.sortCards();
                        self.spades.reverseCards();
                        break;
                    case 1:
                        u.cards.cards.remove(rm);
                        self.clubs.cards.push(rm);
                        self.clubs.sortCards();
                        self.clubs.reverseCards();
                        break;
                    case 2:
                        u.cards.cards.remove(rm);
                        self.hearts.cards.push(rm);
                        self.hearts.sortCards();
                        self.hearts.reverseCards();
                        break;
                    case 0:
                        u.cards.cards.remove(rm);
                        self.diamonds.cards.push(rm);
                        self.diamonds.sortCards();
                        self.diamonds.reverseCards();
                        break;
                    }

                    if (u.cards.cards.length == 0) {

                        for (var i = 0; i < sp.length; i++) {

                            if (sp[i].user == u) {
                                sp[i].effects.push("PlayerWon");
                                break;
                            }
                        }
                        shuff.declareWinner(u);

                        return true;
                    }
                }
                return false;
            });
            if (result) {
                return true;
            }

        }


    };


    self.shuffle = function(arbs) {
        var indes = 0;
        var vafb = _.cloneArray(arbs);
        vafb.forEach(function(fs) {
            var vm = _.floor(_.random() * vafb.length);
            vafb[indes] = vafb[vm];
            indes++;
            vafb[vm] = fs;
        });
        arbs = vafb;
        return arbs;
    };
    return self;
};