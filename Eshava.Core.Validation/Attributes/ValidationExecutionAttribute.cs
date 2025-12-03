using System;

namespace Eshava.Core.Validation.Attributes
{
	/// <summary>
	/// The method must have the following format
	/// IEnumerable<Eshava.Core.Models.ValidationError> SomeMethodName()
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class ValidationExecutionAttribute : Attribute
	{

	}
}