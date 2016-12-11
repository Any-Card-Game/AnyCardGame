import { CardGame, Pile, _, CardState, IShuff} from './lib';
declare let shuff: IShuff;

export class Main {
    static run() {
        let cardGame = new CardGame();
        cardGame.init();


        let sevens = new Sevens(cardGame);
        sevens.runGame().then(() => {
            shuff.setWinner(cardGame.winner);
        }, ex => {
            shuff.error(ex);
        });
    }
}


export class Sevens {
    spades: Pile;
    clubs: Pile;
    hearts: Pile;
    diamonds: Pile;
    cardGame: CardGame;

    constructor(cardGame: CardGame) {
        this.cardGame = cardGame;
        this.spades = new Pile('spades');
        this.clubs = new Pile('clubs');
        this.hearts = new Pile('hearts');
        this.diamonds = new Pile('diamonds');


        this.cardGame.getSpaceByName('clubs').assignPile(this.clubs);
        this.cardGame.getSpaceByName('spades').assignPile(this.spades);
        this.cardGame.getSpaceByName('hearts').assignPile(this.hearts);
        this.cardGame.getSpaceByName('diamonds').assignPile(this.diamonds);
    }

    async runGame(): Promise<void> {
        for (let num of _.numbers(1, 20)) {
            this.cardGame.deck.cards = this.shuffle(this.cardGame.deck.cards);
        }

        for (let user of this.cardGame.users) {
            let spaceByName = this.cardGame.getSpaceByName('User' + user.id);
            let assignPile = spaceByName.assignPile(user.cards);
            assignPile.assignUser(user);
            this.cardGame.getTextByName('User' + user.id).text = user.userName;
        }


        while (this.cardGame.deck.cards.length > 0) {
            for (let user of this.cardGame.users) {
                if (this.cardGame.deck.cards.length > 0) {
                    this.cardGame.deck.cards[0].state = CardState.FaceUpIfOwned;
                    user.cards.cards.push(this.cardGame.deck.cards[0]);
                    this.cardGame.deck.cards.splice(this.cardGame.deck.cards.indexOf(this.cardGame.deck.cards[0]), 1);
                }
            }
        }
        for (let user of this.cardGame.users) {
            user.cards.sortCards();
        }


        let cardTypes = ['Diamonds', 'Clubs', 'Hearts', 'Spades'];
        let cardNames = ['Ace', 'Deuce', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Jack', 'Queen', 'King'];

        while (true) {
            for (let user of this.cardGame.users) {
                let usable = user.cards.cards.filter((c) =>
                    (c.type === 1 && (c.value === 6 || this.clubs.cards.filter(dCard => dCard.value === c.value + 1 || dCard.value === c.value - 1).length > 0)) ||
                    (c.type === 2 && (c.value === 6 || this.hearts.cards.filter(dCard => dCard.value === c.value + 1 || dCard.value === c.value - 1).length > 0)) ||
                    (c.type === 3 && (c.value === 6 || this.spades.cards.filter(dCard => dCard.value === c.value + 1 || dCard.value === c.value - 1).length > 0)) ||
                    (c.type === 0 && (c.value === 6 || this.diamonds.cards.filter(dCard => dCard.value === c.value + 1 || dCard.value === c.value - 1).length > 0))
                );

                let answers = [];
                answers.push('Skip');

                for (let card of usable) {
                    answers.push(cardNames[card.value] + ' Of ' + cardTypes[card.type]);
                }

                for (let space of this.cardGame.spaces) {
                    space.effects = [];
                    if (space.user === user) {
                        if (usable.length === 0) {
                            space.effects.push("CurrentPlayerNoCards");
                        } else {

                            space.effects.push("CurrentPlayer");
                        }
                    } else if (space.user) {
                        space.effects.push("InactivePlayer");
                    }
                    if (!space.user) {
                        space.effects.push("CenterPiles");
                    } else {
                        space.effects.push("Bend");
                    }

                    for (let card of space.pile.cards) {
                        card.effects = [];
                        if (card.value === 6 && !space.user) {
                            card.effects.push("Seven");
                        }

                        for (let j = 0; j < usable.length; j++) {
                            let m = usable[j];
                            if (m.value == card.value && m.type == card.type) {
                                card.effects.push("PlayableCard");
                                break;
                            }
                        }
                    }
                }

                //this.cardGame.log('asking question');
                let answerIndex = await this.cardGame.askQuestion(user, 'Which card would you like to play?', answers);
                //this.cardGame.log('asked question: ' + de);

                if (answerIndex > 0 && usable.length >= answerIndex) {
                    let usableCard = usable[answerIndex - 1];
                    usableCard.state = CardState.FaceUp;

                    _.remove(user.cards.cards, usableCard);
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
                        for (let space of this.cardGame.spaces) {
                            if (space.user === user) {
                                space.effects.push("PlayerWon");
                                break;
                            }
                        }
                        this.cardGame.declareWinner(user);
                        return;
                    }
                }
            }
        }
    }


    shuffle(cards) {
        let index = 0;
        let clonedCards = _.cloneArray(cards);
        for (let fs of clonedCards) {
            let vm = _.floor(_.random(clonedCards.length));
            clonedCards[index] = clonedCards[vm];
            index++;
            clonedCards[vm] = fs;
        }
        cards = clonedCards;
        return cards;
    }
}
