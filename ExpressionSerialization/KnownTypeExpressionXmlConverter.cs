using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;
using System.Text;

namespace ExpressionSerialization
{
	public class KnownTypeExpressionXmlConverter : CustomExpressionXmlConverter
	{

		public KnownTypeExpressionXmlConverter(TypeResolver @resolver)
		{
			this.resolver = @resolver;
		}
		TypeResolver resolver;

		/// <summary>
		///code originally in method ParseConstantFromElement(XElement xml, string elemName, Type expectedType)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		public override bool TryDeserialize(XElement x, out Expression e)
		{							
			if (x.Name.LocalName == typeof(ConstantExpression).Name)
			{
				Type serializedType = resolver.GetType(x.Element("Type").Value);
				if (resolver.HasMappedKnownType(serializedType))
				{
					string xml = x.Element("Value").Value;					
					DataContractSerializer dserializer;
					using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
					{
						//if (typeof(IQueryable).IsAssignableFrom(expectedType) && IsIEnumerableOf(expectedType, knownType))
						//{
						//    dserializer = new DataContractSerializer(knownType.MakeArrayType(), this.resolver.knownTypes);
						//    result = dserializer.ReadObject(ms);
						//    result = Enumerable.ToArray(result);
						//}					
						dserializer = new DataContractSerializer(serializedType, this.resolver.knownTypes);
						object instance = dserializer.ReadObject(ms);
						e = Expression.Constant(instance);
						return true;
					}
				}				
			}			

			e = null;
			return false;
						
		}

		public override bool TrySerialize(Expression e, out XElement x)
		{
			//!Query`1 && !System.Linq.EnumerableQuery`1
			if (e.NodeType == ExpressionType.Constant && !typeof(IQueryable).IsAssignableFrom(e.Type))
			{
				ConstantExpression cx = (ConstantExpression)e;
				Type knownType;
				Type actualType = cx.Type;
				if (cx.Value != null && cx.Type != cx.Value.GetType())
				{
					actualType = cx.Value.GetType();
					//either convert Nullable`1 (cx.Type) to the actual ValueType
					//or convert the actual ValueType to the Nullable`1
					//UnaryExpression u = Expression.Convert(cx, cx.Type);
					//LambdaExpression lambda = Expression.Lambda(u);
					//Delegate fn = lambda.Compile();
					//object result = fn.DynamicInvoke(new object[0]);
					//cx = Expression.Constant(result);
				}				
				if (this.resolver.HasMappedKnownType(actualType, out knownType))
				{
					object instance = cx.Value;
					DataContractSerializer serializer = new DataContractSerializer(actualType, this.resolver.knownTypes);
					using (MemoryStream ms = new MemoryStream())
					{
						serializer.WriteObject(ms, instance);
						ms.Position = 0;
						StreamReader reader = new StreamReader(ms, Encoding.UTF8);
						string xml = reader.ReadToEnd();
						x = new XElement(typeof(ConstantExpression).Name,
							new XAttribute("NodeType", actualType),
							new XElement("Type", cx.Type),
							new XElement("Value", xml));

						return true;
					}
				}
			}

			x = null;
			return false;						
			//if (typeof(IQueryable).IsAssignableFrom(instance.GetType()))
			//{
			//    if (typeof(Query<>).MakeGenericType(knownType).IsAssignableFrom(instance.GetType()))
			//    {
			//        return instance.ToString();
			//    }
			//    something = LinqHelper.CastToGenericEnumerable((IQueryable)instance, knownType);
			//    something = Enumerable.ToArray(something);
			//}
	
		}
	}
}