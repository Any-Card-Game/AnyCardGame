using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alchemy;
using Alchemy.Classes;
using Common.Redis;
using Common.Redis.RedisMessages;
using Newtonsoft.Json;

namespace SocketServer
{
    class SocketServerProgram
    {
        private static RedisClient client;
        private static string GatewayKey;
        static void Main(string[] args)
        {
            string url = Utils.GetPublicIP() + ":81";
            Console.WriteLine(url);

            GatewayKey = Guid.NewGuid().ToString("N");
            Console.WriteLine("Gateway Key "+GatewayKey);
            client = new RedisClient();
            client.Subscribe("Gateway" + GatewayKey, request =>
              {



              });
            client.Subscribe(RedisChannels.GetNextGatewayRequest, request =>
            {
                client.SendMessage(RedisChannels.GetNextGatewayResponse, new NextGatewayResponseRedisMessage()
                {
                    Guid = request.Guid,
                    GatewayUrl = url
                });
            });
            client.Subscribe("GameUpdate" + GatewayKey, request =>
            {
                var rr = (GameUpdateRedisMessage)request;
                Console.Write("Got update "+request);
                users[rr.UserKey].GameData = rr;

                string str;
                switch (rr.GameStatus)
                {
                    case GameStatus.Started:
                        str = JsonConvert.SerializeObject(new GameStartedSocketMessage()
                            , new JsonSerializerSettings()
                            {
                                TypeNameHandling = TypeNameHandling.Objects
                            });
                        break;
                    case GameStatus.AskQuestion:
                        str = JsonConvert.SerializeObject(new AskQuestionSocketMessage()
                        {
                            Question = rr.Question.Question,
                            Answers = rr.Question.Answers,
                            User = rr.Question.User,
                        }, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Objects
                        });

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                users[rr.UserKey].UserContext.Send(str);


            });


            var aServer = new WebSocketServer(81, IPAddress.Any)
            {
                OnConnected = OnConnected,
                TimeOut = new TimeSpan(0, 5, 0)
            };

            aServer.Start();
            start = DateTime.Now;
            curDateTime = DateTime.Now;
            Console.ReadLine();
        }


        private static int count = 0;
        private static DateTime start;
        private static DateTime curDateTime;
        public static Dictionary<string, SocketUser> users = new Dictionary<string, SocketUser>();

        public class SocketUser
        {
            public UserContext UserContext { get; set; }
            public GameUpdateRedisMessage GameData { get; set; }
        }
        private static void OnConnected(UserContext context)
        {
            context.SetOnDisconnect(OnDisconnect);
            context.SetOnReceive(OnReceive);

            users.Add(context.UniqueKey, new SocketUser() { UserContext = context });

            Console.WriteLine("Client Connection From : " + context.ClientAddress.ToString());
        }

        private static void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client disconnected From : " + context.ClientAddress.ToString());
        }

        private static void OnReceive(UserContext context)
        {
            count++;
            if ((DateTime.Now) > (curDateTime.AddSeconds(1)))
            {
                curDateTime = DateTime.Now;
                Console.WriteLine(count / ((curDateTime - start).TotalSeconds));
            }


            SocketMessage obj = JsonConvert.DeserializeObject<SocketMessage>(context.DataFrame.ToString(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

            var uu = users[context.UniqueKey];
            if (obj is CreateNewGameRequestSocketMessage)
            {
                client.SendMessage(RedisChannels.CreateNewGameRequest, new CreateNewGameRequest()
                {
                    GatewayKey = GatewayKey,
                    UserKey = context.UniqueKey,
                    GameName = "sevens"
                });
            }
            else if (obj is AnswerQuestionSocketMessage)
            {
                client.SendMessage("GameServer" + uu.GameData.GameServer, new GameServerRedisMessage()
                {
                    GameId = uu.GameData.GameId,
                    AnswerIndex = ((AnswerQuestionSocketMessage)obj).AnswerIndex
                });
            }
            /*

                                    var str = JsonConvert.SerializeObject(new ThisRedditMessage(), new JsonSerializerSettings()
                                    {
                                        TypeNameHandling = TypeNameHandling.Objects
                                    });
                                    context.Send(str);*/
        }
    }


    public abstract class SocketMessage
    {
        protected SocketMessage()
        {
        }
    }

    public enum SocketChannels
    {
        CreateNewGameRequest,
        CreateNewGameResponse,
    }

    public class CreateNewGameRequestSocketMessage : SocketMessage
    {
        public string GameType { get; set; }
    }

    public class AskQuestionSocketMessage : SocketMessage
    {
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public string User { get; set; }
    }
    public class AnswerQuestionSocketMessage : SocketMessage
    {
        public int AnswerIndex { get; set; }
    }

    public class GameStartedSocketMessage : SocketMessage
    {
    }
}
