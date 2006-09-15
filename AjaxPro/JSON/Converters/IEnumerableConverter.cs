/*
 * MS	06-04-25	removed unnecessarily used cast
 * MS	06-05-23	using local variables instead of "new Type()" for get De-/SerializableTypes
 * 
 */
using System;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace AjaxPro
{
	/// <summary>
	/// Provides methods to serialize and deserialize an object that implements IEnumerable.
	/// </summary>
	public class IEnumerableConverter : IJavaScriptConverter
	{
		public IEnumerableConverter() : base()
		{
			m_AllowInheritance = true;

			m_serializableTypes = new Type[] { typeof(IEnumerable) };
		}

		public override string Serialize(object o)
		{
			IEnumerable enumerable = o as IEnumerable;

			if(enumerable == null)
				throw new NotSupportedException();

			StringBuilder sb = new StringBuilder();
			
			bool b = true;

			sb.Append("[");
				
			foreach(object obj in enumerable)
			{
				if(b){ b = false; }
				else{ sb.Append(","); }

				sb.Append(JavaScriptSerializer.Serialize(obj));
			}

			sb.Append("]");

			return sb.ToString();
		}

		public override bool TrySerializeValue(object o, Type t, out string json)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
			{
				json = null;
				return false;
			}

			return base.TrySerializeValue(o, t, out json);
		}

		public override bool TryDeserializeValue(IJavaScriptObject jso, Type t, out object o)
		{
			if (typeof(IDictionary).IsAssignableFrom(t))
			{
				o = null;
				return false;
			}

			return base.TryDeserializeValue(jso, t, out o);
		}
	}
}