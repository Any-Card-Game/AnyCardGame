using System;
using System.Linq;
using System.Threading;
using BrokerClient;
using BrokerCommon;
using Nancy.Hosting.Self;
using RestServer.Logic;

namespace RestServer
{
    public class Program
    {
        private static Timer timer;
        public static ClientBrokerManager client;
        public static ClientPool gatewayPool;
        public static string currentFastestGateway;

        static void Main(string[] args)
        {
            var ltm = LocalThreadManager.Start();
            client = new ClientBrokerManager();
            client.ConnectToBroker("127.0.0.1");
            client.OnReady(() =>
            {

                client.GetPool("Gateways", pool =>
                {
                    gatewayPool = pool;

                    GatewayLogic.GetFastestGateway(gateway =>
                    {
                        currentFastestGateway = gateway;
                        var uri = new Uri("http://127.0.0.1:3579");
                        HostConfiguration hostConfigs = new HostConfiguration();
                        hostConfigs.UrlReservations.CreateAutomatically = true;
                        timer = new Timer((e) =>
                        {
                            GatewayLogic.GetFastestGateway(gw =>
                            {
                                currentFastestGateway = gw;
                                Console.WriteLine("Current fastest gateway: " + currentFastestGateway);
                            });
                        }, null, 0, 500);


                        using (var host = new NancyHost(uri, new Bootstrapper(), hostConfigs))
                        {
                            host.Start();

                            Console.WriteLine("Your application is running on " + uri);
                            Console.ReadLine();
                        }

                    });

                });
            });
            ltm.Process();
        }
    }
}
