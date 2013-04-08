using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Objects;

namespace ExpressionSerialization
{
    public static class DLinqSerializer
    {
        public static XElement SerializeQuery(this IQueryable query)
        {
            DLinqSerializationTypeResolver resolver = new DLinqSerializationTypeResolver(null);
            ExpressionSerializer serializer = new ExpressionSerializer(resolver) { Converters = { new DLinqCustomExpressionXmlConverter(null, resolver) } };
            return serializer.Serialize(query.Expression);
        }

        //public static IQueryable DeserializeQuery(this DataContext dc, XElement rootXml)
		public static IQueryable DeserializeQuery(this ObjectContext dc, XElement rootXml)
        {
            DLinqSerializationTypeResolver resolver = new DLinqSerializationTypeResolver(dc);
            DLinqCustomExpressionXmlConverter customConverter = new DLinqCustomExpressionXmlConverter(dc, resolver);
            ExpressionSerializer serializer = new ExpressionSerializer(resolver) { Converters = { customConverter } };
            Expression queryExpr = serializer.Deserialize(rootXml);
            // Query kind is populated by the ResolveXmlFromExpression method
            if (customConverter.QueryKind == null)
                throw new Exception(string.Format("CAnnot deserialize into DLinq query for datacontext {0} - no Table found", dc));
            return customConverter.QueryKind.Provider.CreateQuery(queryExpr);
        }

        private class DLinqSerializationTypeResolver : ExpressionSerializationTypeResolver
        {
			private ObjectContext dc;// DataContext dc;

            public DLinqSerializationTypeResolver(ObjectContext dc)//(DataContext dc)
            {
                this.dc = dc;
            }

            protected override Type ResolveTypeFromString(string typeString)
            {
                HashSet<Type> dataContextTableTypes = new HashSet<Type>();//dc.Mapping.GetTables().Select(mt => mt.RowType.Type));
                if (typeString.Contains('`'))
                    return null;
                if (typeString.Contains(','))
                    typeString.Substring(0, typeString.IndexOf(','));
                
                foreach (Type tableType in dataContextTableTypes)
                {
                    if (typeString.EndsWith(tableType.Name))
                        return tableType;
                    if (typeString.EndsWith(tableType.Name + "[]"))
                        return typeof(EntitySet<>).MakeGenericType(tableType);
                }
                return null;
            }

        }

       
    }
}
