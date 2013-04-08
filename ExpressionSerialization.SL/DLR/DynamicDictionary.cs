using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.IO;
using System.Linq.Expressions;

namespace ExpressionSerialization
{

	class DynamicDictionary : IDynamicMetaObjectProvider
	{
		#region IDynamicMetaObjectProvider Members
		DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
		{
			return new DynamicDictionaryMetaObject(parameter, this);
		}

		private Dictionary<string, object> storage = new
			Dictionary<string, object>();
		public object SetDictionaryEntry(string key, object value)
		{
			if (storage.ContainsKey(key))
				storage[key] = value;
			else
				storage.Add(key, value);
			return value;
		}
		public object GetDictionaryEntry(string key)
		{
			object result = null;
			if (storage.ContainsKey(key))
			{
				result = storage[key];
			}
			return result;
		}
		public object WriteMethodInfo(string methodInfo)
		{
			Console.WriteLine(methodInfo);
			return 42; // because it is the answer to everything
		}
		public override string ToString()
		{
			StringWriter message = new StringWriter();
			foreach (var item in storage)
				message.WriteLine("{0}:\t{1}", item.Key, item.Value);
			return message.ToString();
		}
		#endregion
		private class DynamicDictionaryMetaObject : DynamicMetaObject
		{
			internal DynamicDictionaryMetaObject(Expression parameter, DynamicDictionary value)
				: base(parameter, BindingRestrictions.Empty, value)
			{
			}
			public override DynamicMetaObject BindSetMember(SetMemberBinder binder,
				DynamicMetaObject value)
			{
				// Method to call in the containing class:
				string methodName = "SetDictionaryEntry";
				// setup the binding restrictions.
				BindingRestrictions restrictions =
					BindingRestrictions.GetTypeRestriction(Expression, LimitType);
				// setup the parameters:
				Expression[] args = new Expression[2];
				// First parameter is the name of the property to Set
				args[0] = Expression.Constant(binder.Name);
				// Second parameter is the value
				args[1] = Expression.Convert(value.Expression, typeof(object));
				// Setup the 'this' reference
				Expression self = Expression.Convert(Expression, base.LimitType);
				// Setup the method call expression
				Expression methodCall = Expression.Call(self,
						typeof(DynamicDictionary).GetMethod(methodName),
						args);
				// Create a meta object to invoke Set later:
				DynamicMetaObject setDictionaryEntry = new DynamicMetaObject(
					methodCall,
					restrictions);
				// return that dynamic object
				return setDictionaryEntry;
			}
			public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
			{
				// Method call in the containing class:
				string methodName = "GetDictionaryEntry";
				// One parameter
				Expression[] parameters = new Expression[]
           {
               Expression.Constant(binder.Name)
           };
				DynamicMetaObject getDictionaryEntry = new DynamicMetaObject(
					Expression.Call(
						Expression.Convert(Expression, LimitType),
						typeof(DynamicDictionary).GetMethod(methodName),
						parameters),
					BindingRestrictions.GetTypeRestriction(Expression, LimitType));
				return getDictionaryEntry;
			}
			public override DynamicMetaObject BindInvokeMember(
				InvokeMemberBinder binder, DynamicMetaObject[] args)
			{
				StringBuilder paramInfo = new StringBuilder();
				paramInfo.AppendFormat("Calling {0}(", binder.Name);
				foreach (var item in args)
					paramInfo.AppendFormat("{0}, ", item.Value);
				paramInfo.Append(")");
				Expression[] parameters = new Expression[]
           {
               Expression.Constant(paramInfo.ToString())
           };
				DynamicMetaObject methodInfo = new DynamicMetaObject(
					Expression.Call(
					Expression.Convert(Expression, LimitType),
					typeof(DynamicDictionary).GetMethod("WriteMethodInfo"),
					parameters),
					BindingRestrictions.GetTypeRestriction(Expression, LimitType));
				return methodInfo;
			}
		}


	}
}