using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerClient;
using BrokerCommon;
using Common.Messages;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class GatewayLogic
    {
        public static void GetFastestGateway( Action<string> gatewayCallback)
        {
            Program.gatewayPool.SendMessageWithResponse(Query.Build("NextGateway"),
                (response) =>
                {
                    var nextGateway = response.GetJson<NextGatewayResponseServerMessage>();
                    gatewayCallback(nextGateway.GatewayUrl);
                });


        }
    }

}