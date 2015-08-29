using System;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace Common.Utils.Mongo
{
    public class QueryField
    {
        public QueryField(string key, BsonValue value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public BsonValue Value { get; set; }

        public static QueryField FromExpression<T>(Expression<Func<T, object>> key, BsonValue value)
        {
            return new QueryField(GetMemberInfo(key).Member.Name, value);
        }

        private static MemberExpression GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("method");

            MemberExpression memberExpr = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpr =
                    ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
                throw new ArgumentException("method");

            return memberExpr;
        }
    }
}