using System.Linq;
using System.Threading;
using RestServer;
using RestServer.Logic;

namespace RestServer2
{
    using System;
    using Nancy.Hosting.Self;

    class Program
    {
        private static Timer timer;
        static void Main(string[] args)
        {
            var uri = new Uri("http://127.0.0.1:3579");
            HostConfiguration hostConfigs = new HostConfiguration();
            hostConfigs.UrlReservations.CreateAutomatically = true;
            timer = new Timer((e) =>
            {
                lock (GatewayLogic.distribution)
                {
                    Console.WriteLine(GatewayLogic.distribution.Aggregate("", (a, b) => a + b.Key + ":" + b.Value + "|"));
                }
            }, null, 0, 500);
            

            using (var host = new NancyHost(uri, new Bootstrapper(), hostConfigs))
            {
                host.Start();

                Console.WriteLine("Your application is running on " + uri);
                Console.WriteLine("Press any [Enter] to close the host.");
                Console.ReadLine();
            }
        }
    }
}
