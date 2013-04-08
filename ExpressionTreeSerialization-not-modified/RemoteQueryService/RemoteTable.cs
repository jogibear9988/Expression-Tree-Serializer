using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using ExpressionSerialization;

namespace RemoteQueryService
{
    /// <summary>
    /// A client-side proxy for a DataContext. Queries written against a RemoteTable will be
    /// transformed into queries against the corresponding Table on the server-side.
    /// </summary>
    public class RemoteTable<T> : IQueryable<T>, IQueryProvider, IOrderedQueryable, IOrderedQueryable<T>
    {
        Expression expression;
        public Expression Expression { get { return expression; } }
        public IQueryProvider Provider { get { return this; } }
        public Type ElementType { get { return typeof(T); } }

        public RemoteTable()
        {
            expression = Expression.Constant(this);
        }

        private RemoteTable(Expression expression)
        {
            this.expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            Object o = this.Execute(this.expression);
            IEnumerable enumerable = (IEnumerable)o;
            return enumerable.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new RemoteTable<TElement>(expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            //Type queryType = typeof(IQueryable<>).MakeGenericType(new Type[] { elementType });

            return (IQueryable)Activator.CreateInstance(
				typeof(RemoteTable<>).MakeGenericType(new Type[] { elementType }), 
				new object[] { expression });
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)this.Execute(expression);
        }

        public object Execute(Expression expression)
        {
            XElement queryXml = this.SerializeQuery();
            RemoteQueryService.ServiceReference.NorthwindServiceClient client = new RemoteQueryService.ServiceReference.NorthwindServiceClient();

            Type ienumerableExpressionType = TypeSystem.FindIEnumerable(expression.Type);
            if (ienumerableExpressionType == null)
            {
                if (typeof(ServiceReference.Customer).IsAssignableFrom(expression.Type))
                    return client.ExecuteQueryForCustomer(queryXml);
                else if (typeof(ServiceReference.Order).IsAssignableFrom(expression.Type))
                    return client.ExecuteQueryForOrder(queryXml);
                else
                    return client.ExecuteQueryForObject(queryXml);
            }
            Type elementType = TypeSystem.GetElementType(expression.Type);
            if (typeof(ServiceReference.Customer).IsAssignableFrom(elementType))
                return client.ExecuteQueryForCustomers(queryXml);
            else if (typeof(ServiceReference.Order).IsAssignableFrom(elementType))
                return client.ExecuteQueryForOrders(queryXml);
            else
                return client.ExecuteQueryForObjects(queryXml);
        }
    }

    internal static class TypeSystem
    {
        internal static Type GetElementType(Type seqType)
        {
            Type type = FindIEnumerable(seqType);
            if (type == null)
            {
                return seqType;
            }
            return type.GetGenericArguments()[0];
        }

        internal static Type FindIEnumerable(Type seqType)
        {
            if ((seqType != null) && (seqType != typeof(string)))
            {
                if (seqType.IsArray)
                {
                    return typeof(IEnumerable<>).MakeGenericType(new Type[] { seqType.GetElementType() });
                }
                if (seqType.IsGenericType)
                {
                    foreach (Type type in seqType.GetGenericArguments())
                    {
                        Type type2 = typeof(IEnumerable<>).MakeGenericType(new Type[] { type });
                        if (type2.IsAssignableFrom(seqType))
                        {
                            return type2;
                        }
                    }
                }
                Type[] interfaces = seqType.GetInterfaces();
                if ((interfaces != null) && (interfaces.Length > 0))
                {
                    foreach (Type type3 in interfaces)
                    {
                        Type type4 = FindIEnumerable(type3);
                        if (type4 != null)
                        {
                            return type4;
                        }
                    }
                }
                if ((seqType.BaseType != null) && (seqType.BaseType != typeof(object)))
                {
                    return FindIEnumerable(seqType.BaseType);
                }
            }
            return null;
        }


    }
}
