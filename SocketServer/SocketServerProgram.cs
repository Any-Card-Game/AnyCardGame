using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Messages;
using Newtonsoft.Json;
using OnPoolClient;
using OnPoolCommon;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace SocketServer
{
    class SocketServerProgram
    {
        private static OnPoolClient.OnPoolClient client;


        static void Main(string[] args)
        {
            var threadManager = LocalThreadManager.Start();
            Random r = new Random();
            var port = r.Next(10000, 20000);
            string url = Utils.GetPublicIP() + ":" + port;
            Console.WriteLine(url);

            client = new OnPoolClient.OnPoolClient();
            client.OnReady(() =>
            {
                Console.WriteLine("Gateway Key " + client.MyClientId);

                client.OnMessage((from,message,respond) =>
                {
                    switch (message.Method)
                    {
                        case "GameUpdate":
                            var rr = message.GetJson<GameUpdateServerMessage>();
                            users[rr.UserKey].GameData = rr;
                            users[rr.UserKey].GameServer = from;
                            SocketMessage str;
                            switch (rr.GameStatus)
                            {
                                case GameStatus.Started:
                                    //                        Console.WriteLine("Started" + rr.GameId);
                                    str = new GameStartedSocketMessage();
                                    break;
                                case GameStatus.GameOver:
                                    //                        Console.WriteLine("Game Over"+rr.GameId);
                                    str = new GameOverSocketMessage();
                                    break;
                                case GameStatus.AskQuestion:
                                    //                        Console.WriteLine("Ask Question" + rr.GameId);
                                    str = new AskQuestionSocketMessage()
                                    {
                                        Question = rr.Question.Question,
                                        Answers = rr.Question.Answers,
                                        User = rr.Question.User,
                                    };

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            var bytes = Serializer.Serialize(str);

                            users[rr.UserKey].UserContext.SendMessage(bytes);
                            MessagesSent++;
                            break;
                    }
                });

                client.JoinPool("Gateways", (from, message, respond) =>
                {
                    switch (message.Method)
                    {
                        case "NextGateway":
                            respond(QueryParam.Json(new NextGatewayResponseServerMessage()
                            {
                                GatewayUrl = url
                            }));
                            break;
                    }
                });
                 
            });
            client.ConnectToServer("127.0.0.1");


            var wssv = new WebSocketServer("ws://" + url);
            wssv.AddWebSocketService<CardGame>("/");
            wssv.Start();

           /* timer = new Timer((e) =>
            {
                Console.WriteLine($"Users Connected: {UsersConnected} Messages Sent: {MessagesSent} Messages Received: {MessagesReceived}");
            }, null, 0, 500);*/

            threadManager.Process();


            Console.ReadLine();
        }

        public static int UsersConnected { get; set; }
        public static int MessagesSent { get; set; }
        public static int MessagesReceived { get; set; }

        public class CardGame : WebSocketBehavior
        {
            protected override void OnClose(CloseEventArgs e)
            {
                OnDisconnect(this);
            }

            protected override void OnError(ErrorEventArgs e)
            {
                base.OnError(e);
            }

            protected override void OnOpen()
            {
                OnConnected(this);
                base.OnOpen();
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                OnReceive(this, e.RawData);
            }

            public void SendMessage(byte[] str)
            {
                this.Send(str);
            }
        }


        public static Dictionary<string, SocketUser> users = new Dictionary<string, SocketUser>();
        private static object locker = new object();
        private static Timer timer;

        public class SocketUser
        {
            public GameUpdateServerMessage GameData { get; set; }
            public Client GameServer { get; set; }
            public CardGame UserContext { get; set; }
        }
        private static void OnConnected(CardGame user)
        {
            lock (locker)
            {
                users.Add(user.ID, new SocketUser() { UserContext = user });
                UsersConnected++;
            }
            //            Console.WriteLine("Client Connection From : " + user.Context.UserEndPoint.ToString());
        }

        private static void OnDisconnect(CardGame user)
        {
            lock (locker)
            {
                UsersConnected--;
            }
            //            Console.WriteLine("Client disconnected From : " + user.ID + "         " + Process.GetCurrentProcess().Threads.Count);
        }

        private static void OnReceive(CardGame user, byte[] frame)
        {
            MessagesReceived++;

            SocketMessage obj = Serializer.Deserialize(frame);

            SocketUser uu;
            lock (locker)
            {
                uu = users[user.ID];
            }
            if (obj is CreateNewGameRequestSocketMessage)
            {
                //                Console.WriteLine("Starting Game socket");

                client.SendPoolMessage("GameServers", Query.Build("NewGame", QueryDirection.Request, QueryType.Client, new CreateNewGameRequest()
                {
                    UserKey = user.ID,
                    GameName = "sevens"
                }));
            }
            else if (obj is AnswerQuestionSocketMessage)
            {
                client.SendMessage(uu.GameServer.Id, Query.Build("Answer",QueryDirection.Request, QueryType.Client,new GameServerServerMessage()
                {
                    GameId = uu.GameData.GameId,
                    AnswerIndex = ((AnswerQuestionSocketMessage)obj).AnswerIndex
                }));
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
    }

    public enum SocketMessageType
    {
        CreateNewGameRequest,
        AskQuestion,
        AnswerQuestion,
        GameOver,
        GameStarted,
    }

    public class Serializer
    {
        public static byte[] Serialize<T>(T t)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)t.GetType().GetCustomAttribute<MessageTypeAttribute>().MessageType);
            var props = t.GetType().GetProperties();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;
                var value = prop.GetValue(t);

                GetBytes(type, bytes, value);
            }

            return bytes.ToArray();
        }

        private static void GetBytes(Type type, List<byte> bytes, object value)
        {
            if (type == typeof(string))
            {
                bytes.Add((byte)Types.String);
                bytes.AddRange(BitConverter.GetBytes((short)((string)value).Length));
                bytes.AddRange(Encoding.UTF8.GetBytes((string)value));
            }
            else if (type == typeof(string[]))
            {
                bytes.Add((byte)Types.ArrayOfString);
                bytes.Add((byte)((string[])value).Length);
                foreach (var s in (string[])value)
                {
                    GetBytes(typeof(string), bytes, s);
                }
            }
            else if (type == typeof(short))
            {
                bytes.Add((byte)Types.Short);
                bytes.AddRange(BitConverter.GetBytes((short)value));
            }
            else if (type == typeof(byte))
            {
                bytes.Add((byte)Types.Byte);
                bytes.AddRange(BitConverter.GetBytes((byte)value));
            }
            else if (type.IsEnum)
            {
                bytes.Add((byte)Types.Byte);
                bytes.AddRange(BitConverter.GetBytes((byte)(int)value));
            }
            else
            {
                throw new Exception("Type not supported");
            }
        }

        public enum Types
        {
            String = 0, ArrayOfString = 1, Short = 2, ArrayOfInt = 3,
            Byte = 4
        }

        public static SocketMessage Deserialize(byte[] frame)
        {
            var socketMessage = FindObject(frame[0]);
            var propIndex = 0;
            for (var index = 1; index < frame.Length;)
            {
                var value = toValue(frame, ref index);
                socketMessage.GetType().GetProperties()[propIndex++].SetValue(socketMessage, value);

            }
            return socketMessage;
        }

        private static object toValue(byte[] frame, ref int index)
        {
            switch ((Types)frame[index++])
            {
                case Types.Short:
                    var val = BitConverter.ToInt16(frame, index);
                    index += 2;
                    return val;
                case Types.String:
                    var length = BitConverter.ToInt16(frame, index);
                    index += 2;
                    var str = new string(Encoding.UTF8.GetChars(frame, index, length));
                    index += length;
                    return str;
                case Types.ArrayOfString:
                    var strs = new List<string>();
                    var strLength = frame[index++];
                    for (var i = 0; i < strLength; i++)
                    {
                        strs.Add((string)toValue(frame, ref index));
                    }
                    return strs.ToArray();
                case Types.ArrayOfInt:
                    break;
                case Types.Byte:
                    return frame[index++];
            }
            throw new Exception("Cannot find Type");
        }

        private static SocketMessage FindObject(byte b)
        {
            switch ((SocketMessageType)b)
            {
                case SocketMessageType.CreateNewGameRequest:
                    return new CreateNewGameRequestSocketMessage();
                case SocketMessageType.AskQuestion:
                    return new AskQuestionSocketMessage();
                case SocketMessageType.AnswerQuestion:
                    return new AnswerQuestionSocketMessage();
                case SocketMessageType.GameOver:
                    return new GameOverSocketMessage();
                case SocketMessageType.GameStarted:
                    return new GameStartedSocketMessage();
            }
            throw new Exception("Cannot find object");
        }
    }
    [MessageType(SocketMessageType.CreateNewGameRequest)]
    public class CreateNewGameRequestSocketMessage : SocketMessage
    {

        public string GameType { get; set; }

    }


    [MessageType(SocketMessageType.AskQuestion)]
    public class AskQuestionSocketMessage : SocketMessage
    {
        public string Question { get; set; }
        public string[] Answers { get; set; }
        public string User { get; set; }

    }
    [MessageType(SocketMessageType.AnswerQuestion)]
    public class AnswerQuestionSocketMessage : SocketMessage
    {
        public short AnswerIndex { get; set; }
    }
    [MessageType(SocketMessageType.GameOver)]
    public class GameOverSocketMessage : SocketMessage
    {
    }

    [MessageType(SocketMessageType.GameStarted)]
    public class GameStartedSocketMessage : SocketMessage
    {
    }


    public class MessageTypeAttribute : Attribute
    {
        public SocketMessageType MessageType { get; set; }

        public MessageTypeAttribute(SocketMessageType messageType)
        {
            MessageType = messageType;
        }
    }

}
