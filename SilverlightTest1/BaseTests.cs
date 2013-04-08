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
using System.Runtime.Serialization;
using System.IO;

namespace UnitTests
{

	[TestClass]
	public class BaseTests
	{
		[TestMethod]
		[Tag("Type")]
		public void TypeResolverHasKnownTypeTest()
		{
			Type funcT1T2T3 = typeof(Func<>).Assembly.GetType("System.Func`3");
			Type ie = typeof(List<>);
			Type genericListType1 = ie.MakeGenericType(typeof(Int32));
			Type genericListType2 = ie.MakeGenericType(typeof(string));
			Type genericListType3 = ie.MakeGenericType(typeof(Customer));


			Assert.IsTrue(this.resolver.knownTypes.Contains(genericListType1));
			Assert.IsTrue(this.resolver.knownTypes.Contains(genericListType2));
			Assert.IsTrue(this.resolver.knownTypes.Contains(genericListType3));
			Assert.IsTrue(this.resolver.knownTypes.Contains(typeof(List<Order>)));
			Assert.IsTrue(this.resolver.knownTypes.Contains(typeof(Nullable<int>)));
			Assert.IsTrue(this.resolver.knownTypes.Contains(typeof(Shipper[])));
			Assert.IsTrue(this.resolver.knownTypes.Contains(typeof(IEnumerable<Customer>)));
			Assert.IsTrue(this.resolver.knownTypes.Contains(typeof(Nullable<Int32>)));
		}

		[TestMethod]
		[Tag("Type")]
		[Tag("CustomExpressionXmlConverter")]
		public void KnownTypeExpressionXmlConverterTest()
		{
			CustomExpressionXmlConverter knconverter = new KnownTypeExpressionXmlConverter(this.resolver);
			XElement x;
			ConstantExpression cx;
			Expression e;
			List<int> intlist = new List<int> { 0, 1, 3, 4, 5 };
			cx = Expression.Constant(intlist);
			knconverter.TrySerialize(cx, out x);
			knconverter.TryDeserialize(x, out e);
			Assert.IsTrue(((ConstantExpression)e).Value is List<int>);
			Assert.IsTrue(((List<int>)((ConstantExpression)e).Value).SequenceEqual(intlist));

			Customer c = Hans;
			cx = Expression.Constant(c);
			knconverter.TrySerialize(cx, out x);
			knconverter.TryDeserialize(x, out e);
			Assert.IsTrue(((ConstantExpression)e).Value is Customer);

		}


	
		[TestMethod]
		[Tag("Type")]
		public void ParseToBinaryExpression()
		{

			ConstantExpression cx;
			Expression left, right;
			BinaryExpression eq, add;
			Nullable<int> id = -100; // = default(Nullable<int>);
			int Id = 2011;
			var simple = new SimpleClass { ID = id, Name = "some name" };


			left = Expression.MakeMemberAccess(Expression.Constant(simple),
				typeof(SimpleClass).GetField("ID"));
			right = Expression.Constant(id);
			ParseBinaryConvert(ref left, ref right);
			eq = Expression.MakeBinary(ExpressionType.Equal, left, right);

			//property Nullable<int> Customer.ID:			
			left = Expression.MakeMemberAccess(Expression.Constant(Marc),
				typeof(Customer).GetProperty("ID"));
			//left: Nullable<int>, right: Nullable<int>		
			//if (left.Type.Name == "Nullable`1")
			//    left = Expression.Property(left, "Value");			
			right = Expression.Constant(Hans.ID);


			left = Expression.PropertyOrField(left, "Value");
			right = Expression.PropertyOrField(right, "Value");
			eq = Expression.MakeBinary(ExpressionType.Equal, left, right);


			//left = m;
			right = Expression.Constant(id);
			eq = Expression.MakeBinary(ExpressionType.Equal, left, right);


			//left: Nullable<int>, right: int
			eq = Expression.MakeBinary(ExpressionType.Add, left, Expression.Constant(Id));



		}

		void ParseBinaryConvert(ref Expression left, ref Expression right)
		{
			if (left.Type != right.Type)
			{
				UnaryExpression unary;
				LambdaExpression lambda;
				if (right is ConstantExpression)
				{
					unary = Expression.Convert(left, right.Type);
					left = unary;
				}
				else //(left is ConstantExpression)				
				{
					unary = Expression.Convert(right, left.Type);
					right = unary;
				}
				lambda = Expression.Lambda(unary);
				Delegate fn = lambda.Compile();
				var result = fn.DynamicInvoke(new object[0]);
			}
		}

		// Very simple serialization example
		[TestMethod]
		[Tag("LambdaExpression")]
		public void BasicExpressionSerialization()
		{

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
		[Tag("LambdaExpression")]
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

		/// <summary>
		/// test created for writing ExpressionSerializer.GenerateXmlFromExpressionCore
		/// </summary>
		[TestMethod]
		public void MemberAccessInvoke()
		{
			MemberExpression mb;
			LambdaExpression lambda;
			object value;
			PropertyInfo pinfo;
			Customer customer = GetCustomers().First();

			//Here are 2 ways to access a  property dynamically:
			//1: Omit Customer parameter, just the MemberExpression.
			pinfo = typeof(Customer).GetProperty("ID");
			mb = Expression.Property(Expression.Constant(customer), pinfo);
			lambda = Expression.Lambda(mb);
			value = lambda.Compile().DynamicInvoke(new object[] { });

			//2: include Customer as parameter in LambdaExpression:
			pinfo = typeof(Customer).GetProperty("ID");
			mb = Expression.Property(Expression.Constant(customer), pinfo);
			lambda = Expression.Lambda(mb, new ParameterExpression[] { Expression.Parameter(typeof(Customer)) });
			value = lambda.Compile().DynamicInvoke(new object[] { customer });

			//EnumeratePropertiesAndInvoke(customer);

			//ConstantExpression, does not throw Exception on all property invocations (e.g. Expression.Type)

			//ConstantExpression cx = Expression.Constant(customer);
			//EnumeratePropertiesAndInvoke(cx);

			//ParameterExpression, however, throws Exception when attempting to invoke its properties.
			ParameterExpression px = Expression.Parameter(typeof(Customer));
			EnumeratePropertiesAndInvoke(px);

		}
		#region EnumeratePropertiesAndInvoke
		static void EnumeratePropertiesAndInvoke(object instance)
		{
			MemberExpression mb;
			LambdaExpression lambda;
			object value;
			PropertyInfo pinfo;
			dynamic x = instance;
			Expression e = (Expression)instance;
			//if (x is Expression)
			//{
			//    Type expression_Type;
			//    switch (((Expression)x).NodeType)//TODO: auto-generate this switch statement
			//    {
			//        default:
			//            expression_Type = x.Type;
			//            break;
			//    }
			//}				
			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			for (int i = 0; i < properties.Length; i++)
			{
				pinfo = properties[i];

				mb = Expression.Property(Expression.Constant(e), pinfo);

				lambda = Expression.Lambda(mb);
				Delegate fn = lambda.Compile();
				value = Expression.Constant(fn.DynamicInvoke(null), mb.Type);

				lambda = Expression.Lambda(mb);
				value = lambda.Compile().DynamicInvoke(new object[] { });

				mb = Expression.Property(Expression.Constant(instance), pinfo);
				lambda = Expression.Lambda(mb, Expression.Parameter(typeof(Expression)));
				value = lambda.Compile().DynamicInvoke(new object[] { instance });

				value = pinfo.GetValue(instance, null);//3. plain Reflection.
			}
		}
		#endregion


		protected bool CanSerialize(object instance, IEnumerable<Type> knownTypes = null)
		{
			Type inputType = instance.GetType();
			if (knownTypes == null)
				knownTypes = new Type[] { inputType };
			DataContractSerializer dcs = new DataContractSerializer(inputType, knownTypes);
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					dcs.WriteObject(ms, instance);
					ms.Position = 0;
					object value = dcs.ReadObject(ms);
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return false;
			}

		}


		public static int Foo<T>(T t)
		{
			return 1;
		}
		public static int Foo<T, U>(T t, U u)
		{
			return 2;
		}

		public static List<Order> OrderList =
			new List<Order> {
                        new Order {
                            ShipAddress = "address 0",
                            Freight = 5
                        },
                        new Order {
                            ShipAddress = "address 1",
                            Freight = 123
                        },
						new Order() {
                            ShipAddress = "city 0",
                            Freight = 5
                        },
                        new Order() {
                            ShipAddress = "place 2",
                            Freight = 199
                        }
			};

		public static IEnumerable<string> GetStrings()
		{
			return GetCustomers().Select(c => c.Name).ToList();
		}

		public static Customer Hans = new Customer { Name = "Hans Passant", City = "Cambridge", Country = "UK", ID = 199, Orders = OrderList };
		public static Customer Marc = new Customer { Name = "Marc Gravell", City = "Seattle", Country = "USA", ID = 111, Orders = OrderList };

		public static IEnumerable<Customer> GetCustomers()
		{
			Customer[] customers = new Customer[] {
				Hans,
				Marc,
				new Customer { ID = 17, Name = "John Skeet", City = "San Francisco", Orders = new List<Order> { new Order { Freight = 200.1m, OrderDate = DateTime.Now.AddYears(-2), ShipperId = 201 } } },
                new Customer { 
                    ID = -111,
                    Name = "Bob",
                    Orders = OrderList,
				},
                new Customer { 
                    ID = 222,
                    Name = "Dave",
                    Orders = OrderList
                 },
                new Customer { 
                    ID = -3333,
                    Name = "Abigail",
                    Orders = OrderList
                    }
            };
			return customers;
		}

		protected dynamic FnGetObjects(Type elementType)
		{
			if (elementType != typeof(Customer))
				throw new ArgumentException("Expected type: " + typeof(Customer));

			return GetCustomers();
		}
		
		public BaseTests()
		{
			this.assemblies = new Assembly[] { typeof(Customer).Assembly, typeof(BaseTests).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			this.resolver = new TypeResolver(assemblies, new Type[] 
			{ 
				typeof(Customer), typeof(Order), typeof(Product), 
				typeof(Supplier), typeof(Shipper) 
			});
			var creator = new QueryCreator(this.FnGetObjects);
			CustomExpressionXmlConverter queryconverter = new QueryExpressionXmlConverter(creator, this.resolver);
			CustomExpressionXmlConverter knowntypeconverter = new KnownTypeExpressionXmlConverter(this.resolver);
			this.serializer = new ExpressionSerializer(resolver, new CustomExpressionXmlConverter[] { queryconverter, knowntypeconverter });
		}
		protected Assembly[] assemblies;
		protected TypeResolver resolver;
		protected ExpressionSerializer serializer;
	}
	internal class SimpleClass
	{
		public Nullable<int> ID;
		public string Name;
	}

}
