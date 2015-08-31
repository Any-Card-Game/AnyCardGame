using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Redis;
using Common.Redis.RedisMessages;
using GameServer.CardGameLibrary;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.String;
using Jint.Runtime;
using Jint.Runtime.Debugger;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace GameServer
{
    public class GameManager
    {
        public GameManager(string gameId)
        {

            GameId = gameId;
        }

        public string GameId { get; set; }
        public Thread Thread { get; set; }
        public int Answer { get; set; }
        public CreateNewGameRequest InitialRequest { get; set; }
    }
    class GameServerProgram
    {
        private static Dictionary<string, GameManager> games = new Dictionary<string, GameManager>();
        private static RedisClient client;
        static string gameServerKey = Guid.NewGuid().ToString("N");

        static void Main(string[] args)
        {

            Console.WriteLine("Game Server Key " + gameServerKey);


            client = new RedisClient();
            client.Subscribe(RedisChannels.CreateNewGameRequest, request =>
            {

                var gameId = Guid.NewGuid().ToString("N");
                var createNewGameRequest = (CreateNewGameRequest)request;
                GameManager gameManager = new GameManager(gameId) {InitialRequest = createNewGameRequest};

                games.Add(gameId, runThing(gameManager));

                client.SendMessage("GameUpdate" + createNewGameRequest.GatewayKey, new GameUpdateRedisMessage()
                {
                    GameId = gameId,
                    UserKey = createNewGameRequest.UserKey,
                    GameServer = gameServerKey,
                    GameStatus = GameStatus.Started
                });
            });


            client.Subscribe("GameServer" + gameServerKey, request =>
             {
                 var gameServerResponse = (GameServerRedisMessage)request;
                 games[gameServerResponse.GameId].Answer = gameServerResponse.AnswerIndex;
                 games[gameServerResponse.GameId].Thread.Resume();
             });




            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        private static int answers = 0;
        private static int GamesDone = 0;
        private static GameManager runThing(GameManager gameManager)
        {
            BackgroundWorker thread = null;
            thread = new BackgroundWorker();
            thread.WorkerReportsProgress = true;
            int questionIndex = 0;
            thread.ProgressChanged += (a, b) =>
            {
                if (b.ProgressPercentage == 0)
                {
                    answers++;
                    //                                    Console.WriteLine("GAME INDEX::::" + gameId+"::::::"+ questionIndex++);
                    var questionc = (CardGameQuestion)b.UserState;

                    client.SendMessage("GameUpdate" + gameManager.InitialRequest.GatewayKey, new GameUpdateRedisMessage()
                    {
                        GameServer = gameServerKey,
                        GameId = gameManager.GameId,
                        UserKey = gameManager.InitialRequest.UserKey,
                        Question = new CardGameQuestionTransport()
                        {
                            User = questionc.User.UserName,
                            Question = questionc.Question,
                            Answers = questionc.Answers,
                        },
                        GameStatus = GameStatus.AskQuestion
                    });



                }
                else if (b.ProgressPercentage == 1)
                {
                    GamesDone++;
                    Console.WriteLine("GAME INDEX::::" + gameManager.GameId + "::::::");
                    var userc = (CardGameUser)b.UserState;
                    var user = userc;

                    Console.WriteLine(user.UserName + " Has won!");
                }
            };


            thread.DoWork += (t, cc) =>
            {
                gameManager.Thread = Thread.CurrentThread;
                Console.WriteLine("Started " + gameManager.GameId);
                Jint.Engine engine = new Jint.Engine();
                engine.SetValue("log", new Action<object>(a => { Console.WriteLine(a); }));


                engine.SetValue("CardGame", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGame)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));

                engine.SetValue("TableTextArea", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGameTextArea)));
                engine.SetValue("TableSpace", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGameTableSpace)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));
                engine.SetValue("GameUtils", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameUtils)));
                engine.SetValue("Shuff", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.Shuff)));

                try
                {
                    var sevens = File.ReadAllText("js/sevens.js");
                    var c = executeES6(engine, sevens);
                    c = c.Execute("var cg=new CardGame()");

                    c.GetValue("cg").AsObject().Get("__init__").Invoke(c.GetValue("cg"), new JsValue[] { 12 });

                    c = c.Execute("var _=new GameUtils()");
                    c = c.Execute("var shuff=new Shuff()");
                    c.GetValue("shuff").AsObject().Get("SetDelegates").Invoke(c.GetValue("shuff"), new JsValue[]
                    {
                        JsValue.FromObject(engine, new CardGameDelegates()
                        {
                            AskQuestionCallback = question => askQuestion(thread, gameManager, question),
                            DeclareWinnerCallback = user => declareWinner(thread, user)
                        })
                    });
                    c = c.Execute("var sevens=new Sevens()");

                    c = c.Execute("sevens.constructor(cg);");
                    Console.WriteLine("Running game " + gameManager.GameId);

                    c = c.Execute("sevens.runGame(cg);");
                }
                catch (Exception exc)
                {
                    if (exc.InnerException is JavaScriptException)
                    {
                        var location = engine.GetLastSyntaxNode().Location;
                        var javaScriptException = (exc.InnerException as JavaScriptException);
                        throw new ApplicationException($"{javaScriptException.Error} " +
                                                       $"({location.Source}: Line {location.Start.Line}, Column {location.Start.Column} to Line {location.End.Line}, Column {location.End.Column})", exc);
                    }
                }
            };

            thread.RunWorkerAsync();
            return gameManager;
        }

        private static void declareWinner(BackgroundWorker thread, CardGameUser user)
        {
            thread.ReportProgress(1, user);
            Console.WriteLine("Game over");
            Thread.CurrentThread.Abort();
            Console.WriteLine("Should never get here");
        }

        private static CardGameAnswer askQuestion(BackgroundWorker thread, GameManager gameResponse, CardGameQuestion question)
        {
            thread.ReportProgress(0, question);
            Thread.CurrentThread.Suspend();
            return new CardGameAnswer() { Value = gameResponse.Answer };
        }

        private static Engine executeES6(Engine engine, string text)
        {
            /*
                        var typescript = File.ReadAllText("js/typescript/typescript.js");
                        var mc = engine.Execute(typescript);
                        var tsc = mc.GetValue("ts");
                        var cc = tsc.AsObject().Get("transpile");

                        var c = cc.Invoke(text).AsString();

                        var mcc = engine.Execute(c);
            */
            var mcc = engine.Execute(text);
            return mcc;
        }
    }

}
