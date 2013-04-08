using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;
namespace Northwind
{
	/// <summary>
	/// <see cref="http://blogs.msdn.com/b/mattwar/archive/2007/07/31/linq-building-an-iqueryable-provider-part-ii.aspx"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ObjectReader<T> : IEnumerable<T>, System.Collections.IEnumerable where T : class, new()
	{
		Enumerator enumerator;
		internal ObjectReader(DbDataReader reader)
		{
			this.enumerator = new Enumerator(reader);
		}
		public IEnumerator<T> GetEnumerator()
		{
			Enumerator e = this.enumerator;
			if (e == null)
			{
				throw new InvalidOperationException("Cannot enumerate more than once");
			}
			this.enumerator = null;
			return e;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		class Enumerator : IEnumerator<T>, System.Collections.IEnumerator, IDisposable
		{
			DbDataReader _reader;
			FieldInfo[] _fields;
			PropertyInfo[] _properties;
			int[] _Lookup;//or property lookup
			T current;
			internal Enumerator(DbDataReader reader)
			{
				this._reader = reader;
				this._fields = typeof(T).GetFields();
				this._properties = typeof(T).GetProperties();
			}
			public T Current
			{
				get { return this.current; }
			}
			object System.Collections.IEnumerator.Current
			{
				get { return this.current; }
			}
			public bool MoveNext()
			{
				if (this._reader.Read())
				{
					if (this._Lookup == null)
					{
						this.InitLookup();
					}
					T instance = new T();
					for (int i = 0, n = this._properties.Length; i < n; i++)//(int i = 0, n = this._fields.Length; i < n; i++)
					{
						int index = this._Lookup[i];
						if (index >= 0)
						{
							//FieldInfo fi = this._fields[i];
							PropertyInfo pi = this._properties[i];
							if (this._reader.IsDBNull(index))
							{
								pi.SetValue(instance, null, null);//fi.SetValue(instance, null);								
							}
							else
							{
								pi.SetValue(instance, this._reader.GetValue(index), null);//fi.SetValue(instance, this._reader.GetValue(index));								
							}
						}
					}
					this.current = instance;
					return true;
				}
				return false;
			}
			public void Reset()
			{
			}
			public void Dispose()
			{
				this._reader.Dispose();
			}
			private void InitLookup()
			{
				Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
				for (int i = 0, n = this._reader.FieldCount; i < n; i++)
				{
					map.Add(this._reader.GetName(i), i);
				}
				this._Lookup = new int[this._properties.Length]; //new int[this._fields.Length];				
				for (int i = 0, n = this._properties.Length; i < n; i++)//(int i = 0, n = this._fields.Length; i < n; i++)
				{
					int index;
					if (map.TryGetValue(this._properties[i].Name, out index))
					{
						this._Lookup[i] = index;
					}
					//else if (map.TryGetValue(this._fields[i].Name, out index))
					//{
					//    this._fieldLookup[i] = index;
					//}
					else
					{
						this._Lookup[i] = -1;
					}
				}
			}

		}
	}
 
 
}
