using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.Threading;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Reflection;
using Resolver = ExpressionSerialization.TypeResolver;
using System.ServiceModel.Description;

namespace ExpressionSerialization
{

	/// <summary>
	/// this can be a base class for the evental client that implements IWebHttpService
	/// </summary>		
	public static class WebHttpRequestClient
	{

		public static TResult SynchronousCall<TChannel, TResult>(Uri baseAddress,
			Expression<Func<TChannel, TResult>> methodcall)
		{
			//return (TExpected)SynchronousCall<TChannel, TActual>(baseAddress, methodcall, typeof(TExpected), parameters);
			return (TResult)SynchronousCall(baseAddress, methodcall, null);
		}
		/// <summary>		
		/// This call is handled as synchronous, BUT it MUST be invoked on a thread separate from the main UI thread.
		/// Assumes that the OperationContract's parameters is exactly 1 params object[]. This is because there is no 
		/// apparent way to send multiple different parameters in HTTP POST in Silverlight. Tuple cannot be serialized
		/// in Silverlight.
		/// 
		/// Only handles HTTP POST requests (WebInvoke), not HTTP GET (WebGet). This is because WebGet only appears to accept
		/// string parameters. 
		/// 
		/// If I later come across an alternative way to pass multiple non-string parameters to a WCF service method, 
		/// </summary>
		/// <typeparam name="TChannel"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="baseAddress"></param>
		/// <param name="methodcall"></param>
		/// <param name="returnType">user's desired return element Type, as either a single instance or in a collection. May not be what's declared on OperationContract (e.g. if return Type is object[])</param>
		/// <param name="parameters">currently required, but redundant since it's possible to get this information from the MethodCallExpression.Arguments</param>
		/// <returns></returns>
		public static object SynchronousCall<TChannel, TResult>(Uri baseAddress,
			Expression<Func<TChannel, TResult>> methodcall,
			Type returnType = null) //user's desired return Type, may not be what's declared on OperationContract (e.g. if return Type is object[])
		//,params object[] parameters)
		{
			object result = null;
			Stream stream = null;
			MethodCallExpression m = (System.Linq.Expressions.MethodCallExpression)methodcall.Body;
			Type channelType = m.Object.Type;//for debugging only			
			IEnumerable<Type> knownTypes = GetServiceKnownTypes(channelType);			
			if (returnType == null)
				returnType = m.Method.ReturnType;
			else
			{
				IEnumerable<Type> baseTypes = Resolver.GetBaseTypes(returnType);
				knownTypes = knownTypes.Union(baseTypes);
			}
			
			dynamic parameters = getArguments(m);

			string method;			
			IOperationBehavior webattribute;
			OperationContractAttribute operationcontract;
			WebMessageFormat requestformat, responseformat;
			Uri endpoint = GetOperationInfo(m.Method, baseAddress, out method, out webattribute, out operationcontract, out requestformat, out responseformat);

			ManualResetEvent reset = new ManualResetEvent(false);
			Action<Stream> completedHandler = (s) =>
			{
				stream = s;
				if (returnType.IsArray || returnType.GetInterface("IEnumerable`1", false) != null)//returnType == typeof(object[]))//)
					result = Deserialize(returnType, stream, responseformat);
				else
					result = Deserialize(returnType, stream, responseformat);
				reset.Set();
			};
			CreateHttpWebRequest(endpoint, instance: parameters,
			callback: completedHandler, 
			method: method,
				requestFormat: requestformat, 
				responseFormat: responseformat,
			knownTypes: knownTypes);
			reset.WaitOne();
			return result;
		}
	
		public static void CreateHttpWebRequest(Uri absoluteUri,			
			object instance,
			Action<Stream> callback,
			string method = "POST",
			WebMessageFormat requestFormat = WebMessageFormat.Xml,
			WebMessageFormat responseFormat = WebMessageFormat.Xml,
			IEnumerable<Type> knownTypes = null
			)
		{
			Stream postStream;			
			HttpWebRequest request;
			HttpWebResponse response;
#if SILVERLIGHT
			request = WebRequest.CreateHttp(absoluteUri);
#else
			request = WebRequest.Create(absoluteUri) as HttpWebRequest;
#endif
			request.Method = method;

			AsyncCallback responseCallback = (ar2) =>
			{
				HttpWebRequest request2 = (HttpWebRequest)ar2.AsyncState;
				response = (HttpWebResponse)request2.EndGetResponse(ar2);
				Stream stream = response.GetResponseStream();
				callback(stream);				
				//stream.Position = 0;//NotSupportedException: Specified method is not supported.
			};

			if (method == "POST" && instance != null)
			{
				request.ContentType = requestFormat == WebMessageFormat.Json ? "application/json" : "application/xml";				
				AsyncCallback requestCallback = (ar1) =>
				{
					postStream = request.EndGetRequestStream(ar1);
					Serialize(postStream, instance, requestFormat, knownTypes: knownTypes);
					postStream.Close();
					request.BeginGetResponse(responseCallback, request);//GetResponse
				};
				request.BeginGetRequestStream(requestCallback, request);
			}
			else
			{
				request.ContentLength = 0;
				request.BeginGetResponse(responseCallback, request);
			}
				
		}

		static long Serialize(Stream stream, object instance, WebMessageFormat requestFormat, IEnumerable<Type> knownTypes = null)
		{
			dynamic serializer;//DataContractJsonSerializer or DataContractSerializer
			Type elementType = instance.GetType();
			if (knownTypes == null)
			{
				knownTypes = new Type[] { elementType };
			}		
			switch (requestFormat)
			{
				case WebMessageFormat.Json:
					serializer = new DataContractJsonSerializer(elementType, knownTypes);
					break;
				case WebMessageFormat.Xml:
					serializer = new DataContractSerializer(elementType, knownTypes);
					break;
				default:
					serializer = new DataContractSerializer(elementType, knownTypes);
					break;
			}
			serializer.WriteObject(stream, instance);
			return 0;//return stream.Length;
		}
		public static object Deserialize(Type type, Stream stream, WebMessageFormat responseformat = WebMessageFormat.Json)
		{
			dynamic serializer;
			IEnumerable<Type> knownTypes = new Type[] { type, };
			switch (responseformat)
			{
				case WebMessageFormat.Json:
					serializer = new DataContractJsonSerializer(type, knownTypes);
					break;
				case WebMessageFormat.Xml:
					serializer = new DataContractSerializer(type, knownTypes);
					break;
				default:
					serializer = new DataContractJsonSerializer(type, knownTypes);
					break;
			}
			return serializer.ReadObject(stream);
		}
		public static T Deserialize<T>(Stream stream, WebMessageFormat responseformat = WebMessageFormat.Json)
		{
			return (T)Deserialize(typeof(T), stream, responseformat);
		}

		/// <summary>
		/// assumes a single params object[] argument.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		static dynamic getArguments(MethodCallExpression m)
		{
			if (m.Arguments.Count == 0)
				return null;
			Expression argexp = m.Arguments[0];
			var evald = (ConstantExpression)Evaluator.PartialEval(argexp);
			LambdaExpression lambda = Expression.Lambda(evald);
			dynamic args = lambda.Compile().DynamicInvoke(new object[0]);
			return args;
		}




		static Uri GetOperationInfo(MethodInfo operation,
			Uri baseAddress,
			out string method,
			out IOperationBehavior webbehavior,//out WebInvokeAttribute webinvoke,
			out OperationContractAttribute operationcontract,
			out WebMessageFormat requestformat,
			out WebMessageFormat responseformat)
		{
			
			object[] customAttributes = operation.GetCustomAttributes(false);
			webbehavior = customAttributes.Single(a => a is WebInvokeAttribute || a is WebGetAttribute) as IOperationBehavior;			
			operationcontract = customAttributes.Single(a => a is OperationContractAttribute) as OperationContractAttribute;
			if (webbehavior is WebInvokeAttribute)
			{
				requestformat = ((WebInvokeAttribute)webbehavior).RequestFormat;
				responseformat = ((WebInvokeAttribute)webbehavior).ResponseFormat;
				Uri relative = new Uri(((WebInvokeAttribute)webbehavior).UriTemplate, UriKind.Relative);
				Uri endpoint = new Uri(baseAddress, relative);
				method = ((WebInvokeAttribute)webbehavior).Method;
				return endpoint;
			}
			else if (webbehavior is WebGetAttribute)
			{
				requestformat = ((WebGetAttribute)webbehavior).RequestFormat;
				responseformat = ((WebGetAttribute)webbehavior).ResponseFormat;
				Uri relative = new Uri(((WebGetAttribute)webbehavior).UriTemplate, UriKind.Relative);
				Uri endpoint = new Uri(baseAddress, relative);
				method = "GET";
				return endpoint;
			}
			else
				throw new NotSupportedException(webbehavior.GetType().FullName + " is not supported.");
		}

		static IEnumerable<Type> GetServiceKnownTypes(Type service)
		{
			HashSet<Type> knownTypes = new HashSet<Type>();			
			object[] customattributes = service.GetCustomAttributes(true);
			IEnumerable<ServiceKnownTypeAttribute> kattrs = customattributes.OfType<ServiceKnownTypeAttribute>();
			foreach (var k in kattrs)
				knownTypes.Add(k.Type);

			MethodInfo[] methods = service.GetMethods();
			foreach (var m in methods)
			{
				foreach (var k in m.GetCustomAttributes(true).OfType<ServiceKnownTypeAttribute>())
					knownTypes.Add(k.Type);
			}
			return knownTypes;
		}
		

	


	
	
	}

}
