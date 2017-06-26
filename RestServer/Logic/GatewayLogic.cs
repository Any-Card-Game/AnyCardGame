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
            Program.client.SendPoolFastestMessage<NextGatewayResponseServerMessage>("Gateways", "NextGateway", null, (response) =>
            {
                gatewayCallback(response.GatewayUrl);
            });


        }
    }

}