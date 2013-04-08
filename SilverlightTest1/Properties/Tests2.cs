using System;
using System.Net;
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
using ObjectServices;
using ExpressionSerialization;

namespace SilverlightTest1
{
	[TestClass]
	public class Tests2
	{
		[TestMethod]
		public void Test12()
		{
			

			Order order1 = new Order { Customer = Hans, ID = 302, Freight = 2001.99m, OrderDate = DateTime.Now.AddMonths(-20) };

			var assemblies = new Assembly[] { typeof(Order).Assembly, typeof(UnitTests).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			var resolver = new ExpressionSerializationTypeResolver(assemblies, new Type[] { typeof(Customer), typeof(Order), typeof(Product), typeof(Supplier), typeof(Shipper) });
			ExpressionSerializer serializer = new ExpressionSerializer(resolver);

			IEnumerable<Customer> customers = GetCustomers().ToArray();
            Expression<Func<int, IEnumerable<Order[]>>> e12 =
             n =>
                 from c in customers//instance == null : IEnumerable.Where/.Select
                 where c.ID < n
                 select c.Orders.ToArray();
			e12 = ( Expression<Func<int, IEnumerable<Order[]>>>)ObjectServices.Evaluator.PartialEval(e12);
			MethodCallExpression m1 = ((MethodCallExpression)e12.Body).Arguments[0] as MethodCallExpression;
			ConstantExpression cx = ((ConstantExpression)m1.Arguments[0]);
			LambdaExpression lambdaarg = ((LambdaExpression)m1.Arguments[1]);
			//Expression arg1 = ((MethodCallExpression)e12.Body).Arguments[1];
			XElement xml12 = serializer.Serialize(e12);
			
            Expression result12 = serializer.Deserialize( xml12);
            Assert.AreEqual( e12.ToString(), result12.ToString());
            Console.WriteLine(((result12 as Expression<Func<int, IEnumerable<Order[]>>>).Compile())(5));
		}
		//private bool IsIEnumerableOf(Type enumerableType, Type elementType)
		//{
		//    if (!enumerableType.IsGenericType)
		//        return false;
		//    Type[] typeArgs = enumerableType.GetGenericArguments();
		//    if (typeArgs.Length != 1)
		//        return false;
		//    if (!elementType.IsAssignableFrom(typeArgs[0]))
		//        return false;
		//    if (!typeof(IEnumerable<>).MakeGenericType(typeArgs).IsAssignableFrom(enumerableType))
		//        return false;
		//    return true;
		//}

		[TestMethod]
		public void XELementTest()
		{
			XElement xmlTree1 = new XElement("Root",
				new XElement("Child1", 1),
				new XElement("Child2", 2),
				new XElement("Child3", 3),
				new XElement("Child4", 4),
				new XElement("Child5", 5),
				new XElement("Child6", 6)
			);
			foreach (var el in xmlTree1.Elements())
			{
				int value = (int)el;//can cast XElement.Value like this
			}

			XElement xmlTree2 = new XElement("Root",
				from el in xmlTree1.Elements()
				where ((int)el >= 3 && (int)el <= 5)
				select el
			);

			XElement xmlTree3 = new XElement("params object[]", new object[] { new XElement("name", 4.02), new { id = 10013, firstName = "tom" }, 5.7, "string" });
		}


		// Very simple serialization example
		[TestMethod]
		public void BasicExpressionSerialization()
		{
			Type funcT1T2T3 = typeof(Func<>).Assembly.GetType("System.Func`3");

			Debug.WriteLine("BASIC SAMPLE - Serialize/Deserialize Simple Expression:");
			XElement addXml;
			Expression<Func<int, int, int>> addExpr = (x, y) => x + y;
			ExpressionSerializer serializer = new ExpressionSerializer();
			Expression simplifiedAddExpr = Evaluator.PartialEval(addExpr);
			//addXml = serializer.Serialize(simplifiedAddExpr);	//does not seem necessary
			addXml = serializer.Serialize(addExpr);
			Expression<Func<int, int, int>> addExpResult = serializer.Deserialize<Func<int, int, int>>(addXml);
			Func<int, int, int> addExpResultFunc = addExpResult.Compile();
			int result = addExpResultFunc(1, 2);  // evaluates to 3

			Debug.WriteLine("Deserialized Expression Tree:");
			Debug.WriteLine(" " + addExpResult.ToString());
		}

		// Serializing an expression tree representing a query expression
		[TestMethod]
		public void ComplexExpressionSerializationSamples()
		{
			Debug.WriteLine("COMPLEX SAMPLE - Serialize/Deserialize In-Memory Query Expression:");

			Expression<Func<IEnumerable<int>>> queryExp = () => from i in Enumerable.Range(1, 10)
																where i % 2 == 0
																select i * i;

			ExpressionSerializer serializer = new ExpressionSerializer();
			XElement queryXml = serializer.Serialize(queryExp);
			Expression<Func<IEnumerable<int>>> queryExpResult = serializer.Deserialize<Func<IEnumerable<int>>>(queryXml);

			// Print out the expression tree: "(x, y) => x + y"
			Debug.WriteLine("Deserialized Expression Tree:");
			Debug.WriteLine(" " + queryExpResult.ToString());

			// Call it
			Func<IEnumerable<int>> f = queryExpResult.Compile();
			IEnumerable<int> result = f();
			Debug.WriteLine("\nResults: ");
			result.ToList().ForEach(n => Debug.WriteLine(" " + n));
		}


		[TestMethod]
		// Example of querying using LINQ against a LINQ to SQL implementation hidden behind a WCF service
		// Note that no database is being directly referenced - all types and calls are proxies generated by 
		// the service reference.
		public void AcrossTheWireSerializationSamples()
		{
			Console.WriteLine("DLINQ ACROSS THE WIRE SAMPLE - Query against an IQueryable wrapper over a web service:");

			// Query is against a RemoteTable which is a proxy for the the WCF service which executes the DLinq query
			// on the server.  Note that the elements are the service-reference generated types that align with the 
			// DLinq mapping types via the DataContracts.
			QueryProvider provider = null;
			var queryable = new Query<Customer>(provider); //RemoteTable<RemoteQueryService.ServiceReference.Customer>();
			var query = from c in queryable
						where c.City == "London"
						orderby c.ID
						select (from o in c.Orders
								orderby o.OrderDate
								select o.OrderDate).First();

			Console.WriteLine("\n Query Results: ");
			foreach (var c in query)
				Console.WriteLine(" " + c.Value.ToShortDateString());
		}


		public static Customer Hans = new Customer { Name = "Hans Passant", City = "Cambridge", Country = "UK", ID = 200 };
		public static Customer Marc = new Customer { Name = "Marc Gravell", City = "Seattle", Country = "USA", ID = 11 };

		public static IEnumerable<Customer> GetCustomers()
		{
			Customer[] customers = new Customer[] {
				Hans,
				Marc,
                new Customer { 
                    ID = -111,
                    Name = "Bob",
                    Orders = new List<Order> {
                        new Order() {
                            ShipAddress = "address 0",
                            Freight = 5
                        },
                        new Order() {
                            ShipAddress = "address 1",
                            Freight = 123
                        }}},
                new Customer() { 
                    ID = 222,
                    Name = "Dave",
                    Orders = new List<Order> {
                        new Order() {
                            ShipAddress = "city 0",
                            Freight = 5
                        },
                        new Order() {
                            ShipAddress = "place 2",
                            Freight = 199
                        }
                    }
                 } 	,
                new Customer() { 
                    ID = -3333,
                    Name = "Abigail",
                    Orders = new List<Order> {
                        new Order() {
                            ShipAddress = "town 0",
                            Freight = 5
                        },
                        new Order() {
                            ShipAddress = "place 44",
                            Freight = 199
                        }
                    }
                 } 				 
            };
			return customers;
		}

	}
}
