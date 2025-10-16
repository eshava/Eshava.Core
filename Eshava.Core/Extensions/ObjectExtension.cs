﻿using System.Collections;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Core.Extensions
{
	public static class ObjectExtension
	{
		public static Task<T> ToTask<T>(this T data)
		{
			return Task.FromResult(data);
		}

		public static ResponseData<T> ToResponseData<T>(this T data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return new ResponseData<T>(data, statusCode);
		}

		public static Task<ResponseData<T>> ToResponseDataAsync<T>(this T data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return Task.FromResult(data.ToResponseData(statusCode));
		}

		public static object ReplaceEmptyStringsToNull(this object model)
		{
			if (model == null)
			{
				return null;
			}

			//Go through all properties and check them automatically
			foreach (var propertyInfo in model.GetType().GetProperties())
			{
				if (!propertyInfo.CanRead)
				{
					continue;
				}

				if (propertyInfo.PropertyType == typeof(string))
				{
					if (propertyInfo.CanWrite)
					{
						var valueString = propertyInfo.GetValue(model) as string;
						propertyInfo.SetValue(model, valueString.ReturnNullByEmpty());
					}
				}
				else if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.ImplementsIEnumerable())
				{
					var subModel = propertyInfo.GetValue(model);
					model.ReplaceEmptyStringsToNull(subModel, propertyInfo);
				}
			}

			return model;
		}

		private static void ReplaceEmptyStringsToNull(this object model, object propertyValue, PropertyInfo propertyInfo)
		{
			if (propertyValue == null)
			{
				return;
			}

			if (propertyInfo.PropertyType.ImplementsIEnumerable())
			{
				var underlyingType = propertyInfo.PropertyType.GetDataTypeFromIEnumerable();

				if (!underlyingType.IsClass || !(propertyValue is IEnumerable elements))
				{
					return;
				}
				
				if (underlyingType == typeof(string))
				{
					if (propertyInfo.CanWrite)
					{
						var newValue = elements.Cast<string>().Where(value => value.ReturnNullByEmpty() != null).ToList();
						propertyInfo.SetValue(model, newValue);
					}
				}
				else
				{
					foreach (var element in elements)
					{
						ReplaceEmptyStringsToNull(element);
					}
				}
			}
			else
			{
				ReplaceEmptyStringsToNull(propertyValue);
			}
		}
	}
}