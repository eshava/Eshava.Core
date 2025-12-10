using System;
using System.Collections;
using Eshava.Core.Linq.Models;

namespace Eshava.Core.Linq.Constants
{
	internal static class TypeConstants
	{
		public static readonly Type FilterField = typeof(FilterField);
		public static readonly Type NestedFilter = typeof(NestedFilter);
		public static readonly Type NestedSort = typeof(NestedSort);
		public static readonly Type Bool = typeof(bool);
		public static readonly Type DateTime = typeof(DateTime);
		public static readonly Type Decimal = typeof(decimal);
		public static readonly Type Double = typeof(double);
		public static readonly Type Enum = typeof(Enum);
		public static readonly Type Float = typeof(float);
		public static readonly Type Guid = typeof(Guid);
		public static readonly Type IList = typeof(IList);
		public static readonly Type Int = typeof(int);
		public static readonly Type Long = typeof(long);
		public static readonly Type Object = typeof(object);
		public static readonly Type SortField = typeof(SortField);
		public static readonly Type String = typeof(string);

		public static readonly Type NullableBool = typeof(bool?);
		public static readonly Type NullableDateTime = typeof(DateTime?);
		public static readonly Type NullableDecimal = typeof(decimal?);
		public static readonly Type NullableDouble = typeof(double?);
		public static readonly Type NullableFloat = typeof(float?);
		public static readonly Type NullableGuid = typeof(Guid?);
		public static readonly Type NullableInt = typeof(int?);
		public static readonly Type NullableLong = typeof(long?);
	}
}