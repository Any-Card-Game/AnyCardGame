using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messages;
using OnPoolCommon;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class GatewayLogic
    {
        public static void GetFastestGateway(Action<string> gatewayCallback)
        {
            Program.client.SendPoolMessage("Gateways", Query.Build("NextGateway", QueryDirection.Request, QueryType.Client),
                (response) =>
                {
                    var nextGateway = response.GetJson<NextGatewayResponseServerMessage>();
                    gatewayCallback(nextGateway.GatewayUrl);
                });


        }
    }

}