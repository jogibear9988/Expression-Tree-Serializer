using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Reflection;
using System.ServiceModel.Web;

namespace Northwind
{
	/// <summary>
	/// This derived interface exists because it's important to add these 
	/// KnownTypes to the ServiceContract, so that svcutil (esp. for Silverlight)
	/// will auto-generate the classes (so as not to require referencing the 
	/// actual .NET class library (i.e. NorthwindLibrary) which uses LINQ to SQL.
	/// 1. Derive from IObjectService and include the ServiceKnownTypeAttribute(s).
	/// 2. Run the service which uses this ServiceContract, 
	/// 3. Run svcutil.exe
	/// 4. svcutil.exe will generate new INorthwindService interface code.
	/// 5. Use that instead and reference the svcutil.exe from a separate class library, which
	/// can now be referenced by both the service and the client.
	/// </summary>
	[ServiceContract]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.OrderCollection))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.OrderCollection[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.NorthwindObject))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.NorthwindObject[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.CustomerKey))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.CustomerKey[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.ProductKey))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.ProductKey[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Product))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Product[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Customer))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Customer[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Shipper))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Shipper[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Supplier))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Supplier[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Category))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Category[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Employee))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Employee[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Order))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Order[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.SecurityHandle))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.SecurityHandle[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.SecureString))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.SecureString[]))]

	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.PermissionTypeEnum))]
	[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.SL.Security.PermissionTypeEnum[]))]

	//[System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Object[]))]
	//[System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Object))]


	//[System.ServiceModel.ServiceKnownTypeAttribute(typeof(NorthwindObject))]
	//[System.ServiceModel.ServiceKnownTypeAttribute(typeof(NorthwindObject[]))]	
	public interface INorthwindService : ExpressionSerialization.IQueryService, IClientAccessPolicy
	{
		[OperationContract]
		[WebInvoke(Method = "POST",
			UriTemplate = "/update",
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Json,
			BodyStyle = WebMessageBodyStyle.Bare)]
		//BusinessObjects.Orca.NorthwindObject UpdateObject(BusinessObjects.Orca.NorthwindObject updatedobject);
		object[] UpdateObjects(NorthwindObject[] updatedobjects);//BusinessObjects.Orca.NorthwindObject updatedobject);


		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(NorthwindObject))]
		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(NorthwindObject[]))]	
		[OperationContract]
		[WebInvoke(Method = "POST",
			UriTemplate = "/objects",
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Json,
			BodyStyle = WebMessageBodyStyle.Bare)]
		object[] GetObjects(params object[] args);


		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Employee))]
		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Employee[]))]
		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Order))]
		[System.ServiceModel.ServiceKnownTypeAttribute(typeof(Northwind.Order[]))]
		[OperationContract]
		[WebInvoke(Method = "POST",
			UriTemplate = "/object",
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml,
			BodyStyle = WebMessageBodyStyle.Bare)]
		object GetObject();


		//[OperationContract]
		//[WebInvoke(Method = "POST",
		//    UriTemplate = "/execute",
		//    RequestFormat = WebMessageFormat.Xml,
		//    ResponseFormat = WebMessageFormat.Xml,
		//    BodyStyle = WebMessageBodyStyle.Bare)]
		//object[] ExecuteQuery(System.Xml.Linq.XElement xml);
	}//end interface

}

