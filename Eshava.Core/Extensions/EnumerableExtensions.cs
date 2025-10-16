using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Core.Extensions
{
	public static class EnumerableExtensions
	{
		public static ResponseData<IEnumerable<T>> ToIEnumerableResponseData<T>(this IEnumerable<T> data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return new ResponseData<IEnumerable<T>>(data, statusCode);
		}

		public static Task<ResponseData<IEnumerable<T>>> ToIEnumerableResponseDataAsync<T>(this IEnumerable<T> data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return Task.FromResult(data.ToIEnumerableResponseData(statusCode));
		}

		public static ResponseData<IList<T>> ToIListResponseData<T>(this IList<T> data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return new ResponseData<IList<T>>(data,statusCode);
		}

		public static Task<ResponseData<IList<T>>> ToIListResponseDataAsync<T>(this IList<T> data, HttpStatusCode statusCode = HttpStatusCode.OK)
		{
			return Task.FromResult(data.ToIListResponseData(statusCode));
		}
	}
}