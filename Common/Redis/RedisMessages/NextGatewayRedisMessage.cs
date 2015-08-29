namespace Common.Redis.RedisMessages
{
    public abstract class RedisMessage
    {
        protected RedisMessage()
        {
            Guid = System.Guid.NewGuid().ToString("N");
        }

        public string Guid { get; set; }
    }
    public class DefaultRedisMessage : RedisMessage
    {
    }
    public enum RedisChannels
    {
        GetNextGatewayRequest,
        GetNextGatewayResponse
    }

    public class NextGatewayResponseRedisMessage : RedisMessage
    {
        public string GatewayUrl { get; set; }
    }
    public class ThisRedditMessage : RedisMessage
    {
        public int Foo { get; set; }
        public int Bar { get; set; }
    }


}