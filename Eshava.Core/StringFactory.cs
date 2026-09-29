using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Eshava.Core.Models;

namespace Eshava.Core
{
	public static class StringFactory
	{
		private static readonly (int Min, int Max) _rangeNumbers = (48, 57);
		private static readonly (int Min, int Max) _rangeUppercase = (65, 90);
		private static readonly (int Min, int Max) _rangeLowercase = (97, 122);
		private static readonly (int Min, int Max) _rangeSpecialCharacters1 = (33, 47);
		private static readonly (int Min, int Max) _rangeSpecialCharacters2 = (58, 64);
		private static readonly (int Min, int Max) _rangeSpecialCharacters3 = (91, 96);
		private static readonly (int Min, int Max) _rangeSpecialCharacters4 = (123, 126);

		/// <summary>
		/// Generate a random string of specified length, drawn from a cryptographic random number generator
		/// </summary>
		/// <param name="numberOfSigns">Length of the string</param>
		/// <returns>random string</returns>
		public static string GenerateRandomString(int numberOfSigns)
		{
			return GenerateRandomString(numberOfSigns, new List<(int Min, int Max)> { _rangeNumbers, _rangeUppercase, _rangeLowercase });
		}

		/// <summary>
		/// Generate a random string of specified length, drawn from a cryptographic random number generator
		/// </summary>
		/// <param name="numberOfSigns">Length of the string</param>
		/// <param name="options"></param>
		/// <returns>random string</returns>
		public static string GenerateRandomString(int numberOfSigns, RandomStringOptions options)
		{
			if (options.NoCharacterTypeEnabled)
			{
				throw new NotSupportedException("At least one character type must be enabled");
			}

			var ranges = new List<(int Min, int Max)>();

			if (options.IncludeNumbers)
			{
				ranges.Add(_rangeNumbers);
			}

			if (options.IncludeUppercase)
			{
				ranges.Add(_rangeUppercase);
			}

			if (options.IncludeLowercase)
			{
				ranges.Add(_rangeLowercase);
			}

			if (options.IncludeSpecialCharacter)
			{
				ranges.Add(_rangeSpecialCharacters1);
				ranges.Add(_rangeSpecialCharacters2);
				ranges.Add(_rangeSpecialCharacters3);
				ranges.Add(_rangeSpecialCharacters4);
			}

			return GenerateRandomString(numberOfSigns, ranges);
		}

		/// <summary>
		/// Draws every character from a cryptographic random number generator, so the result is suitable
		/// for passwords, secrets and tokens.
		/// </summary>
		/// <remarks>
		/// Each random byte is mapped onto the alphabet by rejection sampling: bytes at or above the
		/// largest multiple of the alphabet length are discarded, so every character of every enabled
		/// range, including the last one, is equally likely.
		/// </remarks>
		private static string GenerateRandomString(int numberOfSigns, List<(int Min, int Max)> ranges)
		{
			if (numberOfSigns <= 0)
			{
				throw new NotSupportedException(nameof(numberOfSigns) + ": A number greater than zero is expected.");
			}

			var alphabet = ranges
				.SelectMany(range => Enumerable.Range(range.Min, range.Max - range.Min + 1))
				.Select(sign => (char)sign)
				.ToArray();

			var limit = 256 - (256 % alphabet.Length);
			var text = new StringBuilder(numberOfSigns);
			var buffer = new byte[numberOfSigns * 2];

			using (var generator = RandomNumberGenerator.Create())
			{
				while (text.Length < numberOfSigns)
				{
					generator.GetBytes(buffer);

					foreach (var value in buffer)
					{
						if (value >= limit)
						{
							continue;
						}

						text.Append(alphabet[value % alphabet.Length]);

						if (text.Length == numberOfSigns)
						{
							break;
						}
					}
				}
			}

			return text.ToString();
		}
	}
}