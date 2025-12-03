using System;
using System.Reflection;

namespace Eshava.Core.Validation.Extension
{
	public static class MethodInfoExtension
	{
		public static bool HasAttribute<T>(this MethodInfo methodInfo) where T : Attribute
		{
			return Attribute.GetCustomAttribute(methodInfo, typeof(T)) != null;
		}
	}
}