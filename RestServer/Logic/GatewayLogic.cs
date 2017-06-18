using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrokerClient;
using BrokerCommon;
using Common.Redis;
using Common.Redis.RedisMessages;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class GatewayLogic
    {
        public static Dictionary<string, int> distribution = new Dictionary<string, int>();
        private static ClientBrokerManager client;

        public static async Task<GetGatewayResponse> GetFastestGateway(GetGatewayRequest model)
        {
            return await Task.Run(() =>
            {
                string gatewayUrl = null;
                Action getPool = () =>
                {
                    client.GetPool("GetNextGateway", pool =>
                    {
                        pool.SendMessageWithResponse<NextGatewayResponseRedisMessage>(Query.Build("Next"),
                            (nextGateway) =>
                            {
                                if (!distribution.ContainsKey(nextGateway.GatewayUrl))
                                {
                                    distribution[nextGateway.GatewayUrl] = 0;
                                }
                                distribution[nextGateway.GatewayUrl]++;

                                gatewayUrl = nextGateway.GatewayUrl;
                            });
                    });
                };
                if (client == null)
                {
                    client = new ClientBrokerManager();
                    client.ConnectToBroker("127.0.0.1");
                    client.OnReady(() =>
                    {
                        getPool();
                    });
                }
                else
                {
                    getPool();
                }

                while (gatewayUrl == null) ;
                return new GetGatewayResponse()
                {
                    GatewayUrl = gatewayUrl
                };
            });
        }
    }

}