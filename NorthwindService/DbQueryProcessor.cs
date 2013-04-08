using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;

namespace Northwind
{
	public class DbQueryProcessor
	{
		DbConnection connection;

		public DbQueryProcessor(DbConnection connection)
		{
			this.connection = connection;
		}
		public System.Collections.IEnumerable Execute(Type elementType)
		{
			DbCommand cmd = this.connection.CreateCommand();
			cmd.CommandText = string.Format("SELECT * FROM dbo.[{0}]", elementType.Name);
			DbDataReader reader = cmd.ExecuteReader();
			dynamic instance = Activator.CreateInstance(
				typeof(ObjectReader<>).MakeGenericType(elementType),
				BindingFlags.Instance | BindingFlags.NonPublic, null,
				new object[] { reader },
				null);

			Type expectedType = typeof(IEnumerable<>).MakeGenericType(elementType);
			if (!expectedType.IsAssignableFrom(instance.GetType()))
				throw new InvalidOperationException("Expected Type: " + expectedType);

			dynamic array = Enumerable.ToArray(instance);//execute at once, while the db connection is open.
			return array;
		}

		public IEnumerable<T> Execute<T>()
		{
			Type elementType = typeof(T);
			return this.Execute(elementType).Cast<T>();
		}

	}
}
