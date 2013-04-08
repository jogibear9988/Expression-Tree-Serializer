using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Northwind
{
	[ServiceContract]
	public interface IClientAccessPolicy
	{
		[OperationContract, System.ServiceModel.Web.WebGet(UriTemplate = "/clientaccesspolicy.xml")]
		System.IO.Stream GetClientAccessPolicy();
	}
	
}

