using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alchemy;
using Alchemy.Classes;

namespace SocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var aServer = new WebSocketServer(81, IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnSend = OnSend,
                OnConnect = OnConnect,
                OnConnected = OnConnected,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };

            aServer.Start();
            Console.ReadLine();
        }

        private static void OnDisconnect(UserContext context)
        {
            throw new NotImplementedException();
        }

        private static void OnConnect(UserContext context)
        {
            throw new NotImplementedException();
        }

        private static void OnSend(UserContext context)
        {
            throw new NotImplementedException();
        }

        private static void OnReceive(UserContext context)
        {
            throw new NotImplementedException();
        }

        static void OnConnected(UserContext context)
        {
            Console.WriteLine("Client Connection From : " +context.ClientAddress.ToString());
        }
    }
}
