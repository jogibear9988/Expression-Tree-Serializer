using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Json;

namespace ExpressionSerialization
{
	public class JsonDynamicObject : DynamicObject
	{
		private Dictionary<string, object> _properties = new Dictionary<string, object>();

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			object value;
			if (_properties.TryGetValue(binder.Name, out value))
			{
				result = value;
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (_properties.ContainsKey(binder.Name))
			{
				_properties[binder.Name] = value;
			}
			else
			{
				_properties.Add(binder.Name, value);
			}
			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _properties.Keys;
		}

		#region static methods
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">expected: JsonDynamicObject, dynamic, or IEnumerable-of, List-of, Array-of</typeparam>
		/// <param name="json">string of JSON</param>
		/// <returns></returns>
		public static T Parse<T>(string json) where T : class
		{
			//JsonValue has Implicit operator conversion (http://msdn.microsoft.com/en-us/library/system.json.jsonvalue(VS.95).aspx)
			T result = null;
			JsonValue jsvalue = JsonObject.Parse(json);
			if (jsvalue.JsonType == JsonType.Object)
				result = buildDynamicObject((JsonObject)jsvalue) as T;
			else if (jsvalue.JsonType == JsonType.Array)
			{
				List<JsonDynamicObject> list = new List<JsonDynamicObject>();
				JsonArray jsarray = (JsonArray)jsvalue;
				for (int i = 0; i < jsarray.Count; i++)
					list.Add(buildDynamicObject((IDictionary<string, JsonValue>)jsarray[i]));
				result = list as T;
			}	
									
			return result;
		}
		
		private static JsonDynamicObject buildDynamicObject(IDictionary<string, JsonValue> props)
		{
			if (props != null)
			{
				JsonDynamicObject dynObj = new JsonDynamicObject();
				foreach (var kvp in props)
				{
					IDictionary<string, JsonValue> subProps = kvp.Value as IDictionary<string, JsonValue>;
					if (subProps != null)
					{
						dynObj._properties.Add(kvp.Key, buildDynamicObject(subProps));
					}
					else
					{
						dynObj._properties.Add(kvp.Key, kvp.Value);
					}
				}
				return dynObj;
			}
			else
			{
				return null;
			}


		}
		#endregion
	}
}
