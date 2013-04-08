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

namespace ExpressionSerialization
{

	/// <summary>
	/// this can be a base class for the evental client that implements IWebHttpService
	/// </summary>		
	public static class WebHttpRequestClient
	{
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
			dynamic serializer;//DataContractJsonSerializer or DataContractSerializer
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
				Type elementType = instance.GetType();
				if (knownTypes == null)
				{					
					knownTypes = new Type[] { elementType };
				}				
				switch (requestFormat)
				{
					case WebMessageFormat.Json:
						request.ContentType = "application/json";
						serializer = new DataContractJsonSerializer(elementType, knownTypes);
						break;
					case WebMessageFormat.Xml:
						request.ContentType = "application/xml";
						serializer = new DataContractSerializer(elementType, knownTypes);
						break;
					default:
						request.ContentType = "application/xml";
						serializer = new DataContractSerializer(elementType, knownTypes);
						break;
				}
				AsyncCallback requestCallback = (ar1) =>
				{
					postStream = request.EndGetRequestStream(ar1);
					serializer.WriteObject(postStream, instance);
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
		public static object SynchronousCall<TChannel,TResult>(Uri baseAddress, 
			Expression<Func<TChannel, TResult>> methodcall,
		    Type returnType = null) //user's desired return Type, may not be what's declared on OperationContract (e.g. if return Type is object[])
			//,params object[] parameters)
		    {				
				object result = null;
				Stream stream = null;
				MethodCallExpression m;
				

				m = (System.Linq.Expressions.MethodCallExpression)methodcall.Body;
				if (returnType == null)
					returnType = m.Method.ReturnType;
				//Type channelType = m.Object.Type;//for debugging only
				IEnumerable<Type> knownTypes = Resolver.GetBaseTypes(returnType);
				dynamic parameters = getArguments(m);

				string method;
				WebInvokeAttribute webinvoke;
				OperationContractAttribute operationcontract;
				WebMessageFormat requestformat, responseformat;
				Uri endpoint = GetOperationInfo(m.Method, baseAddress, out method, out webinvoke, out operationcontract, out requestformat, out responseformat);
				
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
					requestFormat: requestformat, responseFormat: responseformat,
					knownTypes : knownTypes);
				reset.WaitOne();
				return result;		
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
			out WebInvokeAttribute webinvoke,
			out OperationContractAttribute operationcontract,
			out WebMessageFormat requestformat,
			out WebMessageFormat responseformat)
		{
			object[] customAttributes = operation.GetCustomAttributes(false);
			webinvoke = customAttributes.Single(a => a is WebInvokeAttribute) as WebInvokeAttribute;
			method = webinvoke.Method;
			operationcontract = customAttributes.Single(a => a is OperationContractAttribute) as OperationContractAttribute;
			requestformat = webinvoke.RequestFormat;
			responseformat = webinvoke.ResponseFormat;
			Uri relative = new Uri(webinvoke.UriTemplate, UriKind.Relative);
			Uri endpoint = new Uri(baseAddress, relative);
			return endpoint;
		}



		public static object Deserialize(Type type, Stream stream, WebMessageFormat responseformat = WebMessageFormat.Json)
		{
			dynamic serializer;
			IEnumerable<Type> knownTypes = new Type[] { type,  };
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

	
	}

}
