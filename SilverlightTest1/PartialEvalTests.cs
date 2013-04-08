using Microsoft.VisualStudio.TestTools.UnitTesting;
using Northwind;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using System;
using ExpressionSerialization;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.IO;
using Microsoft.Silverlight.Testing;


namespace UnitTests
{
	/// <summary>
	///shows that even with non-Lambda expressions (i.e. PropertyExpression), 
	///that the intended behavior of Evaluator.PartialEval is to return a
	///ConstantExpression wherever possible.
	/// </summary>
	[TestClass]
	[Tag("PartialEval")]
	public class PartialEvalTests : UnitTests.BaseTests
	{

		[TestMethod]
		public void EvalMemberAccess()
		{
			MemberExpression m;
			PropertyInfo pinfo;
			Customer customer = Hans;

			pinfo = typeof(Customer).GetProperty("ID");
			m = Expression.Property(Expression.Constant(customer), pinfo);
			Expression e = Evaluator.PartialEval(m);
			ConstantExpression cx = (ConstantExpression)e;
		}

		[TestMethod]
		[Tag("PartialEval")]
		public void EvalBinary()
		{
			Customer c1 = Marc;
			Customer c2 = Hans;
			ConstantExpression cx;
			Expression e;
			BinaryExpression b, bsub;
			Nullable<int> id = -100;
			ParameterExpression p = Expression.Parameter(typeof(Customer));
			string propertyName = "ID";
			MemberExpression m = Expression.MakeMemberAccess(Expression.Constant(c1),
				typeof(Customer).GetProperty(propertyName));

			if (m.Type.Name == "Nullable`1")
				m = Expression.Property(m, "Value");

			bsub = Expression.MakeBinary(ExpressionType.Add, m, Expression.Constant(Hans.ID));
			b = Expression.MakeBinary(ExpressionType.Add, bsub, Expression.Constant(id));
			e = Evaluator.PartialEval(b);
			cx = (ConstantExpression)e;
		}

		/// <summary>
		/// shows a MethodCallExpression partial evaluates to a ConstantExpression, 
		/// here as int value.
		/// </summary>
		[TestMethod]
		[Tag("PartialEval")]
		public void EvalMethodCall()
		{
			ConstantExpression cx;
			IEnumerable<Customer> customers = GetCustomers();
			MethodCallExpression call = Expression.Call(typeof(System.Linq.Enumerable),
				"Count",
				new Type[] { typeof(Customer) }, Expression.Constant(customers));
			Expression e = Evaluator.PartialEval(call);
			cx = (ConstantExpression)e;
			int result = (int)Expression.Lambda(cx).Compile().DynamicInvoke(new object[0]);
		}

		/// <summary>
		/// I modified Evaluator.PartialEval to evaluate a zero-argument LambdaExpression
		/// as the invocation of that expression; 
		/// resulting in a ConstantExpression equal to the returned results of the lambda expression.
		/// 
		/// Later decided against this.
		/// </summary>
		[TestMethod]
		[Tag("PartialEval")]
		public void EvalZeroParametersLambda()
		{
			ConstantExpression cx;
			Func<List<int>> f13;
			List<int> list;
			Expression<Func<List<int>>> e13 = () => new List<int>() { 1, 2, 3 };
			LambdaExpression lambda13 = e13;
			Expression lambda13_evald = Evaluator.PartialEval(lambda13);
			cx = (ConstantExpression)lambda13_evald;
			f13 = (Func<List<int>>)cx.Value;			
			list = f13();
			//however a ConstantExpression with Value equal to LambdaExpression doesn't serialize well.
		}

	
	}

}
