using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

	
namespace Northwind
{	
	public partial class OrderCollection : List<Order>
 
	{	
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class SequencedObject 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public Northwind.SL.Security.SecureString SecureName { get; set; }	
			[DataMember]public System.Nullable<Northwind.SL.Security.PermissionTypeEnum> PermissionType { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class CustomerKey 
 
	{		[DataMember]public System.Nullable<System.Int64> Sequence { get; set; }	
			[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class ProductKey 
 
	{		[DataMember]public System.Nullable<System.Int64> Sequence { get; set; }	
			[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Product 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.Nullable<Northwind.SL.Security.PermissionTypeEnum> PermissionType { get; set; }	
			[DataMember]public Northwind.SL.Security.SecureString SecureName { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public Northwind.SL.Security.SecurityHandle SecurityHandle { get; set; }	
			[DataMember]public System.Nullable<System.Int32> SupplierID { get; set; }	
			[DataMember]public System.Nullable<System.Int32> CategoryID { get; set; }	
			[DataMember]public System.String QuantityPerUnit { get; set; }	
			[DataMember]public System.Nullable<System.Decimal> UnitPrice { get; set; }	
			[DataMember]public System.Nullable<System.Int16> UnitsInStock { get; set; }	
			[DataMember]public System.Nullable<System.Int16> UnitsOnOrder { get; set; }	
			[DataMember]public System.Nullable<System.Int16> ReorderLevel { get; set; }	
			[DataMember]public System.Nullable<System.Boolean> Discontinued { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public Northwind.Supplier Supplier { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Customer 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.String Code { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public Northwind.SL.Security.SecureString SecureName { get; set; }	
			[DataMember]public System.Nullable<Northwind.SL.Security.PermissionTypeEnum> PermissionType { get; set; }	
			[DataMember]public Northwind.SL.Security.SecurityHandle SecurityHandle { get; set; }	
			[DataMember]public System.String ContactName { get; set; }	
			[DataMember]public System.String ContactTitle { get; set; }	
			[DataMember]public System.String Address { get; set; }	
			[DataMember]public System.String City { get; set; }	
			[DataMember]public System.String Region { get; set; }	
			[DataMember]public System.String PostalCode { get; set; }	
			[DataMember]public System.String Country { get; set; }	
			[DataMember]public System.String Phone { get; set; }	
			[DataMember]public System.String Fax { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public System.Collections.Generic.List<Northwind.Order> Orders { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Shipper 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public Northwind.SL.Security.SecureString SecureName { get; set; }	
			[DataMember]public System.Nullable<Northwind.SL.Security.PermissionTypeEnum> PermissionType { get; set; }	
			[DataMember]public System.String Phone { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public System.Collections.Generic.List<Northwind.Order> Orders { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Supplier 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public Northwind.SL.Security.SecureString SecureName { get; set; }	
			[DataMember]public System.Nullable<Northwind.SL.Security.PermissionTypeEnum> PermissionType { get; set; }	
			[DataMember]public System.String ContactName { get; set; }	
			[DataMember]public System.String ContactTitle { get; set; }	
			[DataMember]public System.String Address { get; set; }	
			[DataMember]public System.String City { get; set; }	
			[DataMember]public System.String Region { get; set; }	
			[DataMember]public System.String PostalCode { get; set; }	
			[DataMember]public System.String Country { get; set; }	
			[DataMember]public System.String Phone { get; set; }	
			[DataMember]public System.String Fax { get; set; }	
			[DataMember]public System.String HomePage { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public System.Collections.Generic.List<Northwind.Product> Products { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Category 
 
	{		[DataMember]public System.Nullable<System.Int32> ID { get; set; }	
			[DataMember]public System.String Name { get; set; }	
			[DataMember]public System.String Description { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> Pictures { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public System.Collections.Generic.List<Northwind.Product> Products { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Employee 
 
	{		[DataMember]public System.Int32 ID { get; set; }	
			[DataMember]public System.String LastName { get; set; }	
			[DataMember]public System.String FirstName { get; set; }	
			[DataMember]public System.String Title { get; set; }	
			[DataMember]public System.String TitleOfCourtesy { get; set; }	
			[DataMember]public System.Nullable<System.DateTime> BirthDate { get; set; }	
			[DataMember]public System.Nullable<System.DateTime> HireDate { get; set; }	
			[DataMember]public System.String Address { get; set; }	
			[DataMember]public System.String City { get; set; }	
			[DataMember]public System.String Region { get; set; }	
			[DataMember]public System.String PostalCode { get; set; }	
			[DataMember]public System.String Country { get; set; }	
			[DataMember]public System.String HomePhone { get; set; }	
			[DataMember]public System.String Extension { get; set; }	
			[DataMember]public System.String Photo { get; set; }	
			[DataMember]public System.String Notes { get; set; }	
			[DataMember]public System.Nullable<System.Int32> ReportsTo { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamps { get; set; }	
			[DataMember]public System.Collections.Generic.List<Northwind.Order> Orders { get; set; }	
		
	}
}
	
namespace Northwind
{	
	[DataContract] public partial class Order 
 
	{		[DataMember]public System.Int32 ID { get; set; }	
			[DataMember]public System.Nullable<System.Int32> CustomerId { get; set; }	
			[DataMember]public System.Nullable<System.Int32> EmployeeID { get; set; }	
			[DataMember]public System.Nullable<System.DateTime> OrderDate { get; set; }	
			[DataMember]public System.Nullable<System.DateTime> RequiredDate { get; set; }	
			[DataMember]public System.Nullable<System.DateTime> ShippedDate { get; set; }	
			[DataMember]public System.Nullable<System.Int32> ShipperId { get; set; }	
			[DataMember]public System.Nullable<System.Decimal> Freight { get; set; }	
			[DataMember]public System.String ShipName { get; set; }	
			[DataMember]public System.String ShipAddress { get; set; }	
			[DataMember]public System.String ShipCity { get; set; }	
			[DataMember]public System.String ShipRegion { get; set; }	
			[DataMember]public System.String ShipPostalCode { get; set; }	
			[DataMember]public System.String ShipCountry { get; set; }	
			[DataMember]public System.Collections.Generic.List<System.Byte> RowTimeStamp { get; set; }	
			[DataMember]public Northwind.Customer Customer { get; set; }	
			[DataMember]public Northwind.Employee Employee { get; set; }	
			[DataMember]public Northwind.Shipper Shipper { get; set; }	
		
	}
}
	
namespace Northwind.SL.Security
{	
	public partial class SecureCollection<T> : List<T>
 
	{	
	}
}
	
namespace Northwind.SL.Security
{	
	public struct SecurityHandle 
 
	{		[DataMember]public System.String Domain { get; set; }	
		
	}
}
	
namespace Northwind.SL.Security
{	
	public struct SecureString 
 
	{		[DataMember]public Northwind.SL.Security.SecurityHandle SecurityHandle { get; set; }	
			[DataMember]public System.String Value { get; set; }	
		
	}
}
	
namespace Northwind.SL.Security
{	
	public enum PermissionTypeEnum 
 
	{		R = 0,
				W = 1,
				X = 2,
			
	}
}

