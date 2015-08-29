using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Common.Redis
{
    public class RedisClient
    {
        private ISubscriber subscriber;
        private Dictionary<string, Action<IRedisMessage>> subscriptions = new Dictionary<string, Action<IRedisMessage>>();
        public RedisClient()
        {
            var redisIP = "198.211.107.101";
              redisIP = "198.211.107.101";
            var redisPort = 6379;
            Console.WriteLine("Connecting");
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisIP}:{redisPort}");
            Console.WriteLine("Connected");
            subscriber = redis.GetSubscriber();
            Console.WriteLine("Subscribed");
        }

        public void Subscribe(string channel, Action<IRedisMessage> resolve)
        {
            subscriptions.Add(channel, resolve);

            subscriber.Subscribe(channel, onReceiveMessage);
        }

        private void onReceiveMessage(RedisChannel channel, RedisValue value)
        {
            IRedisMessage obj = JsonConvert.DeserializeObject<IRedisMessage>(value, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            subscriptions[(string)channel](obj);
        }

        public void SendMessage(string channel, IRedisMessage message)
        {
            var sz = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            subscriber.Publish(channel, sz);
        }
    }
     
    public interface IRedisMessage
    {
    }
}