using System;
using System.Linq;
using System.Threading;
using Nancy.Hosting.Self;
using OnPoolCommon;
using RestServer.Logic;

namespace RestServer
{
    public class Program
    {
        private static Timer timer;
        public static OnPoolClient.OnPoolClient client;
        public static string currentFastestGateway;

        static void Main(string[] args)
        {
            var ltm = LocalThreadManager.Start();
            client = new OnPoolClient.OnPoolClient();
            client.OnReady(() =>
            {
                timer = new Timer((e) =>
                {
                    GatewayLogic.GetFastestGateway(gw =>
                    {
                        currentFastestGateway = gw;
                        Console.WriteLine("Current fastest gateway: " + currentFastestGateway);
                    });
                }, null, 0, 5000);

            });
            client.ConnectToServer("127.0.0.1");
            ltm.Process();

            var uri = new Uri("http://127.0.0.1:3579");
            HostConfiguration hostConfigs = new HostConfiguration();
            hostConfigs.UrlReservations.CreateAutomatically = true;
            using (var host = new NancyHost(uri, new Bootstrapper(), hostConfigs))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
              Console.ReadLine();
            }
        }
    }
}
