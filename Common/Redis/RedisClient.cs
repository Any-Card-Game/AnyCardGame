//#define log

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Redis.RedisMessages;
using Common.Utils;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Common.Redis
{
    public class RedisClient
    {
        private Dictionary<string, Action<RedisMessage>> subscriptions = new Dictionary<string, Action<RedisMessage>>();
        private IDatabase database;
        private ISubscriber subscriber;
        private LateTaskManager<RedisMessage> lateTaskManager;


        public RedisClient()
        {
            var redisIP = "198.211.107.101";
            var redisPort = 6379;
            var options = ConfigurationOptions.Parse($"{redisIP}:{redisPort}");
            options.SyncTimeout = 10*1000;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            database = redis.GetDatabase();
            
            subscriber = redis.GetSubscriber();
            lateTaskManager = new LateTaskManager<RedisMessage>();
        }

        public void Subscribe(RedisChannels channel, Action<RedisMessage> resolve)
        {
            subscriptions.Add(channel.ToString(), resolve);
            subscriber.Subscribe(channel.ToString(), onReceiveMessage);
        }
        public void Subscribe(string channel, Action<RedisMessage> resolve)
        {
            subscriptions.Add(channel, resolve);
            subscriber.Subscribe(channel.ToString(), onReceiveMessage);
        }
        public void SubscribeToAnswers(RedisChannels channel)
        {
            subscriber.Subscribe(channel.ToString(), onReceiveMessage);
        }

        private void onReceiveMessage(RedisChannel channel, RedisValue value)
        {
            string work = (string)database.ListRightPop($"{(string)channel}-bl");
            if (work == null)
            {
//                Console.WriteLine($"{channel} got thing, but no message");
                return;
            }
#if log
            Console.WriteLine("Receiving Message: " + channel + "    " + work);
            Console.WriteLine();
#endif

            RedisMessage message = JsonConvert.DeserializeObject<RedisMessage>(work, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });


            if (lateTaskManager.Exists(message.Guid))
            {
                lateTaskManager.Resolve(message.Guid,message);
            }
            else
            {
                if (subscriptions[channel] != null)
                {
                    subscriptions[channel](message);
                }
            }
        }

        public void SendMessage(RedisChannels channel, RedisMessage message = null)
        {

            message = message ?? new DefaultRedisMessage();

            string str = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
#if log

            Console.WriteLine("Sending Message: " + channel + "    " + str);
            Console.WriteLine();
#endif

            database.ListLeftPush($"{channel}-bl", str);
            subscriber.Publish(channel.ToString(), "");
        }
        public void SendMessage(string channel, RedisMessage message = null)
        {

            message = message ?? new DefaultRedisMessage();

            string str = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });

#if log
            Console.WriteLine("Sending Message: " + channel + "    " + str);
            Console.WriteLine();
#endif

            database.ListLeftPush($"{channel}-bl", str);
            subscriber.Publish(channel, "");
        }

        public Task<RedisMessage> AskQuestion(RedisChannels channelEnum, RedisMessage message = null)
        {
            var channel = channelEnum.ToString();

            message = message ?? new DefaultRedisMessage();

            string str = JsonConvert.SerializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
#if log

            Console.WriteLine("Asking Question: " + channel + "    " + str);
            Console.WriteLine();
#endif
            database.ListLeftPush($"{channel}-bl", str);
            subscriber.Publish(channel, "");

            return lateTaskManager.Build(message.Guid);
        } 
    }

}