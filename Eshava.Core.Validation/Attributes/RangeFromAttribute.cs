using System;

namespace Eshava.Core.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class RangeFromAttribute : AbstractRangeFromOrToAttribute
	{
		public RangeFromAttribute(string propertyName, bool allowNull) : base(propertyName, allowNull)
		{

		}
	}
}