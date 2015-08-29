using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alchemy;
using Alchemy.Classes;
using Common.Redis;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {

            RedisClient client = new RedisClient();
            client.Subscribe("Shoes", a =>
            {

            });
            client.SendMessage("Shoes", new ThisRedditMessage()
            {
                Bar = 12,
                Foo = 19
            });
            var aServer = new WebSocketServer(81, IPAddress.Any)
            {
                OnConnected = OnConnected,
                TimeOut = new TimeSpan(0, 5, 0)
            };

            aServer.Start();
            Console.ReadLine();
        }

        public class ThisRedditMessage : IRedisMessage
        {
            public int Foo { get; set; }
            public int Bar { get; set; }
        }

        static void OnConnected(UserContext context)
        {
            context.SetOnDisconnect(OnDisconnect);
            context.SetOnReceive(OnReceive);

            Console.WriteLine("Client Connection From : " + context.ClientAddress.ToString());
            context.Send("hi");
        }

        private static void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Client disconnected From : " + context.ClientAddress.ToString());
        }

        private static void OnReceive(UserContext context)
        {
            Console.WriteLine("Client receive From : " + context.ClientAddress.ToString() + " " + context.DataFrame.ToString());
            context.Send("hi");
        }
    }
}
