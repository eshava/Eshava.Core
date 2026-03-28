using System.ComponentModel.DataAnnotations;
using Eshava.Core.Validation.Attributes;
using Newtonsoft.Json;

namespace Eshava.Test.Core.Validation.Models
{
	internal class BasicRules
	{
		[Required]
		public string CamelCaseName { get; set; }

		[Required]
		public string CAPITALLetters { get; set; }

		[SpecialValidation]
		[JsonProperty("itsNotJustAnotherProperty")]
		public string JustAnotherProperty { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("itsNotTheNextProperty")]
		public string NextProperty { get; set; }

		[ValidationIgnore]
		public string IgnoreMe { get; set; }
	}
}