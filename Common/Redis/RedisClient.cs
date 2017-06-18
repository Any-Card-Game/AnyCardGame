/*//#define log

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
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
            /*            var redisIP = "198.211.107.101";
                        var redisPort = 6379;
            #1#
            var redisIP = "127.0.0.1";
            var redisPort = 6379;
            var options = ConfigurationOptions.Parse($"{redisIP}:{redisPort}");
            options.SyncTimeout = 10 * 1000;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            database = redis.GetDatabase();

            subscriber = redis.GetSubscriber();
            lateTaskManager = new LateTaskManager<RedisMessage>();
        }

        public void AddMe(string list, string name)
        {
            database.ListRightPush(list, name);
        }
        public string[] GetList(string list)
        {
            return database.ListRange(list).ToStringArray();
        }
        public int GetCount(string name)
        {
            return (int)database.StringGet(name + "COUNT");
        }
        public void IncreaseCount(string name)
        {
            database.StringIncrement(name + "COUNT", 1);
        }

        public void DecreaseCount(string name)
        {
            database.StringIncrement(name + "COUNT", 1);
        }
        public void RemoveMe(string list, string name)
        {
            database.KeyDelete(name + "COUNT");
            database.ListRemove(list, name);
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
            byte[] work = (byte[])database.ListRightPop($"{(string)channel}-bl");
            if (work == null)
            {
                //                Console.WriteLine($"{channel} got thing, but no message");
                return;
            }
#if log
            Console.WriteLine("Receiving Message: " + channel + "    " + work);
            Console.WriteLine();
#endif

            RedisMessage  message = ByteArrayToObject<RedisMessage>(work);


            if (lateTaskManager.Exists(message.Guid))
            {
                lateTaskManager.Resolve(message.Guid, message);
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


#if log

            Console.WriteLine("Sending Message: " + channel + "    " + str);
            Console.WriteLine();
#endif

            database.ListLeftPush($"{channel}-bl", ObjectToByteArray(message));
            subscriber.Publish(channel.ToString(), "");
        }
        public void SendMessage(string channel, RedisMessage message = null)
        {

            message = message ?? new DefaultRedisMessage();



#if log
            Console.WriteLine("Sending Message: " + channel + "    " + str);
            Console.WriteLine();
#endif

            database.ListLeftPush($"{channel}-bl", ObjectToByteArray(message));
            subscriber.Publish(channel, "");
        }

        public Task<RedisMessage> AskQuestion(RedisChannels channelEnum, RedisMessage message = null)
        {
            var channel = channelEnum.ToString();

            message = message ?? new DefaultRedisMessage();


#if log

            Console.WriteLine("Asking Question: " + channel + "    " + str);
            Console.WriteLine();
#endif
            database.ListLeftPush($"{channel}-bl", ObjectToByteArray(message));
            subscriber.Publish(channel, "");

            return lateTaskManager.Build(message.Guid);
        }

        byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        T ByteArrayToObject<T>(byte[] obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(obj))
            {
                return (T)bf.Deserialize(ms);
            }
        }
    }

}*/