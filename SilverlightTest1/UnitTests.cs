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

namespace SilverlightTest1
{
	[TestClass]
    public class UnitTests
    {
		[TestMethod]
        public void ManyTests()
        {
			Order order1 = new Order { Customer = Hans, ID= 302,  Freight = 2001.99m, OrderDate = DateTime.Now.AddMonths(-20) } ;

			var assemblies = new Assembly[] { typeof(Order).Assembly, typeof(UnitTests).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			var resolver = new ExpressionSerializationTypeResolver(assemblies, new Type[] { typeof(Customer), typeof(Order), typeof(Product), typeof(Supplier), typeof(Shipper) });
            ExpressionSerializer serializer = new ExpressionSerializer(resolver);

            Console.WriteLine("\nTEST - 2");
            Expression<Func<int>> e2 = () => 1;
            XElement xml2 = serializer.Serialize(e2.Body);
            Expression result2 = serializer.Deserialize(xml2);
            Assert.AreEqual( e2.Body.ToString(), result2.ToString());

            Console.WriteLine("\nTEST - 3");
            Expression<Func<ExpressionType>> e3 = () => ExpressionType.Add;
            XElement xml3 = serializer.Serialize(e3.Body);
            Expression result3 = serializer.Deserialize(xml3);
            Assert.AreEqual( e3.Body.ToString(), result3.ToString());

            Console.WriteLine("\nTEST - 4");
            Expression<Func<bool>> e4 = () => true;
            XElement xml4 = serializer.Serialize(e4.Body);
            Expression result4 = serializer.Deserialize(xml4);
            Assert.AreEqual( e4.Body.ToString(), result4.ToString());

            Console.WriteLine("\nTEST - 5");
            Expression<Func<decimal, decimal>> e5 = d => d + 1m;
            XElement xml5 = serializer.Serialize(e5.Body);
            Expression result5 = serializer.Deserialize(xml5);
            Assert.AreEqual( e5.Body.ToString(), result5.ToString());

            Console.WriteLine("\nTEST - 6");
            Expression<Func<decimal, decimal>> e6 = d => d + 1m;
            XElement xml6 = serializer.Serialize(e6);
            Expression result6 = serializer.Deserialize(xml6);
            Assert.AreEqual( e6.ToString(), result6.ToString());
            Console.WriteLine(((result6 as Expression<Func<decimal, decimal>>).Compile())(3));

            Console.WriteLine("\nTEST - 7");
            Expression<Func<string, int>> e7 = s => int.Parse(s);
            XElement xml7 = serializer.Serialize(e7);
            Expression result7 = serializer.Deserialize(xml7);
            Assert.AreEqual( e7.ToString(), result7.ToString());
            Console.WriteLine(((result7 as Expression<Func<string, int>>).Compile())("1234"));

            Console.WriteLine("\nTEST - 8");
            Expression<Func<string, string>> e8 = s => s.PadLeft(4);
            XElement xml8 = serializer.Serialize(e8);
            Expression result8 = serializer.Deserialize(xml8);
            Assert.AreEqual( e8.ToString(), result8.ToString());
            Console.WriteLine(((result8 as Expression<Func<string, string>>).Compile())("1"));

            Console.WriteLine("\nTEST - 9");
            Expression<Func<string, int>> e9 = s => Foo<string, int>(s, 1);
            XElement xml9 = serializer.Serialize(e9);
            Expression result9 = serializer.Deserialize(xml9);
            Assert.AreEqual( e9.ToString(), result9.ToString());
            Console.WriteLine(((result9 as Expression<Func<string, int>>).Compile())("abcdac"));

            Console.WriteLine("\nTEST - 10");
            Expression<Func<string, char[]>> e10 = s => s.Where(c => c != 'a').ToArray();
            XElement xml10 = serializer.Serialize(e10);
            Expression result10 = serializer.Deserialize(xml10);
            Assert.AreEqual( e10.ToString(), result10.ToString());
            Console.WriteLine(((result10 as Expression<Func<string, char[]>>).Compile())("abcdac"));

            Console.WriteLine("\nTEST - 11");
            Expression<Func<string, char[]>> e11 =
                s =>
                    (from c in s
                     where c != 'a'
                     select (char)(c + 1)).ToArray();
            XElement xml11 = serializer.Serialize(e11);
            Expression result11 = serializer.Deserialize(xml11);
            Assert.AreEqual( e11.ToString(), result11.ToString());
            Console.WriteLine(((result11 as Expression<Func<string, char[]>>).Compile())("abcdac"));



            Console.WriteLine("\nTEST - 13");
            Expression<Func<List<int>>> e13 = () => new List<int>() { 1, 2, 3 };
            XElement xml13 = serializer.Serialize(e13);
            Expression result13 = serializer.Deserialize(xml13);
            Assert.AreEqual( e13.ToString(), result13.ToString());
            Console.WriteLine(((result13 as Expression<Func<List<int>>>).Compile())());

            Console.WriteLine("\nTEST - 14");
            Expression<Func<List<List<int>>>> e14 = () => new List<List<int>>() { new List<int>() { 1, 2, 3 }, new List<int>() { 2, 3, 4 }, new List<int>() { 3, 4, 5 } };
            XElement xml14 = serializer.Serialize(e14);
            Expression result14 = serializer.Deserialize(xml14);
            Assert.AreEqual( e14.ToString(), result14.ToString());
            Console.WriteLine(((result14 as Expression<Func<List<List<int>>>>).Compile())());

            Console.WriteLine("\nTEST - 15");
			Expression<Func<Customer>> e15 = () => new Customer() { Name = "Hans", Orders = { order1 } };
            XElement xml15 = serializer.Serialize(e15);
            Expression result15 = serializer.Deserialize(xml15);
            Assert.AreEqual( e15.ToString(), result15.ToString());
            Console.WriteLine(((result15 as Expression<Func<Customer>>).Compile())());

            Console.WriteLine("\nTEST - 16");
            Expression<Func<bool, int>> e16 = b => b ? 1 : 2;
            XElement xml16 = serializer.Serialize(e16);
            Expression result16 = serializer.Deserialize(xml16);
            Assert.AreEqual( e16.ToString(), result16.ToString());
            Console.WriteLine(((result16 as Expression<Func<bool, int>>).Compile())(false));

            Console.WriteLine("\nTEST - 17");
            Expression<Func<int, int[]>> e17 = n => new[] { n };
            XElement xml17 = serializer.Serialize(e17);
            Expression result17 = serializer.Deserialize(xml17);
            Assert.AreEqual( e17.ToString(), result17.ToString());
            Console.WriteLine(((result17 as Expression<Func<int, int[]>>).Compile())(7));

            Console.WriteLine("\nTEST - 18");
            Expression<Func<int, int[]>> e18 = n => new int[n];
            XElement xml18 = serializer.Serialize(e18);
            Expression result18 = serializer.Deserialize(xml18);
            Assert.AreEqual( e18.ToString(), result18.ToString());
            Console.WriteLine(((result18 as Expression<Func<int, int[]>>).Compile())(7));

            Console.WriteLine("\nTEST - 19");
            Expression<Func<object, string>> e19 = o => o as string;
            XElement xml19 = serializer.Serialize(e19);
            Expression result19 = serializer.Deserialize(xml19);
            Assert.AreEqual( e19.ToString(), result19.ToString());
            Console.WriteLine(((result19 as Expression<Func<object, string>>).Compile())(7));

            Console.WriteLine("\nTEST - 20");
            Expression<Func<object, bool>> e20 = o => o is string;
            XElement xml20 = serializer.Serialize(e20);
            Expression result20 = serializer.Deserialize(xml20);
            Assert.AreEqual( e20.ToString(), result20.ToString());
            Console.WriteLine(((result20 as Expression<Func<object, bool>>).Compile())(7));

            Console.WriteLine("\nTEST - 21");
            Expression<Func<IEnumerable<string>>> e21 = () => from m in typeof(string).GetMethods()
                                                              where !m.IsStatic
                                                              group m by m.Name into g
                                                              select g.Key + g.Count().ToString();

            XElement xml21 = serializer.Serialize(e21);
            Expression result21 = serializer.Deserialize(xml21);
            Assert.AreEqual( e21.ToString(), result21.ToString());
            Console.WriteLine(((result21 as Expression<Func<IEnumerable<string>>>).Compile())());

            Console.WriteLine("\nTEST - 22 (may take a while)");
            Expression<Func<IEnumerable<int>>> e22 = () => from a in Enumerable.Range(1, 13)
                                                           join b in Enumerable.Range(1, 13) on 4 * a equals b
                                                           from c in Enumerable.Range(1, 13)
                                                           join d in Enumerable.Range(1, 13) on 5 * c equals d
                                                           from e in Enumerable.Range(1, 13)
                                                           join f in Enumerable.Range(1, 13) on 3 * e equals 2 * f
                                                           join g in Enumerable.Range(1, 13) on 2 * (c + d) equals 3 * g
                                                           from h in Enumerable.Range(1, 13)
                                                           join i in Enumerable.Range(1, 13) on 3 * h - 2 * (e + f) equals 3 * i
                                                           from j in Enumerable.Range(1, 13)
                                                           join k in Enumerable.Range(1, 13) on 3 * (a + b) + 2 * j - 2 * (g + c + d) equals k
                                                           from l in Enumerable.Range(1, 13)
                                                           join m in Enumerable.Range(1, 13) on (h + i + e + f) - l equals 4 * m
                                                           where (4 * (l + m + h + i + e + f) == 3 * (j + k + g + a + b + c + d))
                                                           select a + b + c + d + e + f + g + h + i + j + k + l + m;
            XElement xml22 = serializer.Serialize(e22);
            Expression result22 = serializer.Deserialize(xml22);
            Assert.AreEqual( e22.ToString(), result22.ToString());
            Console.WriteLine(((result22 as Expression<Func<IEnumerable<int>>>).Compile())().FirstOrDefault());

            Console.WriteLine("\nTEST - 23");
            Expression<Func<int, int>> e23 = n => ((Func<int, int>)(x => x + 1))(n);
            XElement xml23 = serializer.Serialize(e23);
            Expression result23 = serializer.Deserialize(xml23);
            Assert.AreEqual( e23.ToString(), result23.ToString());
            Console.WriteLine(((result23 as Expression<Func<int, int>>).Compile())(7));


            Console.WriteLine("\nTEST - 24");
            Expression<Func<IEnumerable<int>>> e24 = () => from x in Enumerable.Range(1, 10)
                                                           from y in Enumerable.Range(1, 10)
                                                           where x < y
                                                           select x * y;
            XElement xml24 = serializer.Serialize(e24);
            Expression result24 = serializer.Deserialize(xml24);
            Assert.AreEqual( e24.ToString(), result24.ToString());
            Console.WriteLine(((result24 as Expression<Func<IEnumerable<int>>>).Compile())());

            Console.WriteLine("\nTEST - 25");
            Expression<Func<DateTime>> e25 = () => new DateTime(10000);
            XElement xml25 = serializer.Serialize(e25);
            Expression result25 = serializer.Deserialize(xml25);
            Assert.AreEqual( e25.ToString(), result25.ToString());
            Console.WriteLine(((result25 as Expression<Func<DateTime>>).Compile())());

        }

        public static int Foo<T>(T t)
        {
            return 1;
        }
        public static int Foo<T, U>(T t, U u)
        {
            return 2;
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
