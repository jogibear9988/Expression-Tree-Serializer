using System;
using ExpressionSerialization;
using Northwind;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
	public class CustomerDAL : IDAL<Customer>
	{


		#region IDAL<Customer> Members

		public IEnumerable<Customer> GetObjects()
		{
			return this.GetObjects().Cast<Customer>().ToArray();
		}

		#endregion

		#region IDAL Members

		public System.Collections.Generic.IEnumerable<object> GetObjects(Type elementType)
		{
			return this.GetCustomers().Cast<object>().ToArray(); 
		}

		#endregion


		static List<Order> OrderList =
			new List<Order> {
                        new Order {
                            ShipAddress = "address 0",
                            Freight = 5
                        },
                        new Order {
                            ShipAddress = "address 1",
                            Freight = 123
                        },
						new Order() {
                            ShipAddress = "city 0",
                            Freight = 5
                        },
                        new Order() {
                            ShipAddress = "place 2",
                            Freight = 199
                        }
			};


		Customer Hans = new Customer { Name = "Hans Passant", City = "Cambridge", Country = "UK", ID = 199, Orders = OrderList };
		Customer Marc = new Customer { Name = "Marc Gravell", City = "Seattle", Country = "USA", ID = 111, Orders = OrderList };

		IEnumerable<Customer> GetCustomers()
		{
			Customer[] customers = new Customer[] {
				Hans,
				Marc,
                new Customer { 
                    ID = -111,
                    Name = "Bob",
                    Orders = OrderList,
				},
                new Customer { 
                    ID = 222,
                    Name = "Dave",
                    Orders = OrderList
                 },
                new Customer { 
                    ID = -3333,
                    Name = "Abigail",
                    Orders = OrderList
                    }
            };
			return customers;
		}


	}
}
