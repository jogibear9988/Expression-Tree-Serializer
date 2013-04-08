using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Resolver = ExpressionSerialization.TypeResolver;

namespace ExpressionSerialization
{
	/// <summary>
	/// mock client QueryProvider class to simulate
	/// calls against remote WCF service.
	/// </summary>
	public class ClientQueryProvider : QueryProvider
	{
		
		public override string GetQueryText(Expression expression)
		{
			return string.Empty;
		}

		public override object Execute(Expression expression)
		{
			if (expression.NodeType == ExpressionType.Call)
			{
				MethodCallExpression m = ((MethodCallExpression)expression);
				ConstantExpression cx = (ConstantExpression)m.Arguments[0];
				LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
				throw new NotImplementedException("Make call to remote WCF service.");
			}
			else
				throw new ArgumentException("Expected: " + typeof(MethodCallExpression));
		}
		private static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
			{
				e = ((UnaryExpression)e).Operand;
			}
			return e;
		}
	}
}