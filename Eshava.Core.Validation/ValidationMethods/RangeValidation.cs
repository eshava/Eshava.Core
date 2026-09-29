using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Enums;
using Eshava.Core.Validation.Models;

namespace Eshava.Core.Validation.ValidationMethods
{
	internal static class RangeValidation
	{
		private static List<(Func<Type, bool> Check, Func<ValidationCheckParameters, RangeAttribute, ValidationCheckResult> Validate)> _validationRules =
			new List<(Func<Type, bool> Check, Func<ValidationCheckParameters, RangeAttribute, ValidationCheckResult> Validate)>
			{
				(type => type == typeof(float), (parameters, range) => CheckRangeFloat(parameters, range)),
				(type => type == typeof(double), (parameters, range) => CheckRangeDouble(parameters, range)),
				(type => type == typeof(decimal), (parameters, range) => CheckRangeDecimal(parameters, range)),
				(type => type == typeof(int) || type.IsEnum, (parameters, range) => CheckRangeInteger(parameters, range)),
				(type => type == typeof(long), (parameters, range) => CheckRangeLong(parameters, range))
			};

		public static ValidationCheckResult CheckRange(ValidationCheckParameters parameters)
		{
			var dataType = parameters.PropertyInfo.GetDataType();
			var range = Attribute.GetCustomAttribute(parameters.PropertyInfo, typeof(RangeAttribute)) as RangeAttribute;

			if (parameters.PropertyValue == null || range == null)
			{
				return new ValidationCheckResult();
			}

			var validationRule = _validationRules.SingleOrDefault(rule => rule.Check(dataType));
			if (validationRule.Validate != null)
			{
				return validationRule.Validate(parameters, range);
			}

			return GetErrorResult(ValidationErrorType.DataTypeNotSupported, parameters.PropertyInfo.Name, null);
		}

		private static ValidationCheckResult CheckRangeInteger(ValidationCheckParameters parameters, RangeAttribute range)
		{
			var maximum = ToInt32Clamped(range.Maximum);
			var minimum = ToInt32Clamped(range.Minimum);
			var valueInteger = (int)parameters.PropertyValue;

			if (valueInteger < minimum || valueInteger > maximum)
			{
				return GetErrorResult(ValidationErrorType.DataTypeInteger, parameters.PropertyInfo.Name, valueInteger.ToString(CultureInfo.InvariantCulture));
			}

			return new ValidationCheckResult();
		}

		private static ValidationCheckResult CheckRangeLong(ValidationCheckParameters parameters, RangeAttribute range)
		{
			var maximum = ToInt64Clamped(range.Maximum);
			var minimum = ToInt64Clamped(range.Minimum);
			var valueLong = (long)parameters.PropertyValue;

			if (valueLong < minimum || valueLong > maximum)
			{
				return GetErrorResult(ValidationErrorType.DataTypeLong, parameters.PropertyInfo.Name, valueLong.ToString(CultureInfo.InvariantCulture));
			}

			return new ValidationCheckResult();
		}

		private static ValidationCheckResult CheckRangeFloat(ValidationCheckParameters parameters, RangeAttribute range)
		{
			var maximum = Convert.ToDouble(range.Maximum, CultureInfo.InvariantCulture);
			var minimum = Convert.ToDouble(range.Minimum, CultureInfo.InvariantCulture);
			var valueFloat = (float)parameters.PropertyValue;

			if (valueFloat < minimum || valueFloat > maximum)
			{
				return GetErrorResult(ValidationErrorType.DataTypeFloat, parameters.PropertyInfo.Name, valueFloat.ToString(CultureInfo.InvariantCulture));
			}

			return new ValidationCheckResult();
		}

		private static ValidationCheckResult CheckRangeDouble(ValidationCheckParameters parameters, RangeAttribute range)
		{
			var maximum = Convert.ToDouble(range.Maximum, CultureInfo.InvariantCulture);
			var minimum = Convert.ToDouble(range.Minimum, CultureInfo.InvariantCulture);
			var valueDouble = (double)parameters.PropertyValue;

			if (valueDouble < minimum || valueDouble > maximum)
			{
				return GetErrorResult(ValidationErrorType.DataTypeDouble, parameters.PropertyInfo.Name, valueDouble.ToString(CultureInfo.InvariantCulture));
			}

			return new ValidationCheckResult();
		}

		private static ValidationCheckResult CheckRangeDecimal(ValidationCheckParameters parameters, RangeAttribute range)
		{
			var maximum = Convert.ToDecimal(range.Maximum, CultureInfo.InvariantCulture);
			var minimum = Convert.ToDecimal(range.Minimum, CultureInfo.InvariantCulture);
			var valueDecimal = (decimal)parameters.PropertyValue;

			if (valueDecimal < minimum || valueDecimal > maximum)
			{
				return GetErrorResult(ValidationErrorType.DataTypeDecimal, parameters.PropertyInfo.Name, valueDecimal.ToString(CultureInfo.InvariantCulture));
			}

			return new ValidationCheckResult();
		}

		/// <summary>
		/// Converts a range bound to <see cref="int"/>, clamping values beyond the limits of the type.
		/// </summary>
		private static int ToInt32Clamped(object bound)
		{
			var boundLong = ToInt64Clamped(bound);

			if (boundLong >= Int32.MaxValue)
			{
				return Int32.MaxValue;
			}

			if (boundLong <= Int32.MinValue)
			{
				return Int32.MinValue;
			}

			return (int)boundLong;
		}

		/// <summary>
		/// Converts a range bound to <see cref="long"/>, clamping values beyond the limits of the type.
		/// </summary>
		/// <remarks>
		/// <see cref="RangeAttribute(double, double)"/> stores its bounds as double, and <see cref="Int64.MaxValue"/>
		/// rounds up to 2^63 on the way, which no longer fits into a long.
		/// </remarks>
		private static long ToInt64Clamped(object bound)
		{
			if (bound is double || bound is float)
			{
				var boundDouble = Convert.ToDouble(bound, CultureInfo.InvariantCulture);

				if (boundDouble >= Int64.MaxValue)
				{
					return Int64.MaxValue;
				}

				if (boundDouble <= Int64.MinValue)
				{
					return Int64.MinValue;
				}
			}

			return Convert.ToInt64(bound);
		}

		private static ValidationCheckResult GetErrorResult(ValidationErrorType errorType, string propertyName, string @value)
		{
			return new ValidationCheckResult
			{
				ValidationErrors = new List<ValidationError>
				{
					new ValidationError
					{
						MethodType = ValidationMethodType.RangeHardCoded.ToString(),
						ErrorType = errorType.ToString(),
						PropertyName = propertyName,
						Value = @value
					}
				}
			};
		}
	}
}