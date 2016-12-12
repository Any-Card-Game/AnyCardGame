using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Redis;
using Common.Redis.RedisMessages;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class GatewayLogic
    {
        public static Dictionary<string, int> distribution = new Dictionary<string, int>();

        public static async Task<GetGatewayResponse> GetFastestGateway(GetGatewayRequest model)
        {
            var nextGateway = (NextGatewayResponseRedisMessage)await RestRedisClient.Client.AskQuestion(RedisChannels.GetNextGatewayRequest);
            if (!distribution.ContainsKey(nextGateway.GatewayUrl))
            {
                distribution[nextGateway.GatewayUrl] = 0;
            }
            distribution[nextGateway.GatewayUrl]++;

            return new GetGatewayResponse()
            {
                GatewayUrl = nextGateway.GatewayUrl
            };
        }
    }

}