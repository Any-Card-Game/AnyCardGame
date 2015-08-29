using System;
using System.Collections.Generic;
using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Common.Utils.Mongo
{
    public interface IMongoModel
    {
        ObjectId Id { get; set; }
    }

    public static class MongoTools
    {
        public static MongoDatabase GetDatabase()
        {
            var client = new MongoClient(ConnectionString);
            var server = client.GetServer();
            var database = server.GetDatabase(Database);
            return database;
        }

        public static MongoCollection<T> GetCollection<T>() where T : IMongoModel
        {
            var collection = GetDatabase().GetCollection<T>(GetCollectionName<T>());
            return collection;
        }
        public static MongoCollection<T> GetCollection<T>(string collectionName) where T : IMongoModel
        {
            var collection = GetDatabase().GetCollection<T>(collectionName);
            return collection;
        }

        public static string ConnectionString
        {
            get { return ConfigurationManager.AppSettings["MongoConnectionString"]; }
        }

        public static string Database
        {
            get { return ConfigurationManager.AppSettings["MongoDatabase"]; }
        }

        public static string GetCollectionName<T>() where T : IMongoModel
        {
            var m = typeof(T);

            string collectionName;
            if (cachedCollectionName.TryGetValue(m, out collectionName))
            {
                return collectionName;
            }


            var collectionNameProperpty = m.DeclaringType.GetField("CollectionName");
            collectionName = (string)collectionNameProperpty.GetValue(null);
            cachedCollectionName.Add(m, collectionName);
            return collectionName;
        }

        private static Dictionary<Type, string> cachedCollectionName = new Dictionary<Type, string>();

    }


}