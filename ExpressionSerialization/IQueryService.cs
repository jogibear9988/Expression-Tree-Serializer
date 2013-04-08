using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;
using System.Xml.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ExpressionSerialization
{
	/// <summary>
	/// WCF Web HTTP (REST) query service. 
	/// Derive your ServiceContract from this.
	/// </summary>
	[ServiceContract]
    public interface IQueryService
    {

		[OperationContract]
		[WebInvoke(Method = "POST",
			UriTemplate = "/execute",
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml,
			BodyStyle = WebMessageBodyStyle.Bare)]
		object[] ExecuteQuery(System.Xml.Linq.XElement xml);
    }
   
}
