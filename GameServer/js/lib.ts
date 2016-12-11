declare let shuff: IShuff;

export class Pile {
    cards: Card[];
    name: string;

    constructor(name: string) {
        this.cards = [];
    }

    sortCards() {
    }

    reverseCards() {
    }
}
export class TableSpace {
    constructor(options: { name: string }) {
        this.name = options.name;
    }

    assignPile(pile: Pile): TableSpace {
        this.pile = pile;
        return this;
    }

    assignUser(user: User): TableSpace {
        this.user = user;
        return this;
    }

    effects: string[];
    user: User;
    pile: Pile;
    name: string;
}
export class TableText {
    effects: string[];
    name: string;
    text: string;

    constructor(options: { name: string }) {
        this.name = options.name;
    }

}
export class Card {
    constructor(value: number, type: number) {
        this.value = value;
        this.type = type;
    }

    state: CardState;
    type: number;
    value: number;
    effects: string[];
}
export class User {
    constructor(userName: string) {
        this.userName = userName;
        this.id = ((Math.random() * 100000) | 0).toString()
        this.cards = new Pile(userName);
    }

    cards: Pile;
    id: string;
    userName: string;

}
export class CardGame {
    users: User[];
    spaces: TableSpace[];
    texts: TableText[];
    deck: Pile;
    effects: string[];
    winner: User;

    constructor() {
        //        this.emulatedAnswers = [];
        this.spaces = [];
        this.texts = [];
        this.users = [];
        this.effects = [];
        this.deck = new Pile("deck");
    }

    init() {
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

        for (var user of this.users) {
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

    }

    questionAnswered: (answerIndex: number) => void;
    declareWinner(user: User) {
        this.winner = user;
    }

    getSpaceByName(name: string): TableSpace {
        for (let space of this.spaces) {
            if (space.name === name) {
                return space;
            }
        }
        return null;
    }

    getTextByName(name: string): TableText {
        for (let text of this.texts) {
            if (text.name === name) {
                return text;
            }
        }
        return null;
    }

    askQuestion(user: User, question: string, answers: string[]): Promise<number> {
        return new Promise((resolve, reject) => {
            shuff.questionAsked(user.userName, question, answers, resolve);
        });
    }

    numberOfCards: number;
    numberOfJokers;
}
export class _ {
    static cloneArray<T>(arr: T[]): T[] {
        return arr.map(a => a);
    }

    static floor(val: number): number {
        return val | 0;
    }

    static random(top: number): number {
        return (Math.random() * top) | 0;
    }

    static numbers(min: number, max: number): number[] {
        var numbers = [];
        for (var i = min; i < max; i++) {
            numbers.push(i);
        }
        return numbers;
    }

    static remove<T>(arr: T[], item: T): T[] {
        arr.splice(arr.indexOf(item), 1);
        return arr;
    }
}
export enum CardState {
    FaceUp = 0,
    FaceDown = 1,
    FaceUpIfOwned = 2
}
export interface IShuff {
    error(error: string);
    log(error: string);
    setWinner(winner: User);
    questionAsked(userName: string, question: string, answers: string[], resolve: (answerIndex) => void);
}
