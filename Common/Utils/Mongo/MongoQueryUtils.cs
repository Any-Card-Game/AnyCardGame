using System.Linq;
using Common.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Common.Utils.Mongo
{
    public static class MongoQueryUtils
    {
        public static T GetById<T>(this MongoCollection<T> collection, string id)
        {
            return collection.FindOneAs<T>(Query.EQ("_id", new ObjectId(id)));
        }

        public static T GetOne<T>(this MongoCollection<T> collection, string name, BsonValue value)
        {
            return collection.FindOneAs<T>(Query.EQ(name, value));
        }

        public static T GetOne<T>(this MongoCollection<T> collection, params QueryField[] keys)
        {
            return collection.FindOneAs<T>(Query.And(keys.Select(a => Query.EQ(a.Key, a.Value))));
        }



        public static T[] GetAll<T>(this MongoCollection<T> collection, int page, int limit, SortByBuilder sort, string name, BsonValue value)
        {
            return collection.Find(Query.EQ(name, value)).SetSortOrder(sort).SetSkip(page * limit).Take(limit).ToArray();
        }



        public static T[] GetAll<T>(this MongoCollection<T> collection, int page, int limit, SortByBuilder sort, params QueryField[] keys)
        {
            if (!keys.Any())
                return collection.FindAll().SetLimit(limit).SetSortOrder(sort).SetSkip((page) * limit).Take(limit).ToArray();

            return collection.Find(Query.And(keys.Select(a => Query.EQ(a.Key, a.Value)))).SetSortOrder(sort).SetSkip((page) * limit).Take(limit).ToArray();
        }

        public static long Count<T>(this MongoCollection<T> collection, params QueryField[] keys)
        {
            if (!keys.Any())
                return collection.Count();

            return collection.Count(Query.And(keys.Select(a => Query.EQ(a.Key, a.Value))));
        }

        public static T[] GetAll<T>(this MongoCollection<T> collection, int page, int limit, SortByBuilder sort, IMongoQuery query)
        {
            return collection.Find(query).SetSortOrder(sort).SetSkip((page) * limit).Take(limit).ToArray();
        }
        public static long Count<T>(this MongoCollection<T> collection, IMongoQuery query)
        {
            return collection.Count(query);
        }


        public static T[] GetAll<T>(this MongoCollection<T> collection, IMongoQuery query)
        {
            return collection.Find(query).ToArray();
        }
        public static T[] GetAll<T>(this MongoCollection<T> collection, string name, BsonValue value)
        {
            return collection.Find(Query.EQ(name, value)).ToArray();
        }

        public static T[] GetAll<T>(this MongoCollection<T> collection, params QueryField[] keys)
        {
            return collection.Find(Query.And(keys.Select(a => Query.EQ(a.Key, a.Value)))).ToArray();
        }

        public static T[] GetAll<T>(this MongoCollection<T> collection)
        {
            return collection.FindAll().ToArray();
        }

        public static T[] GetNear<T>(this MongoCollection<T> collection, string key, double x, double y, int limit)
        {
            collection.EnsureIndex(IndexKeys.GeoSpatial(key));
            var query = Query.Near(key, x, y);
            return collection.Find(query).Take(limit).ToArray();
        }



        public static T Insert<T>(this T item) where T : IMongoModel
        {
            var collection = MongoTools.GetCollection<T>();
            collection.Insert(item);
            return item;
        }

        public static T Update<T>(this T item) where T : IMongoModel
        {
            var collection = MongoTools.GetCollection<T>();
            collection.Save(item);
            return item;
        }

        public static void Delete<T>(this string id) where T : IMongoModel
        {
            var collection = MongoTools.GetCollection<T>();
            collection.Remove(Query.EQ("_id", ObjectId.Parse(id)));
        }

        public static void Delete<T>(this T item) where T : IMongoModel
        {
            var collection = MongoTools.GetCollection<T>();
            collection.Remove(Query.EQ("_id", item.Id));
        }
    }
}