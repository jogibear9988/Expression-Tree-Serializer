using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;
using System.ServiceModel;
using System.ServiceModel.Web;
using ExpressionSerialization;
using System.Reflection;
using System.Linq.Expressions;

namespace Northwind
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class NorthwindService : INorthwindService
	{
		
		public NorthwindService()
		{
			var connectionStrings = System.Configuration.ConfigurationManager.ConnectionStrings;
			this.connectionString = connectionStrings["northwind"].ConnectionString;
			this.creator = new QueryCreator(this.FnGetDatabaseObjects);
			this.assemblies = new Assembly[] { typeof(Customer).Assembly, typeof(ExpressionType).Assembly, typeof(IQueryable).Assembly };
			this.resolver = new TypeResolver(assemblies, new Type[] 
			{ 
				typeof(Customer), typeof(Order), typeof(Product), 
				typeof(Supplier), typeof(Shipper) 
			});			
			CustomExpressionXmlConverter queryconverter = new QueryExpressionXmlConverter(creator, this.resolver);
			CustomExpressionXmlConverter knowntypeconverter = new KnownTypeExpressionXmlConverter(this.resolver);
			this.serializer = new ExpressionSerializer(resolver, new CustomExpressionXmlConverter[] { queryconverter, knowntypeconverter });			
		}
		string connectionString;
		QueryCreator creator;
		ExpressionSerializer serializer;
		TypeResolver resolver;
		Assembly[] assemblies;

		#region INorthwindService Members

		public object[] UpdateObjects(NorthwindObject[] updatedobjects)
		{
			throw new NotImplementedException();
		}

		public object[] GetObjects(params object[] args)
		{
			throw new NotImplementedException();
		}

		public object[] ExecuteQuery(System.Xml.Linq.XElement xml)
		{
			Expression e = this.serializer.Deserialize(xml);
			MethodCallExpression m = (MethodCallExpression)e;
			LambdaExpression lambda = Expression.Lambda(m);
			Delegate fn = lambda.Compile();
			dynamic result = fn.DynamicInvoke(new object[0]);
			//dynamic array = Enumerable.ToArray(result);			
			var array = Enumerable.ToArray(Enumerable.Cast<NorthwindObject>(result));
			return array;
		}

		public object GetObject()
		{
			using (DbConnection dbconnection = new System.Data.SqlClient.SqlConnection(this.connectionString))
			{
				dbconnection.Open();
				var processor = new DbQueryProcessor(dbconnection);
				return processor.Execute<Order>().First();
			}
			
		}
		#endregion

		dynamic FnGetDatabaseObjects(Type elementType)
		{
			using (DbConnection dbconnection = new System.Data.SqlClient.SqlConnection(this.connectionString))
			{
				dbconnection.Open();
				var processor = new DbQueryProcessor(dbconnection);
				return processor.Execute(elementType);
			}
		}


		#region IClientAccessPolicy Members
		[System.ServiceModel.OperationBehavior]
		public System.IO.Stream GetClientAccessPolicy()
		{
			const string result = @"<?xml version=""1.0"" encoding=""utf-8""?>
<access-policy>
    <cross-domain-access>
        <policy>
            <allow-from http-request-headers=""*"">
                <domain uri=""*""/>
            </allow-from>
            <grant-to>
                <resource path=""/"" include-subpaths=""true""/>				
            </grant-to>
        </policy>
    </cross-domain-access>
</access-policy>";
			//<socket-resource port=""4502-4534"" protocol=""tcp"" />
			if (System.ServiceModel.Web.WebOperationContext.Current != null)
				System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
			return new System.IO.MemoryStream(Encoding.UTF8.GetBytes(result));
		}
		#endregion
        
		static void Main()
		{
			//Uri baseAddress = new Uri("http://localhost:8999/");
			using (var host = new WebServiceHost(typeof(NorthwindService)))
			{
				host.Open();
				string baseAddress = host.BaseAddresses.FirstOrDefault().AbsoluteUri ?? "not in .config file";
				Console.WriteLine("Service {0} started.\n\tAddress:\n\t{1}", typeof(NorthwindService).FullName, baseAddress);
				Console.ReadLine();
			}
		}
	}
}
