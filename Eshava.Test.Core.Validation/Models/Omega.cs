using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Eshava.Core.Models;
using Eshava.Core.Validation.Attributes;

namespace Eshava.Test.Core.Validation.Models
{
	public class Omega
	{
		[Required]
		public string Psi { get; set; }

		[MaxLength(20)]
		public string Chi { get; set; }
		public int? Sigma { get; set; }

		public bool ValidationShouldFail { get; set; }

		[ValidationExecution]
		public IEnumerable<ValidationError> ValidateOne()
		{
			return ValidationShouldFail
				? [new ValidationError { PropertyName = nameof(ValidationShouldFail) }]
				: Array.Empty<ValidationError>();
		}

	}
}