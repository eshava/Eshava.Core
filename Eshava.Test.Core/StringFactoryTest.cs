using System;
using System.Linq;
using Eshava.Core;
using Eshava.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core
{
	[TestClass, TestCategory("Core")]
	public class StringFactoryTest
	{
		[TestMethod]
		public void GenerateRandomStringTest()
		{
			// Arrange
			var stringLength = 10;

			// Act
			var result = StringFactory.GenerateRandomString(stringLength);

			// Assert
			result.Should().HaveLength(stringLength);
		}

		[TestMethod]
		public void GenerateRandomStringOnlyNumbersTest()
		{
			// Arrange
			var numbers = (Min: 48, Max: 57);
			var stringLength = 10;
			var options = new RandomStringOptions
			{
				IncludeNumbers = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(stringLength, options);

			// Assert
			result.Should().HaveLength(stringLength);
			result.All(c => Convert.ToByte(c) >= numbers.Min && Convert.ToByte(c) <= numbers.Max).Should().BeTrue();
		}

		[TestMethod]
		public void GenerateRandomStringOnlyLowerCaseTest()
		{
			// Arrange
			var characters = (Min: 97, Max: 122);
			var stringLength = 10;
			var options = new RandomStringOptions
			{
				IncludeLowercase = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(stringLength, options);

			// Assert
			result.Should().HaveLength(stringLength);
			result.All(c => Convert.ToByte(c) >= characters.Min && Convert.ToByte(c) <= characters.Max).Should().BeTrue();
		}

		[TestMethod]
		public void GenerateRandomStringOnlyUpperCaseTest()
		{
			// Arrange
			var characters = (Min: 65, Max: 90);
			var stringLength = 10;
			var options = new RandomStringOptions
			{
				IncludeUppercase = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(stringLength, options);

			// Assert
			result.Should().HaveLength(stringLength);
			result.All(c => Convert.ToByte(c) >= characters.Min && Convert.ToByte(c) <= characters.Max).Should().BeTrue();
		}

		[TestMethod]
		public void GenerateRandomStringOnlySpecialCharactersTest()
		{
			// Arrange
			var charactersOne = (Min: 33, Max: 47);
			var charactersTwo = (Min: 58, Max: 64);
			var charactersThree = (Min: 91, Max: 96);
			var charactersFour = (Min: 123, Max: 126);
			var stringLength = 10;
			var options = new RandomStringOptions
			{
				IncludeSpecialCharacter = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(stringLength, options);

			// Assert
			result.Should().HaveLength(stringLength);
			result.All(c =>
				Convert.ToByte(c) >= charactersOne.Min && Convert.ToByte(c) <= charactersOne.Max
				|| Convert.ToByte(c) >= charactersTwo.Min && Convert.ToByte(c) <= charactersTwo.Max
				|| Convert.ToByte(c) >= charactersThree.Min && Convert.ToByte(c) <= charactersThree.Max
				|| Convert.ToByte(c) >= charactersFour.Min && Convert.ToByte(c) <= charactersFour.Max
			).Should().BeTrue();
		}

		[TestMethod]
		public void GenerateRandomStringProducesEveryAlphanumericCharacterTest()
		{
			// Arrange
			var expected = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

			// Act
			var result = StringFactory.GenerateRandomString(20000);

			// Assert
			result.Distinct().OrderBy(c => c).Should().Equal(expected);
		}

		[TestMethod]
		public void GenerateRandomStringOnlyNumbersProducesEveryDigitTest()
		{
			// Arrange
			var options = new RandomStringOptions
			{
				IncludeNumbers = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(2000, options);

			// Assert
			result.Distinct().OrderBy(c => c).Should().Equal("0123456789");
		}

		[TestMethod]
		public void GenerateRandomStringAllCharacterTypesProduceEveryPrintableCharacterTest()
		{
			// Arrange
			var expected = Enumerable.Range(33, 94).Select(sign => (char)sign);
			var options = new RandomStringOptions
			{
				IncludeNumbers = true,
				IncludeUppercase = true,
				IncludeLowercase = true,
				IncludeSpecialCharacter = true
			};

			// Act
			var result = StringFactory.GenerateRandomString(30000, options);

			// Assert
			result.Distinct().OrderBy(c => c).Should().Equal(expected);
		}

		[TestMethod]
		public void GenerateRandomStringReturnsDifferentValuesTest()
		{
			// Act
			var first = StringFactory.GenerateRandomString(50);
			var second = StringFactory.GenerateRandomString(50);

			// Assert
			first.Should().NotBe(second);
		}

		[TestMethod]
		public void GenerateRandomStringNoCharactersEnabledTest()
		{
			Assert.Throws<NotSupportedException>(() =>
			{
				// Arrange
				var stringLength = 10;
				var options = new RandomStringOptions();

				// Act
				StringFactory.GenerateRandomString(stringLength, options);
			});
		}

		[TestMethod]
		public void GenerateRandomStringZeroStringLenghtTest()
		{
			Assert.Throws<NotSupportedException>(() =>
			{
				// Act
				StringFactory.GenerateRandomString(0);
			});
		}

		[TestMethod]
		public void GenerateRandomStringNegativeStringLenghtTest()
		{
			Assert.Throws<NotSupportedException>(() =>
			{
				// Act
				StringFactory.GenerateRandomString(-10);
			});			
		}
	}
}