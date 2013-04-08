using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace ExpressionSerialization
{
	/// <summary>
	/// IQueryProvider that has a (remote) Web HTTPWCF service as a backing data source.
	/// (analagous to the RemoteTable class).
	/// </summary>
	public class RemoteProvider : QueryProvider
	{

		public RemoteProvider(WebHttpClient<IQueryService> client)
		{
			this.client = client;
			this.visitor = new StripQuoteVisitor();
			this.resolver = new TypeResolver(assemblies: null, knownTypes: client.knownTypes ?? new Type[0]);
			CustomExpressionXmlConverter queryconverter = new QueryExpressionXmlConverter(creator: null, resolver: resolver);
			CustomExpressionXmlConverter knowntypeconverter = new KnownTypeExpressionXmlConverter(resolver);			
			this.serializer = new ExpressionSerializer(resolver, new CustomExpressionXmlConverter[] { queryconverter, knowntypeconverter });
		}
		WebHttpClient<IQueryService> client;
		ExpressionSerializer serializer;
		TypeResolver resolver;
		StripQuoteVisitor visitor;
		public override string GetQueryText(System.Linq.Expressions.Expression expression)
		{
			return this.GetType().FullName;
		}

		/// <summary>
		/// This makes an asynchronous network request.
		/// Silverlight users can expect this call to return synchronously, 
		/// but will need to call this from a thread separate from the main UI thread.
		/// (For example, enclose this method call within ThreadPool.QueueUserWorkItem.)
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public override object Execute(Expression e)
		{
			
			if (e.NodeType == ExpressionType.Call)
			{
				Type returnType, elementType;				
				MethodCallExpression m = ((MethodCallExpression)e);
				if (m.Arguments[0] is ConstantExpression)
				{
					ConstantExpression cx = ((ConstantExpression)m.Arguments[0]);
					elementType = ((IQueryable)cx.Value).ElementType;
					if (typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(m.Method.ReturnType))
						returnType = elementType.MakeArrayType(); //typeof(IEnumerable<>).MakeGenericType(elementType);
					else
						throw new ArgumentException(string.Format("Expected for {0} of Type {1}\n Return Type of {2}", ((ConstantExpression)m.Arguments[0]), typeof(ConstantExpression), typeof(IEnumerable<>).MakeGenericType(elementType)));
				}								
				else
					returnType = m.Method.ReturnType;

				m = (MethodCallExpression)this.visitor.Visit(m);
				XElement x = serializer.Serialize(m);
				object result = client.SynchronousCall( svc => svc.ExecuteQuery(x), returnType);				
				return result;
			}
			else
				throw new ArgumentException("Exrpression expected: " + typeof(MethodCallExpression));
		}

		class StripQuoteVisitor : ExpressionVisitor
		{
			protected override Expression VisitMethodCall(MethodCallExpression m)
			{
				if (m.Method.DeclaringType == typeof(Queryable) && m.Arguments.Count == 2
					&& (m.Arguments[1] is UnaryExpression || m.Arguments[1].NodeType == ExpressionType.Quote))
				{
					ConstantExpression cx = (ConstantExpression)m.Arguments[0];
					//VisitUnary((UnaryExpression)m.Arguments[1]);
					Expression e =  base.VisitMethodCall(m);//strip quotes
					return e;
				}				
				return m;//else do not modify.
			}

			protected override Expression VisitUnary(UnaryExpression u)
			{
				LambdaExpression lambda = (LambdaExpression)StripQuotes(u);
				return lambda;
			}

			Expression StripQuotes(Expression e)
			{
				while (e.NodeType == ExpressionType.Quote)
				{
					e = ((UnaryExpression)e).Operand;
				}
				return e;
			}

		}
	}
}
