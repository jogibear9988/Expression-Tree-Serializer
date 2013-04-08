using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;
using ExpressionSerialization;
//using Northwind;
using NorthwindMapping;

[ServiceContract]
public interface INorthwindService
{
    [OperationContract]
    Customer[] ExecuteQueryForCustomers(XElement xml);
    [OperationContract]
    Order[] ExecuteQueryForOrders(XElement xml);
    [OperationContract]
    object[] ExecuteQueryForObjects(XElement xml);
    [OperationContract]
    Customer ExecuteQueryForCustomer(XElement xml);
    [OperationContract]
    Order ExecuteQueryForOrder(XElement xml);
    [OperationContract]
    object ExecuteQueryForObject(XElement xml);
}

public class NorthwindService : INorthwindService
{
    public Customer[] ExecuteQueryForCustomers(XElement xml)
    {
		//NorthwindDataContext db = new NorthwindDataContext();
		//IQueryable queryAfter = db.DeserializeQuery(xml);
		//return queryAfter.Cast<Customer>().ToArray();
		throw new NotImplementedException();
    }

    public Order[] ExecuteQueryForOrders(XElement xml)
    {
		//NorthwindDataContext db = new NorthwindDataContext();
		//IQueryable queryAfter = db.DeserializeQuery(xml);
		//return queryAfter.Cast<Order>().ToArray();
		throw new NotImplementedException();
    }

    public object[] ExecuteQueryForObjects(XElement xml)
    {
		NorthwindEntities db = new NorthwindEntities();		
		IQueryable queryAfter = db.DeserializeQuery(xml);
		return queryAfter.Cast<object>().ToArray();
		throw new NotImplementedException();
    }

    public Customer ExecuteQueryForCustomer(XElement xml)
    {
		//NorthwindDataContext db = new NorthwindDataContext();
		//IQueryable queryAfter = db.DeserializeQuery(xml);
		//return (Customer)queryAfter.Provider.Execute(queryAfter.Expression);
		throw new NotImplementedException();
    }

    public Order ExecuteQueryForOrder(XElement xml)
    {
		//Debugger.Launch();
		//NorthwindDataContext db = new NorthwindDataContext();
		//IQueryable queryAfter = db.DeserializeQuery(xml);
		//return (Order)queryAfter.Provider.Execute(queryAfter.Expression);
		throw new NotImplementedException();
    }

    public object ExecuteQueryForObject(XElement xml)
    {
		//NorthwindDataContext db = new NorthwindDataContext();
		//IQueryable queryAfter = db.DeserializeQuery(xml);
		//return queryAfter.Provider.Execute(queryAfter.Expression);
		throw new NotImplementedException();
    }
}