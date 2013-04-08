using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Data.Objects;

namespace ExpressionSerialization
{
	internal class DLinqCustomExpressionXmlConverter : CustomExpressionXmlConverter
	{
		//private DataContext dc;
		private ObjectContext dc;
		private ExpressionSerializationTypeResolver resolver;
		public IQueryable QueryKind { get; private set; }

		public DLinqCustomExpressionXmlConverter(ObjectContext dc, ExpressionSerializationTypeResolver resolver)
		{
			this.dc = dc;
			this.resolver = resolver;
		}

		public override Expression Deserialize(XElement expressionXml)
		{
			if (expressionXml.Name.LocalName == "Table")
			{
				Type type = resolver.GetType(expressionXml.Attribute("Type").Value);
				ITable table = null;// dc.GetTable(type);
				// REturning a random IQueryable of the right kind so that we can re-create the IQueryable
				// instance at the end of this method...
				QueryKind = table;
				return Expression.Constant(table);
			}
			return null;
		}

		public override XElement Serialize(Expression expression)
		{
			if (typeof(IQueryService).IsAssignableFrom(expression.Type))
			{
				return new XElement("Table",
					new XAttribute("Type", expression.Type.GetGenericArguments()[0].FullName));
			}
			return null;
		}
	}
}
