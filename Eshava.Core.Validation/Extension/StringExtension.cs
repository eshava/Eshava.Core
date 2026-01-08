using System;
using System.Text.RegularExpressions;
using Eshava.Core.Extensions;

namespace Eshava.Core.Validation.Extension
{
	public static class StringExtension
	{
		public static bool IsEmailAddress(this string value)
		{
			if (value.IsNullOrEmpty())
			{
				return false;
			}

			var regex = new Regex(@"^(([^<>()[\]\\.,;:\s@"" ]+(\.[^<>()[\]\\.,;:\s@"" ]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
			var match = regex.Match(value);

			return match.Success;
		}

		public static bool IsUrl(this string value)
		{
			if (value.IsNullOrEmpty() || value.Contains("@"))
			{
				return false;
			}

			if (!value.StartsWith("http://") && !value.StartsWith("https://"))
			{
				value = "http://" + value;
			}

			if (!Uri.TryCreate(value, UriKind.Absolute, out var outputUri) || value.Length > 2048)
			{
				return false;
			}

			if (outputUri.HostNameType == UriHostNameType.Dns && outputUri.Host.ToLower() == "localhost")
			{
				return true;
			}

			if (outputUri.HostNameType == UriHostNameType.IPv4 || outputUri.HostNameType == UriHostNameType.IPv6)
			{
				return true;
			}

			return outputUri.Host.Replace("www.", "").Split('.').Length > 1
				&& outputUri.Host.Length > outputUri.Host.LastIndexOf(".", StringComparison.Ordinal) + 1;
		}

		public static string FormatToJsonPropertyName(this string propertyName)
		{
			return propertyName.ToLower()[0] + propertyName.Substring(1);
		}
	}
}