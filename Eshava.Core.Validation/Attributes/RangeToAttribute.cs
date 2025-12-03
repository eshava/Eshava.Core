using System;

namespace Eshava.Core.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property)]
	public class RangeToAttribute : AbstractRangeFromOrToAttribute
	{
		public RangeToAttribute(string propertyName, bool allowNull) : base(propertyName, allowNull)
		{

		}
	}
}