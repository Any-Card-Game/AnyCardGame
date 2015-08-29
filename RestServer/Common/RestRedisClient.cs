using Common.Redis;
using Common.Redis.RedisMessages;

namespace RestServer.Common
{
    public static class RestRedisClient
    {
        private static RedisClient client;

        static RestRedisClient()
        {
            client = new RedisClient();

            client.SubscribeToAnswers(RedisChannels.GetNextGatewayResponse);
        }

        public static RedisClient Client => client;
    }
}