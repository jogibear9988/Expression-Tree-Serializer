using System;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressionSerialization;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Northwind;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using System.Net;


namespace UnitTests
{
	[TestClass]
	public class ServiceTests : BaseTests
	{
		Uri baseAddress = new Uri("http://localhost:8999/");		
		[TestMethod]
		[Tag("WCF")]
		public void Test1()
		{
			//just create a IQueryable to get the Expression:
			IQueryable<Customer> queryable = from c in new Query<Customer>()
											 where c.ID != null && c.ID > 5 && c.ID < 10
											 select c;
			//serialize:
			XElement xmlExpression = this.serializer.Serialize(queryable.Expression);

			//make direct call to WebHttp service and send the Expression as XML
			//(do not use RemoteProvider).
			var client = new WebHttpClient<IQueryService>(baseAddress);

			ThreadPool.QueueUserWorkItem(state =>
				{
					Customer[] result = client.SynchronousCall<Customer[]>((svc) => svc.ExecuteQuery(xmlExpression));
					int count = Enumerable.Count(result);
				});			
		}

		[TestMethod]
		[Tag("WCF")]
		public void Test2()
		{
			var client = new WebHttpClient<IQueryService>(baseAddress);
			IQueryProvider provider = new RemoteProvider(client);
			Query<Customer> query = new Query<Customer>(provider);
			
			ThreadPool.QueueUserWorkItem(state => 
				{
					IQueryable<Customer> queryable = from c in new Query<Customer>()
													 where c.ID <= 30
													 && c.Country == "Spain"
													 || c.ContactTitle == "Owner"
													 select c;

					List<Customer> results = queryable.ToList();
					int count = queryable.Count();
				});
			
		}


		public ServiceTests() 
			:base()
		{
			//this.assemblies = new Assembly[] { typeof(Customer).Assembly, typeof(BaseTests).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			//this.resolver = new TypeResolver(assemblies, new Type[] 
			//{ 
			//    typeof(Customer), typeof(Order), typeof(Product), 
			//    typeof(Supplier), typeof(Shipper) 
			//});
			//var creator = new QueryCreator(this.FnGetObjects);
			//CustomExpressionXmlConverter queryconverter = new QueryExpressionXmlConverter(creator, this.resolver);
			//CustomExpressionXmlConverter knowntypeconverter = new KnownTypeExpressionXmlConverter(this.resolver);
			//this.serializer = new ExpressionSerializer(resolver, new CustomExpressionXmlConverter[] { queryconverter, knowntypeconverter });
		}
	}
}
