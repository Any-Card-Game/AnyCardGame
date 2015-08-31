using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Data;
using Jint.Runtime.Interop;

namespace GameServer.CardGameLibrary
{
    public delegate CardGameAnswer AskQuestionDelegate(CardGameQuestion question);

    public class CardGameDelegates
    {
        public AskQuestionDelegate AskQuestionCallback { get; set; }

    }
    public   class Shuff
    {
        private CardGameDelegates cardGameDelegates;
        public void SetDelegates(CardGameDelegates t)
        {
            cardGameDelegates = t;
        }
        public   int AskQuestion(CardGameUser user, string question, string[] answers, GameCardGame cardGame)
        {
            cardGame.Emulating = false;
            if (cardGame.EmulatedAnswers.Count - 1 > cardGame.EmulatedAnswerIndex)
            {
                cardGame.Emulating = true;
                return cardGame.EmulatedAnswers[cardGame.EmulatedAnswerIndex++].Value; //todo .value
            }

            var m = new CardGameQuestion(user, question, answers, cardGame);
            var answer= cardGameDelegates.AskQuestionCallback(m);
            cardGame.EmulatedAnswerIndex++;
            return answer == null ? 0 : answer.Value;

        }

        public   void DeclareWinner(CardGameUser user)
        {
//            Fiber<FiberYieldResponse>.Yield(new FiberYieldResponse(FiberYieldResponseType.GameOver));
        }
        public   void GameOver()
        {
//            Fiber<FiberYieldResponse>.Yield(new FiberYieldResponse(FiberYieldResponseType.GameOver));
        }
        public   void PlayersLeave(Action<List<CardGameUser>> usersLeft)
        {

//            var users = Fiber<List<UserLogicModel>>.Yield(new FiberYieldResponse(FiberYieldResponseType.PlayersLeft));

//            if (users.Count > 0)
//            {
//                usersLeft(users);
//            }

        }

        public   void Log(string msg)
        {
//            Fiber<FiberYieldResponse>.Yield(new FiberYieldResponse(FiberYieldResponseType.Log, msg));
        }
      

        public void Break(BreakInfoObject breakInfo, GameCardGame cardGame, Func<string, string> varLookup)
        {
            /*   if (cardGame.Emulating)
                   return;*/

            if (cardGame != null && cardGame.DebugInfo != null)
            {
                if (cardGame.DebugInfo.Breakpoints.Contains(breakInfo.Line) || (!!(dynamic)cardGame.DebugInfo.StepThrough && cardGame.DebugInfo.StepThrough != StepType.Continue))
                {
                    if (cardGame.DebugInfo.LastBrokenLine == breakInfo.Line) return;
                    cardGame.DebugInfo.LastBrokenLine = breakInfo.Line;

                    var wasLastBreakLast = cardGame.DebugInfo.LastWasEndOfFunction;
                    var wasLastIndexBreakLast = cardGame.DebugInfo.LastWasEndOfFunctionIndex;
                    cardGame.DebugInfo.LastWasEndOfFunction = breakInfo.IsLast;
                    if (cardGame.DebugInfo.LastWasEndOfFunction)
                    {
                        cardGame.DebugInfo.LastWasEndOfFunctionIndex = breakInfo.Funcdef;
                    }

                    //step out
                    // if step into/over 
                    //  if end of function is reached (determined at parse time) then
                    //    allow lastfunc to != funcdef
                    // if step out
                    //  wait until end of function, trigger next debug?

                    switch (cardGame.DebugInfo.StepThrough)
                    {
                        case StepType.Into:
                            if (cardGame.DebugInfo.LastFunction == breakInfo.Funcdef)
                            {
                                Console.WriteLine("step over happened " + cardGame.DebugInfo.LastFunction + " == " + breakInfo.Funcdef);
                            }
                            else if (cardGame.DebugInfo.LastFunction != breakInfo.Funcdef)
                            {
                                Console.WriteLine("step into happened " + cardGame.DebugInfo.LastFunction + " != " + breakInfo.Funcdef);
                            }
                            break;
                        case StepType.Out:
                            if (cardGame.DebugInfo.LastFunction != breakInfo.Funcdef)
                            {
                                Console.WriteLine("step over/out skipped " + cardGame.DebugInfo.LastFunction + " != " + breakInfo.Funcdef);
                                return;
                            }
                            Console.WriteLine("step over/out okay");
                            break;
                        case StepType.Over:
                            if (cardGame.DebugInfo.LastFunction != breakInfo.Funcdef)
                            {
                                Console.WriteLine("step over skipped " + cardGame.DebugInfo.LastFunction + " != " + breakInfo.Funcdef);

                                if (!wasLastBreakLast)
                                {
                                    Console.WriteLine("wasnt last line, gonna skip ");
                                    return;
                                }
                                if (wasLastIndexBreakLast != breakInfo.Funcdef)
                                {
                                    Console.WriteLine("wasnt idk line, gonna skip ");
                                    return;
                                }
                                Console.WriteLine("was last line, gonna continue");
                            }
                            Console.WriteLine("step over/out okay");
                            break;
                        case StepType.Lookup:
                            Console.WriteLine("Lookup");
                            break;
                        default:
                            Console.WriteLine("idk step " + cardGame.DebugInfo.StepThrough);
                            break;
                    }

                    cardGame.DebugInfo.LastFunction = breakInfo.Funcdef;

                    /*
                                        var yieldObject = new FiberYieldResponse(FiberYieldResponseType.Break, breakInfo.Line, "");
                                        while (true)
                                        {
                                            Console.WriteLine("breaking");
                                            var answ = Fiber<FiberYieldResponse>.Yield(yieldObject);

                                            if (answ == null)
                                            {
                                                //continue
                                                return;
                                            }
                                            if (answ.VariableLookup != null)
                                            {
                                                object lookup;
                                                try
                                                {
                                                    lookup = varLookup(answ.VariableLookup);
                                                }
                                                catch (Exception e)
                                                {
                                                    lookup = e.Message;
                                                }
                                                yieldObject = new FiberYieldResponse(FiberYieldResponseType.VariableLookup, breakInfo.Line, Json.Stringify(lookup));
                                                continue;
                                            }
                                            break;
                                        }
                    */
                }
            }

        }
    }
    public class BreakInfoObject
    {
        public bool IsLast { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }
        public int Funcdef { get; set; }
    }
    public class CardGameQuestion
    {
        public CardGameUser User { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public GameCardGame CardGame { get; set; }

        public CardGameQuestion(CardGameUser user, string question, string[] answers, GameCardGame cardGame)
        {
            User = user;
            Question = question;
            Answers = answers;
            CardGame = cardGame;
        }
    }


    public class GameUtils
    {
        public int[] Numbers(int start, int finish)
        {
            var items = new int[finish - start+1];

            for (var i = 0; i <= finish - start; i++)
            {
                items[i] = start + i;
            }
            return items;
        }
        
        public object[] CloneArray(IEnumerable<object> obj)
        {
            var arr = obj.ToArray();
            object[] cc = new object[arr.Length];
            Array.Copy(arr, cc, arr.Length);
            return cc;
        }

        public int Floor(double j)
        {
            return (int)j;
        }
        Random random = new Random(Guid.NewGuid().ToByteArray()[0]);//todo better random
        public double Random()
        {
            return random.NextDouble();
        }
    }


    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Size() { }
    }

    public class CardGameAnswer
    {
        public int Value { get; set; }
    }
    public class CardGameUser
    {

        public string UserName { get; set; }

        public int PlayerDealingOrder { get; set; }

        public CardGamePile Cards { get; set; }

        public CardGameUser(string name)
        {
            UserName = name;
            Cards = new CardGamePile(name);
        }
    }
    public class CardGamePile
    {

        public string Name { get; set; }

        public JsList<CardGameCard> Cards { get; set; }

        public CardGamePile(string name)
        {
            Name = name;
            Cards = new JsList<CardGameCard>();
        }
        
        public void ReverseCards()
        {
            this.Cards.Reverse();
        }
        public void SortCards()
        {

            var ijc = this.Cards.GroupBy(a => a.Type);
            var ij = ijc.Select(
                            a =>
                            {
                                return a.OrderBy(b => b.Value);
                            });

            JsList<CardGameCard> items = new JsList<CardGameCard>();
            int jf = 0;
            items.AddRange(ij.SelectMany(cardGameCard => cardGameCard));
            Cards = items;
        }

        private Random random = new Random();

        public void Shuffle()
        {
            var o = Cards;
            CardGameCard x;
            for (int j, i = o.Count; i == 0; j = int.Parse((random.NextDouble() * i).ToString()), x = o[--i], o[i] = o[j], o[j] = x) ; //lol
            Cards = o;
        }
    }
    public enum CardGameCardState
    {
        FaceUp = 0,
        FaceDown = 1,
        FaceUpIfOwned = 2
    }
    public class CardGameCard
    {

        public int Value { get; set; }

        public int Type { get; set; }

        public Guid Guid { get; set; }

        public CardGameCardState State { get; set; }

        public JsList<string> Effects { get; set; }

        public CardGameCard(int value, int type)
        {
            Value = value;
            Type = type;
            Effects = new JsList<string>();
            Guid = Guid.NewGuid();

        }
    }

    public class GameCardGameTextArea
    {

        public string Name { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public string Text { get; set; }

        public GameCardGameTextArea(GameCardGameTextAreaOptions options)
        {
            Name = options.Name ?? "Text Area";
            X = options.X == 0 ? 0 : options.X;
            Y = options.Y == 0 ? 0 : options.Y;
            Text = options.Text ?? "Text";
        }
    }

    public class GameCardGameTextAreaOptions
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Text { get; set; }
    }

    public enum TableSpaceResizeType
    {
        Grow,
        Static
    }
    public enum CardGameOrder
    {
        NoOrder = 0,
        Ascending = 1,
        Descending = 2
    }
    public class CardGameTableSpaceOptions
    {

        public bool Vertical { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public CardGamePile Pile { get; set; }

        public double Rotate { get; set; }

        public bool Visible { get; set; }

        public bool StackCards { get; set; }

        public string Name { get; set; }

        public CardGameOrder SortOrder { get; set; }

        public int NumerOfCardsHorizontal { get; set; }

        public int NumerOfCardsVertical { get; set; }

        public TableSpaceResizeType ResizeType { get; set; }

        public CardGameTableSpaceOptions()
        {
            ResizeType = TableSpaceResizeType.Grow;
            Rotate = 0;
        }
    }

    public class CardGameTableSpace
    {


        public bool Vertical { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public CardGamePile Pile { get; set; }

        public JsList<string> Effects { get; set; }

        public bool Visible { get; set; }

        public bool StackCards { get; set; }

        public string Name { get; set; }

        public CardGameOrder SortOrder { get; set; }

        public int NumberOfCardsHorizontal { get; set; }

        public int NumberOfCardsVertical { get; set; }

        public TableSpaceResizeType ResizeType { get; set; }

        public string PileName { get; set; }

        public string UserName { get; set; }

        public CardGameUser User { get; set; }



        public CardGameTableSpace(CardGameTableSpaceOptions options)
        {
            Vertical = !options.Vertical ? false : options.Vertical;
            X = options.X == 0 ? 0 : options.X;
            Y = options.Y == 0 ? 0 : options.Y;
            Name = options.Name ?? "TableSpace";
            Width = options.Width == 0 ? 0 : options.Width;
            Height = options.Height == 0 ? 0 : options.Height;
            //Rotate = options.Rotate == 0 ? 0 : options.Rotate;
            Visible = !options.Visible ? true : options.Visible;
            StackCards = !options.StackCards ? false : options.StackCards;
            Pile = new CardGamePile(Name + "Pile");
            SortOrder = options.SortOrder;
            NumberOfCardsHorizontal = options.NumerOfCardsHorizontal == 0 ? 1 : options.NumerOfCardsHorizontal;
            NumberOfCardsVertical = options.NumerOfCardsVertical == 0 ? 1 : options.NumerOfCardsVertical;
            ResizeType = options.ResizeType;
            //Rotate = ExtensionMethods.eval("options.rotate? options.rotate : 0");
            Effects = new JsList<string>();
        }

        public CardGameTableSpace AssignPile(CardGamePile pile)
        {
            Pile = pile;
            PileName = pile.Name;
            return this;
        }
        public CardGameTableSpace AssignUser(CardGameUser user)
        {
            User = user;
            UserName = user.UserName;
            return AssignPile(user.Cards);
        }

    }

    public class GameCardGame
    {

        public bool Emulating { get; set; }

        public string Name { get; set; }

        public int EmulatedAnswerIndex { get; set; }

        public JsList<CardGameTableSpace> Spaces { get; set; }

        public JsList<GameCardGameTextArea> TextAreas { get; set; }

        public Size Size { get; set; }

        public JsList<CardGameAnswer> EmulatedAnswers { get; set; }

        public JsList<CardGameUser> Users { get; set; }

        public CardGamePile Deck { get; set; }

        public int NumberOfCards { get; set; }

        public int NumberOfJokers { get; set; }

        public JsList<CardGameEffect> Effects { get; set; }


        public DebugInfo DebugInfo { get; set; }

        public GameCardGame()
        {
            Spaces = new JsList<CardGameTableSpace>();
            TextAreas = new JsList<GameCardGameTextArea>();
            EmulatedAnswers = new JsList<CardGameAnswer>();
            Users = new JsList<CardGameUser>();
            Effects = new JsList<CardGameEffect>();
            Deck = new CardGamePile("deck");
        }

        public void __init__(int c)
        {
            Spaces.Add(new CardGameTableSpace(new CardGameTableSpaceOptions()
            {
                Name = "clubs"
            }));
            Spaces.Add(new CardGameTableSpace(new CardGameTableSpaceOptions()
            {
                Name = "spades"
            }));
            Spaces.Add(new CardGameTableSpace(new CardGameTableSpaceOptions()
            {
                Name = "hearts"
            }));
            Spaces.Add(new CardGameTableSpace(new CardGameTableSpaceOptions()
            {
                Name = "diamonds"
            }));



            TextAreas.Add(new GameCardGameTextArea(new GameCardGameTextAreaOptions()
            {
                Name = "clubs"
            }));
            TextAreas.Add(new GameCardGameTextArea(new GameCardGameTextAreaOptions()
            {
                Name = "spades"
            }));
            TextAreas.Add(new GameCardGameTextArea(new GameCardGameTextAreaOptions()
            {
                Name = "hearts"
            }));
            TextAreas.Add(new GameCardGameTextArea(new GameCardGameTextAreaOptions()
            {
                Name = "diamonds"
            }));




            Users.Add(new CardGameUser("Joe"));
            Users.Add(new CardGameUser("Mike"));
            Users.Add(new CardGameUser("Chris"));
            Users.Add(new CardGameUser("Steve"));

            for (int index = 0; index < 6; index++)
            {
                Spaces.Add(new CardGameTableSpace(new CardGameTableSpaceOptions()
                {
                    Name = "User" + index
                }));
                TextAreas.Add(new GameCardGameTextArea(new GameCardGameTextAreaOptions()
                {
                    Name = "User" + index
                }));
            }

            Size = new Size(15, 15);

            NumberOfCards = 52;
            NumberOfJokers = 0;

            ConfigurationCompleted();


        }

        public void ConfigurationCompleted()
        {
            for (var i = 0; i < NumberOfCards; i++)
            {
                Deck.Cards.Add(new CardGameCard(i % 13, (int)Math.Floor(i / 13d)));
            }
            for (var i = 0; i < NumberOfJokers; i++)
            {
                Deck.Cards.Add(new CardGameCard(0, 0));
            }

        }

        public void SetEmulatedAnswers(JsList<CardGameAnswer> answers)
        {
            EmulatedAnswers = answers;
        }

        public void SetPlayers(JsList<MongoUser.User> players)
        {
            Users = new JsList<CardGameUser>();

            if (players == null || players.Count == 0)
                return;
            if (players.Count > 6)
                players.RemoveRange(6, players.Count - 6);
            for (var j = 0; j < players.Count; j++)
            {
                Users.Add(new CardGameUser(players[j].Email));
            }
        }
        public CardGameTableSpace GetSpaceByName(string name)
        {
            foreach (var cardGameTableSpace in Spaces)
            {
                if (cardGameTableSpace.Name.ToLower() == name.ToLower())
                {
                    return cardGameTableSpace;
                }
            }
            return null;
        }
        public GameCardGameTextArea GetTextByName(string name)
        {
            foreach (var gameCardGameTextArea in TextAreas)
            {
                if (gameCardGameTextArea.Name.ToLower() == name.ToLower())
                {
                    return gameCardGameTextArea;
                }
            }
            return null;
        }

        //arg0: cards per player
        //arg1: CardState
        //return undefined 
        public void DealCards(int numberOfCards, int state) { }

        public CardGameEffect GetEffectByName(string effectName)
        {
            foreach (var cardGameEffect in Effects)
            {
                if (cardGameEffect.Name.ToLower() == effectName.ToLower())
                {
                    return cardGameEffect;
                }
            }
            return null;
        }
    }

    public class DebugInfo
    {
        public JsList<int> Breakpoints { get; set; }
        public StepType StepThrough { get; set; }
        public int LastFunction { get; set; }
        public bool Action { get; set; }
        public int LastBrokenLine { get; set; }
        public bool LastWasEndOfFunction { get; set; }
        public int LastWasEndOfFunctionIndex { get; set; }
    }

    public class CreateDebugGameRequest
    {
        public int NumberOfPlayers { get; set; }
        public string GameName { get; set; }
        public JsList<int> Breakpoints { get; set; }


        public CreateDebugGameRequest(int numberOfPlayers, string gameName, JsList<int> breakpoints)
        {
            NumberOfPlayers = numberOfPlayers;
            GameName = gameName;
            Breakpoints = breakpoints;
        }
    }

    public class DestroyDebugGameRequest
    {
        public string RoomID { get; set; }


        public DestroyDebugGameRequest(string roomID)
        {
            RoomID = roomID;
        }
    }

    public class DebugResponse
    {
        public string RoomID { get; set; }
        public JsList<int> Breakpoints { get; set; }
        public StepType Step { get; set; }
        public bool Action { get; set; }
        public string VariableLookup { get; set; }


        public DebugResponse(string roomID, JsList<int> breakpoints, StepType step, bool action)
        {
            RoomID = roomID;
            Breakpoints = breakpoints;
            Step = step;
            Action = action;
        }
    }

    public enum StepType
    {
        Into, Over, Out, Continue, Lookup
    }
    public class CardGameEffect
    {

        public string Name { get; set; }

        public EffectType Type { get; set; }

        public JsList<CardGameEffectProperty> Properties { get; set; }

        public CardGameEffect(CardGameEffectOptions cardGameEffectOptions)
        {
            Name = cardGameEffectOptions.Name;
            Type = cardGameEffectOptions.Type;
            Properties = cardGameEffectOptions.Properties;
        }
    }

    public class CardGameEffectOptions
    {
        public string Name { get; set; }
        public EffectType Type { get; set; }
        public JsList<CardGameEffectProperty> Properties { get; set; }
    }

    public class CardGameEffectProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
    public enum EffectType
    {
        Highlight,
        Rotate,
        Bend,
        StyleProperty,
        Animated,
    }


    public class GameEffectPropertyModel
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public GameEffectPropertyType Type { get; set; }
    }
    public enum GameEffectPropertyType
    {
        Text, Number, Color
    }

}