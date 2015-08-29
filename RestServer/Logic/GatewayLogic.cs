using System.Threading.Tasks;
using Common.Redis;
using Common.Redis.RedisMessages;
using RestServer.Common;
using RestServer.Modules;

namespace RestServer.Logic
{
    public class GatewayLogic
    {
        public static async Task<GetGatewayResponse> GetFastestGateway(GetGatewayRequest model)
        {
            var nextGateway = (NextGatewayResponseRedisMessage)await RestRedisClient.Client.AskQuestion(RedisChannels.GetNextGatewayRequest);
            
            return new GetGatewayResponse()
            {
                GatewayUrl= nextGateway.GatewayUrl
            };
        }
    }

}