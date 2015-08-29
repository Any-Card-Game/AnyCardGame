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
    class Program
    {

        static void Main(string[] args)
        {
            string url = Utils.GetPublicIP() + ":81";
            Console.WriteLine(url);

            RedisClient client = new RedisClient();
            client.Subscribe(RedisChannels.GetNextGatewayRequest, request =>
            {
                client.SendMessage(RedisChannels.GetNextGatewayResponse,new NextGatewayResponseRedisMessage()
                {
                    Guid= request.Guid,
                    GatewayUrl = url
                });
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
        static void OnConnected(UserContext context)
        {
            context.SetOnDisconnect(OnDisconnect);
            context.SetOnReceive(OnReceive);

            Console.WriteLine("Client Connection From : " + context.ClientAddress.ToString());

            var str = JsonConvert.SerializeObject(new ThisRedditMessage(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            context.Send(str);
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


            RedisMessage obj = JsonConvert.DeserializeObject<RedisMessage>(context.DataFrame.ToString(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });


            var str = JsonConvert.SerializeObject(new ThisRedditMessage(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            context.Send(str);
        }
    }
}
