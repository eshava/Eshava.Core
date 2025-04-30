﻿using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Eshava.Core.Linq.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CompareOperator
	{
		[EnumMember(Value = nameof(None))]
		None = 0,
		[EnumMember(Value = nameof(Equal))]
		Equal = 1,
		[EnumMember(Value = nameof(NotEqual))]
		NotEqual = 2,
		[EnumMember(Value = nameof(GreaterThan))]
		GreaterThan = 3,
		[EnumMember(Value = nameof(GreaterThanOrEqual))]
		GreaterThanOrEqual = 4,
		[EnumMember(Value = nameof(LessThan))]
		LessThan = 5,
		[EnumMember(Value = nameof(LessThanOrEqual))]
		LessThanOrEqual = 6,
		[EnumMember(Value = nameof(Contains))]
		Contains = 7,
		[EnumMember(Value = nameof(StartsWith))]
		StartsWith = 8,
		[EnumMember(Value = nameof(EndsWith))]
		EndsWith = 9,
		[EnumMember(Value = nameof(ContainsNot))]
		ContainsNot = 10,
		[EnumMember(Value = nameof(ContainedIn))]
		ContainedIn = 11,
		[EnumMember(Value = nameof(IsNull))]
		IsNull = 12,
		[EnumMember(Value = nameof(IsNotNull))]
		IsNotNull = 13,
		[EnumMember(Value = nameof(EqualOrNull))]
		EqualOrNull = 14,
		[EnumMember(Value = nameof(NotEqualOrNull))]
		NotEqualOrNull = 15,
		[EnumMember(Value = nameof(GreaterThanOrNull))]
		GreaterThanOrNull = 16,
		[EnumMember(Value = nameof(GreaterThanOrEqualOrNull))]
		GreaterThanOrEqualOrNull = 17,
		[EnumMember(Value = nameof(LessThanOrNull))]
		LessThanOrNull = 18,
		[EnumMember(Value = nameof(LessThanOrEqualOrNull))]
		LessThanOrEqualOrNull = 19,
	}
}