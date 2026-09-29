using System;
using System.ComponentModel.DataAnnotations;

namespace Eshava.Test.Core.Validation.Models
{
	public class RangeLimitData
	{
		[Range(1, Int64.MaxValue)]
		public long Alpha { get; set; }

		[Range(1, Int64.MaxValue)]
		public long? AlphaNullable { get; set; }

		[Range(Int64.MinValue, -1)]
		public long Beta { get; set; }

		[Range(typeof(long), "1", "9223372036854775807")]
		public long Gamma { get; set; }

		[Range(1, Int64.MaxValue)]
		public int Delta { get; set; }

		[Range(Int64.MinValue, -1)]
		public int Epsilon { get; set; }
	}
}