using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionSerialization;
using Northwind;
using System.Reflection;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.ServiceModel;

namespace NorthwindClient
{
	class Program
	{
		static Uri baseAddress = new Uri("http://localhost:8999/");
		static void Main(string[] args)
		{			
			Test1();
			Test2();
		}


		
		static void Test1()
		{				
			//just create a IQueryable to get the Expression:
			IQueryable<Customer> queryable = from c in new Query<Customer>()
											 where c.ID != null && c.ID > 5 && c.ID < 10
											 select c;
			//serialize:
			var serializer = CreateSerializer();
			XElement xmlExpression = serializer.Serialize(queryable.Expression);

			//make direct call to WebHttp service and send the Expression as XML
			//(do not use RemoteProvider).
			var client = new WebHttpClient<IQueryService>(baseAddress);			
			Customer[] result = client.SynchronousCall<Customer[]>((svc) => svc.ExecuteQuery(xmlExpression));
			var count = Enumerable.Count(result); 
		}

		/// <summary>
		/// make request with RemoteProvider
		/// </summary>
		static void Test2()
		{
			var client = new WebHttpClient<IQueryService>(baseAddress);
			IQueryProvider provider = new RemoteProvider(client);
			Query<Customer> query = new Query<Customer>(provider);
			IQueryable<Customer> queryable = from c in ((IQueryable<Customer>)query)
											 where c.ID > 5 && c.ID < 10
											 select c;

			List<Customer> results = queryable.ToList();

		}


		static ExpressionSerializer CreateSerializer()
		{
			var assemblies = new Assembly[] { typeof(Customer).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			var resolver = new TypeResolver(assemblies, new Type[] 
			{ 
				typeof(Customer), typeof(Order), typeof(Product), 
				typeof(Supplier), typeof(Shipper) 
			});
			//var creator = new QueryCreator();
			CustomExpressionXmlConverter queryconverter = new QueryExpressionXmlConverter(creator: null, resolver: resolver);
			CustomExpressionXmlConverter knowntypeconverter = new KnownTypeExpressionXmlConverter(resolver);
			ExpressionSerializer serializer = new ExpressionSerializer(resolver, new CustomExpressionXmlConverter[] { queryconverter, knowntypeconverter });
			return serializer;
			//ExpressionSerializer serializer = new ExpressionSerializer()
		}
	}
}
