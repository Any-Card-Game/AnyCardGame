using System;
using System.Collections.Generic;
using Common.Utils.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Common.Data
{
    public static class MongoUser
    {
        public static string CollectionName = "__user";

        public static MongoCollection<User> Collection
        {
            get { return MongoTools.GetCollection<User>(); }
        }
        public static MongoCollection<T> CollectionAs<T>() where T : User
        {
            return MongoTools.GetCollection<T>();
        }

        [BsonIgnoreExtraElements]
        public class User : IMongoModel
        {
            public ObjectId Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
