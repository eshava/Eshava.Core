using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eshava.Core.Security.Cryptography;
using Eshava.Core.Security.Cryptography.Constants;
using Eshava.Core.Security.Cryptography.Interfaces;
using Eshava.Core.Security.Cryptography.Models;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Security.Cryptography
{
	[TestClass, TestCategory("Core.Security")]
	public class MigratingCryptographyEngineTest
	{
		private const string INPUT = "Darkwing Duck knows QuackFu";
		private const string KEY_ID = "k202610";

		private RijndaelEngine _rijndaelEngine;
		private string _key;

		[TestInitialize]
		public void Setup()
		{
			_rijndaelEngine = new RijndaelEngine(new SymmetricCryptographySettings());
			_rijndaelEngine.GenerateKeys(true);
			_key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		}

		[TestMethod]
		public void TypeIsMigratingTest()
		{
			// Arrange
			var classUnderTest = new MigratingCryptographyEngine(CreateAesGcmEngine(KEY_ID), _rijndaelEngine);

			// Act & Assert
			classUnderTest.Type.Should().Be(SymmetricCryptographyEngineConstants.MIGRATING);
		}

		[TestMethod]
		public void WritesThroughLegacyEngineWithoutActiveKeyIdTest()
		{
			// Arrange
			var classUnderTest = new MigratingCryptographyEngine(CreateAesGcmEngine(null), _rijndaelEngine);

			// Act
			var encrypted = classUnderTest.Encrypt(INPUT);

			// Assert
			AesGcmEngine.IsAesGcmFormat(encrypted).Should().BeFalse();
			_rijndaelEngine.Decrypt(encrypted).Should().Be(INPUT);
			classUnderTest.Decrypt(encrypted).Should().Be(INPUT);
		}

		[TestMethod]
		public void WritesThroughAesGcmWithActiveKeyIdTest()
		{
			// Arrange
			var aesGcmEngine = CreateAesGcmEngine(KEY_ID);
			var classUnderTest = new MigratingCryptographyEngine(aesGcmEngine, _rijndaelEngine);

			// Act
			var encrypted = classUnderTest.Encrypt(INPUT);

			// Assert
			AesGcmEngine.IsAesGcmFormat(encrypted).Should().BeTrue();
			aesGcmEngine.Decrypt(encrypted).Should().Be(INPUT);
			classUnderTest.Decrypt(encrypted).Should().Be(INPUT);
		}

		[TestMethod]
		public void ReadsLegacyValueAndRewritesItTest()
		{
			// Arrange
			var legacyValue = _rijndaelEngine.Encrypt(INPUT);
			var classUnderTest = new MigratingCryptographyEngine(CreateAesGcmEngine(KEY_ID), _rijndaelEngine);

			// Act
			var plaintext = classUnderTest.Decrypt(legacyValue);
			var rewritten = classUnderTest.Encrypt(plaintext);

			// Assert
			plaintext.Should().Be(INPUT);
			AesGcmEngine.IsAesGcmFormat(rewritten).Should().BeTrue();
			classUnderTest.Decrypt(rewritten).Should().Be(INPUT);
		}

		[TestMethod]
		public void TamperedAesGcmValueDoesNotFallBackToLegacyEngineTest()
		{
			// Arrange
			var legacyEngine = A.Fake<ISymmetricCryptographyEngine>();
			A.CallTo(() => legacyEngine.Type).Returns(SymmetricCryptographyEngineConstants.RIJNDAEL);
			var classUnderTest = new MigratingCryptographyEngine(CreateAesGcmEngine(KEY_ID), legacyEngine);
			var encrypted = classUnderTest.Encrypt(INPUT);
			encrypted[^1] ^= 0x01;

			// Act
			var act = () => classUnderTest.Decrypt(encrypted);

			// Assert
			act.Should().Throw<CryptographicException>();
			A.CallTo(() => legacyEngine.Decrypt(A<byte[]>._)).MustNotHaveHappened();
		}

		[TestMethod]
		public void GenerateKeysDelegatesToAesGcmTest()
		{
			// Arrange
			var classUnderTest = new MigratingCryptographyEngine(CreateAesGcmEngine(KEY_ID), _rijndaelEngine);

			// Act
			(var iv, var key) = classUnderTest.GenerateKeys(true);

			// Assert
			iv.Should().BeEmpty();
			key.Length.Should().Be(32);
		}

		[TestMethod]
		public void ConstructWithSwappedEnginesThrowsTest()
		{
			// Act
			var act = () => new MigratingCryptographyEngine(_rijndaelEngine, CreateAesGcmEngine(KEY_ID));

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void ConstructWithForeignWriterThrowsTest()
		{
			// Arrange
			var writer = A.Fake<ISymmetricCryptographyEngine>();
			A.CallTo(() => writer.Type).Returns(SymmetricCryptographyEngineConstants.AESGCM);

			// Act
			var act = () => new MigratingCryptographyEngine(writer, _rijndaelEngine);

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void ConstructWithoutEnginesThrowsTest()
		{
			// Act
			var withoutWriter = () => new MigratingCryptographyEngine(null, _rijndaelEngine);
			var withoutLegacy = () => new MigratingCryptographyEngine(CreateAesGcmEngine(KEY_ID), null);

			// Assert
			withoutWriter.Should().Throw<ArgumentNullException>();
			withoutLegacy.Should().Throw<ArgumentNullException>();
		}

		private AesGcmEngine CreateAesGcmEngine(string activeKeyId)
		{
			return new AesGcmEngine(new AesGcmSettings
			{
				ActiveKeyId = activeKeyId,
				Keys = new List<AesGcmKey> { new AesGcmKey { Id = KEY_ID, Key = _key } }
			});
		}
	}
}