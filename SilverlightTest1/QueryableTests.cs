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


namespace UnitTests
{
	[TestClass]
	public class QueryableTests : BaseTests
	{
		public QueryableTests()
			:base()
		{

		}

		/// <summary>
		/// We want to be able to call the standard LINQ IQueryProvider
		/// on any arbitrary Expression.
		/// 
		/// Create the IQueryProvider by calling Queryable.AsQueryable on
		/// a string collection to create a Linq.EnumerableQuery`1, 
		/// then get its .Provider property.
		/// 
		/// The call to IQueryProvider.Execute will fail if the input Expression
		/// does not match the return type. e.g. calling Execute without
		/// specifying IEnumerable`1 return type (generic arg) will fail.
		/// </summary>
		[TestMethod]
		[Tag("System.Linq.EnumerableQuery`1")]
		[Tag("IQueryable")]
		[Tag("IQueryProvider")]
		public void QueryProvider_Execute1()
		{
			IEnumerable<string> data = GetStrings();
			IQueryable<string> query1 = data.AsQueryable<string>();
			IQueryable<string> query2;
			IQueryProvider provider;

			provider = query1.Provider;
			query2 = provider.CreateQuery<string>(query1.Expression);//create IQueryable<string>
			query2.ToList();//force call to Execute

			try
			{
				provider.Execute(query1.Expression);//this will throw error
			}
			catch
			{
				provider.Execute<IEnumerable<string>>(query1.Expression);//the generic type arg is necessary.
			}			
			
		}


		[TestMethod]
		[Tag("System.Linq.EnumerableQuery`1")]
		[Tag("IQueryable")]
		[Tag("IQueryProvider")]
		public void QueryProvider_Execute2()
		{
			object result;
			IQueryable<Customer> query1 = GetCustomers().AsQueryable();
			IQueryable<Customer> query2 = query1.Where(c => c.ID > 0);
			IQueryProvider provider;

			provider = query1.Provider;
			//provider.Execute(query1.Expression); //does not work

			query2 = provider.CreateQuery<Customer>(query1.Expression);
			query2.ToList();
			provider.Execute<IEnumerable<Customer>>(query1.Expression);
			//client has remote WCF IQueryProvider
			//server has actual data (e.g. RequestProcessor) IQueryProvider.

			//Query<Customer> query1 = new Query<Customer>(provider, query1.Expression);
			IQueryable<Customer> queryable = query1.Where(c => c.ID > 0);


			query1.Provider.Execute<IQueryable<Customer>>(query1.Expression);
			result = query1.Provider.Execute(query1.Expression);//fails because customers.Expression as a ConstantExpression yields IEnumerable<Customer> not EnumerableQuery<Customer>			
			result = query1.Provider.Execute(query2.Expression);

			result = queryable.Provider.Execute(Expression.Constant((IEnumerable<Customer>)query1.AsEnumerable()));
			var list = query1.ToList();

		}


		/// <summary>
		/// demonstrate how to use Query and how IQueryProvider.CreateQuery
		/// is called.
		/// </summary>
		[TestMethod]
		[Tag("Query")]
		[Tag("IQueryable")]
		[Tag("IQueryProvider")]
		public void IQueryProvider_CreateQuery1()
		{
			IQueryProvider clientprovider = null;

			//Expression is ConstantExpression:
			Query<Customer> clientquery = new Query<Customer>(clientprovider);
			Assert.IsTrue(clientquery is IQueryable);
			Assert.IsTrue(((IQueryable)clientquery).Expression is ConstantExpression);

			//This will NOT force call to IQueryProvider.CreateQuery
			clientquery.AsQueryable();

			//This next statement WILL call IQueryProvider.CreateQuery.
			//Expression is MethodCallExpression:
			IQueryable<Customer> clientqueryable = from c in clientquery
												   where c.ID > 0
												   select c;
			Assert.IsTrue(clientqueryable.Expression is MethodCallExpression);
		}

		/// <summary>
		/// Create a Query (of string) with another IQueryable's IQueryProvider. 
		/// Then call IQueryProvider.Execute
		/// </summary>
		[TestMethod]
		[Tag("Query")]
		[Tag("IQueryProvider")]
		public void Query_Execute1()
		{
			IEnumerable<string> data = GetStrings();
			IQueryable<string> queryable = data.AsQueryable<string>();
			IQueryProvider provider = queryable.Provider;//IQueryProvider: Linq.EnumerableQuery`1

			//create the Query
			IQueryable<string> query = new Query<string>(provider, queryable.Expression);

			//Execute the Query with (borrowed) queryable.Provider
			//IQueryProvider: Linq.EnumerableQuery`1
			//Expression is a ConstantExpression
			IEnumerable<string> executedresults = query.Provider.Execute<IEnumerable<string>>(queryable.Expression);

			//Expression is MethodCallExpression.
			//Return type is IEnumerable`1
			query = from s in query
					where s.StartsWith("H") || s.StartsWith("J")
					select s;
			executedresults = query.Provider.Execute<IEnumerable<string>>(query.Expression);


			//Create a MethodCallExpression for Queryable.Count
			//Return type is int
			IEnumerable<MethodInfo> methods = from m in typeof(Queryable).GetMethods()
								where m.Name == "Count" && m.IsGenericMethod
								select m;
			MethodInfo method = methods.First();
			MethodInfo generic = method.MakeGenericMethod(typeof(string));
			MethodCallExpression countCall = Expression.Call(null,//static
				generic, new Expression[] { Expression.Constant(query)});

			//check that the MethodCallExprssion is valid.
			LambdaExpression lambda = Expression.Lambda(countCall, 
				Expression.Parameter(typeof(IQueryable)));//I'm not even sure this is the correct parameter Type!
			int count = (int)lambda.Compile().DynamicInvoke(new object[] {  query });

			//call IQueryProvider.Execute:
			count = (int)query.Provider.Execute<int>(countCall);
			Assert.AreEqual(query.Count(), count);
		}

			/// <summary>
		/// Create a Query (of  Customer) with another IQueryable's IQueryProvider. 
		/// Then call IQueryProvider.Execute
		/// </summary>
		[TestMethod]
		[Tag("Query")]
		[Tag("IQueryProvider")]
		public void Query_Execute2()
		{
			IEnumerable<Customer> data = GetCustomers();
			IQueryable<Customer> queryable = data.AsQueryable<Customer>();
			IQueryProvider provider = queryable.Provider;//IQueryProvider: Linq.EnumerableQuery`1
			IEnumerable<Customer> executedresults;

			//create the Query
			IQueryable<Customer> query = new Query<Customer>(provider, queryable.Expression);

			//Execute the Query with (borrowed) queryable.Provider
			//IQueryProvider: Linq.EnumerableQuery`1
			//Expression is a ConstantExpression
			query.Provider.Execute<IEnumerable<Customer>>(queryable.Expression);

			//Execute on ConstantExpression of IEnumerable`1 
			//instead of Linq.EnumerableQuery`1. Works.
			provider.Execute<IEnumerable<Customer>>(Expression.Constant(GetCustomers()));

			//Expression is MethodCallExpression.
			//Return type is IEnumerable`1
			query = from c in query
					where c.Name.StartsWith("H") || c.Name.StartsWith("J")
					select c;
			executedresults = query.Provider.Execute<IEnumerable<Customer>>(query.Expression);

			//re-create the Query, but this time with no Expression in constructor:
			query = new Query<Customer>(provider);//ConstantExpression has no data.

			//Expression is MethodCallExpression.
			//Arguments[0]: query: [System.Linq.Expressions.ConstantExpression]: {ExpressionSerialization.Query`1
			//Arguments[1]: Unary (Quote) or LambdaExpression
			query = from c in query				// call IQueryProvider.CreateQuery
					where c.Name != null		//but query (ConstantExpression) has no data.
					select c;

			//Even though we'd think the IQueryProvider (Linq.EnumerableQuery`1) has data,
			//the MethodCallExpression will execute with ConstantExpression (query)
			//as Argument 0. But Argument 0 (the no-data Query) we just created will
			//be evaluated as a ConstantExpression with Value != IEnumerable`1.
			executedresults = query.Provider.Execute<IEnumerable<Customer>>(query.Expression);

			//So this will throw exception:
			//var list = executedresults.ToList();	

			//To make the Execute call work, we'd need to either
			//1: instantiate Query with a ConstantExpression equal to actual IEnumerable`1
			//2: ExpressionVisitor.Visit the MethodCallExpression, and swap
			//Argument 0 with an actual data source (IEnumerable`1).				
			query = new Query<Customer>(provider, Expression.Constant(GetCustomers()));
			provider.Execute<IEnumerable<Customer>>(query.Expression);			
		}

		/// <summary>
		/// demonstrate how to use Query and how IQueryProvider.CreateQuery
		/// is called.
		/// </summary>
		[TestMethod]
		[Tag("Query")]
		[Tag("IQueryable")]
		[Tag("IQueryProvider")]
		public void IQueryProvider_CreateQuery2()
		{
			IEnumerable<Customer> data = GetCustomers();
			IQueryable<Customer> queryable = data.AsQueryable<Customer>();
			IQueryProvider EnumerableQueryProvider = queryable.Provider;//IQueryProvider: Linq.EnumerableQuery`1			
			IEnumerable<Customer> execute;
			IEnumerable<Customer> executeList;
			//This next statement WILL call IQueryProvider.CreateQuery.
			//Expression is MethodCallExpression:
			IQueryable<Customer> query = new Query<Customer>(default(IQueryProvider));
			query = from c in query
					where c.ID != null
					select c;
			
			execute = EnumerableQueryProvider.Execute<IEnumerable<Customer>>(query.Expression);
			try
			{
				executeList = execute.ToList();
			}
			catch { }

			//TryDeserialize:
			query = new Query<Customer>(EnumerableQueryProvider, Expression.Constant(queryable));
			query = from c in query
					where c.ID != null
					select c;
			execute = EnumerableQueryProvider.Execute<IEnumerable<Customer>>(query.Expression);
			executeList = execute.ToList();
		}

		[TestMethod()]
		[Tag("Query")]
		[Tag("QueryCreator")]
		[Tag("IQueryable")]
		[Tag("IQueryProvider")]
		public void CreateNewQuery_Execute()
		{
			QueryCreator creator = new QueryCreator();
			Type elementType = typeof(Customer);
			Query<Customer> query = creator.CreateQuery(elementType);
			IQueryable<Customer> queryable;

			queryable = from c in query
					where c.ID == null //initialized with properties == null
					select c;
			IQueryProvider provider = ((IQueryable)query).Provider;

			//execute:
			var ienumerable = provider.Execute<IEnumerable<Customer>>(queryable.Expression);
			var list = ienumerable.ToList();

			//new QueryCreator:
			creator = new QueryCreator(this.FnGetObjects);
			query = creator.CreateQuery(elementType);
			queryable = from c in query
						where c.ID == null //initialized with properties == null
						select c;
			provider.Execute<IEnumerable<Customer>>(queryable.Expression).ToList();
		}

		
		[TestMethod]
		[Tag("System.Linq.EnumerableQuery`1")]
		[Tag("IQueryable")]
		[Tag("CustomExpressionXmlConverter")]
		[Tag("QueryCreator")]
		public void QueryExpressionXmlConverterTest()
		{
			Type elementType = typeof(Customer);
			var creator = new QueryCreator(this.FnGetObjects);
			var converter = new QueryExpressionXmlConverter(creator, this.resolver);
			bool success;
			XElement x;
			Expression e;
			MethodCallExpression m;
			ConstantExpression cx;
			IQueryProvider provider;

			//the client Query. IQueryProvider has not real data.
			Query<Customer> query = new Query<Customer>(default(IQueryProvider));
			provider = ((IQueryable)query).Provider;
			IQueryable<Customer> queryable;			
			queryable = from c in query
						where c.ID == null //initialized with properties == null
						select c;

			//serialize to XML
			e = Expression.Constant(queryable);
			success = converter.TrySerialize(e, out x);

			//deserialize to ConstantExpression(Query)			
			success = converter.TryDeserialize(x, out e);
			cx = (ConstantExpression)e;
			Query<Customer> serverquery = (Query<Customer>)cx.Value;
			//upon deserialization, should have a new IQueryProvider
			Assert.AreNotEqual(provider, ((IQueryable)serverquery).Provider);
			provider = ((IQueryable)serverquery).Provider;
			
			//Execute Query on server side.
			int count = serverquery.Count();

		}

		/// <summary>
		/// now, actually attempt to serialize IQueryable Expression
		/// 
		/// </summary>
		[TestMethod]
		[Tag("System.Linq.EnumerableQuery`1")]
		[Tag("IQueryable")]
		[Tag("ExpressionSerializer")]
		public void EnumerableQueryExpressionSerialize()
		{
			XElement xml;
			Expression e;
			IEnumerable<Customer> customers = GetCustomers();
			Query<Customer> query = new Query<Customer>(default(IQueryProvider));
			IQueryable<Customer> queryable = from c in query
										  where c.ID >= 0
										  select c;
			//serialize the Expression to WCF remote service:
			xml = serializer.Serialize(queryable.Expression);


			//deserialize the XML to Expression on WCF server side:
			e = serializer.Deserialize(xml);
			MethodCallExpression m = (MethodCallExpression)e;

			//now simply invoke the MethodCallExpression
			LambdaExpression lambda = Expression.Lambda(m);
			Delegate fn = lambda.Compile();
			dynamic result = fn.DynamicInvoke(new object[0]);
			Assert.AreEqual("EnumerableQuery`1", result.GetType().Name);

			dynamic array = Enumerable.ToArray(result);
			Assert.IsTrue(array is Customer[]);
		}

		#region Resolved
		/// <summary>
		/// Initial test during run-in with issue with System.Linq.EnumerableQuery`1, 
		/// typical during "Where" LINQ method calls. 
		/// The original code (even in .NET 3.5) could not handle this scenario.
		/// 
		/// This shows that the solution WAS to serialize the System.Linq.EnumerableQuery
		/// to IEnumerable`1.
		/// 
		/// Now we use QueryExpressionXmlConverter to send Query Type of IQueryable 
		/// across the wire. For plain System.Linq.EnumerableQuery`1 the user should 
		/// just take responsiblity for using IEnumerable`1 instead.
		/// </summary>
		//[TestMethod]
		//[Tag("Type")]
		//[Tag("System.Linq.EnumerableQuery`1")]
		//[Tag("IQueryable")]
		//public void EnumerableQuery_Queryable_Test()
		//{
		//    //The following Type(s) not System.Linq.EnumerableQuery`1:
		//    //Type iqueryableType = typeof(IQueryable<Customer>);
		//    //Type existing = typeof(Customer);
		//    //Type queryableGeneric = typeof(IQueryable<>).MakeGenericType(existing);

		//    IEnumerable<Customer> customers = GetCustomers();
		//    IQueryable<Customer> queryable = customers.AsQueryable<Customer>();
		//    IQueryable<Customer> query = from c in queryable
		//                                 where c.ID >= 0
		//                                 select c;

		//    //Is the System.Linq.EnumerableQuery<Customer> a KnownType, or mapped to one in TypeResolver?
		//    Type knownType;
		//    bool canSerialize, hasKnownType;
		//    Type enumerableQueryType = queryable.Expression.Type;
		//    hasKnownType = this.resolver.HasMappedKnownType(enumerableQueryType, out knownType);
		//    Assert.IsTrue(hasKnownType);

		//    //Attempt to DataContractSerializer-serialize  IQueryable...
		//    canSerialize = this.CanSerialize(queryable, new Type[] { enumerableQueryType, knownType });
		//    Assert.AreEqual(false, canSerialize, "did not expect to serialize " + queryable.Expression.Type);


		//    //Cast to what can serialize...
		//    Assert.IsTrue(typeof(IQueryable).IsAssignableFrom(enumerableQueryType));
		//    if (typeof(IQueryable).IsAssignableFrom(enumerableQueryType))
		//    {
		//        System.Collections.IEnumerable enumerable
		//        = LinqHelper.CastToGenericEnumerable(queryable, knownType);
		//        Assert.IsTrue(enumerable is IEnumerable<Customer>);
		//        canSerialize = CanSerialize(enumerable);//IEnumerable<T> cannot serialize.
		//        var list = ((IEnumerable<Customer>)enumerable).ToList();
		//        canSerialize = CanSerialize(list);
		//        Assert.IsTrue(canSerialize);
		//        dynamic array = enumerable;			//I like Arrays better here.
		//        array = Enumerable.ToArray(array);
		//        Assert.IsTrue(CanSerialize(array));
		//    }
		//    else
		//        Assert.Fail("!typeof(IQueryable).IsAssignableFrom");

		//}
		#endregion
	

	}
}
