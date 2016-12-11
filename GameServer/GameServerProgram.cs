using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public int Answer { get; set; }
        public CreateNewGameRequest InitialRequest { get; set; }
        public GameServerProgram.DataClass DataClass { get; set; }
    }

    public class GameServerProgram
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
                GameManager gameManager = new GameManager(gameId) { InitialRequest = createNewGameRequest };

                games.Add(gameId, gameManager);
                Console.WriteLine("New Game Request " + games.Count);

                client.SendMessage("GameUpdate" + createNewGameRequest.GatewayKey, new GameUpdateRedisMessage()
                {
                    GameId = gameId,
                    UserKey = createNewGameRequest.UserKey,
                    GameServer = gameServerKey,
                    GameStatus = GameStatus.Started
                });
                startGame(gameManager);
            });


            client.Subscribe("GameServer" + gameServerKey, request =>
             {
                 var gameServerResponse = (GameServerRedisMessage)request;
                 games[gameServerResponse.GameId].DataClass.curAnswered(gameServerResponse.AnswerIndex);
             });




            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        private static int answers = 0;
        private static int GamesDone = 0;
        public class DataClass
        {
            public GameManager GameManager { get; set; }

            public DataClass(GameManager gameManager)
            {
                GameManager = gameManager;
                GameManager.DataClass = this;
            }

            public Action<int> curAnswered;

            public void questionAsked(string username, string question, string[] answers, Action<int> a)
            {
                curAnswered = a;
                client.SendMessage("GameUpdate" + GameManager.InitialRequest.GatewayKey, new GameUpdateRedisMessage()
                {
                    GameServer = gameServerKey,
                    GameId = GameManager.GameId,
                    UserKey = GameManager.InitialRequest.UserKey,
                    Question = new CardGameQuestionTransport()
                    {
                        User = username,
                        Question = question,
                        Answers = answers,
                    },
                    GameStatus = GameStatus.AskQuestion
                });

                Console.WriteLine(username + " " + question + " " + string.Join(",", answers));
            }
            public void setWinner(object username)
            {
                Console.WriteLine(username);
                GamesDone++;
                //                                    Console.WriteLine("GAME INDEX::::" + gameId+"::::::"+ questionIndex++);

                client.SendMessage("GameUpdate" + GameManager.InitialRequest.GatewayKey, new GameUpdateRedisMessage()
                {
                    GameServer = gameServerKey,
                    GameId = GameManager.GameId,
                    UserKey = GameManager.InitialRequest.UserKey,
                    GameStatus = GameStatus.GameOver
                });
                games.Remove(GameManager.GameId);
                Console.WriteLine("0Game over " + games.Count);

                //                    Console.WriteLine(userc.UserName + " Has won!");

            }
            public void log(string a)
            {
                Console.WriteLine("--" + a);
            }

        }


        private static void startGame(GameManager gameManager)
        {
            Engine engine = new Engine();
            var promise = getFile(@"./js/promise.js");
            var sevens = getFile(@"./js/sevens.js");

            var dataClass = new DataClass(gameManager);
            engine.SetValue("shuff", dataClass);
            engine.SetValue("exports", new { });
            engine.SetValue("require", new Func<string, JsValue>((file) =>
             {
                 var txt = getFile($@"./js/{file}.js");
                 engine.SetValue("shuff", dataClass);
                 engine.Execute("var exports={};"+txt);
                 return engine.GetValue("exports");
             }));

            engine.Execute(promise + "; " + sevens);
            engine.Execute("Main.run()");
        }


        static Dictionary<string,string> files=new Dictionary<string, string>();
        private static string getFile(string file)
        {
            if (files.ContainsKey(file))
            {
                return files[file];
            }
            files[file] = File.ReadAllText(file);
            return files[file];
        }

    }


}
