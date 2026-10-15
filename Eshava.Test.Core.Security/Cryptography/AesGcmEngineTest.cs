using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Eshava.Core.Security.Cryptography;
using Eshava.Core.Security.Cryptography.Constants;
using Eshava.Core.Security.Cryptography.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.Core.Security.Cryptography
{
	[TestClass, TestCategory("Core.Security")]
	public class AesGcmEngineTest
	{
		private const string INPUT = "Darkwing Duck knows QuackFu";
		private const string KEY_ID = "k202610";

		private AesGcmEngine _classUnderTest;
		private AesGcmSettings _settings;

		[TestInitialize]
		public void Setup()
		{
			_settings = CreateSettings(KEY_ID, (KEY_ID, CreateKey()));
			_classUnderTest = new AesGcmEngine(_settings);
		}

		[TestMethod]
		public void TypeIsAesGcmTest()
		{
			// Act & Assert
			_classUnderTest.Type.Should().Be(SymmetricCryptographyEngineConstants.AESGCM);
		}

		[TestMethod]
		public void EncryptAndDecryptTest()
		{
			// Act
			var encrypted = _classUnderTest.Encrypt(INPUT);
			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			decrypted.Should().Be(INPUT);
			Convert.ToBase64String(encrypted).Should().StartWith("ESG1");
			encrypted.Length.Should().Be(3 + 1 + KEY_ID.Length + 12 + Encoding.Unicode.GetByteCount(INPUT) + 16);
			Encoding.ASCII.GetString(encrypted, 4, KEY_ID.Length).Should().Be(KEY_ID);
			AesGcmEngine.IsAesGcmFormat(encrypted).Should().BeTrue();
		}

		[TestMethod]
		public void EncryptAndDecryptEmptyStringTest()
		{
			// Act
			var encrypted = _classUnderTest.Encrypt(String.Empty);
			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			decrypted.Should().BeEmpty();
		}

		[TestMethod]
		public void EncryptTwiceGivesDifferentValuesTest()
		{
			// Act
			var first = _classUnderTest.Encrypt(INPUT);
			var second = _classUnderTest.Encrypt(INPUT);

			// Assert
			var headerLength = 4 + KEY_ID.Length;
			first.AsSpan(0, headerLength).SequenceEqual(second.AsSpan(0, headerLength)).Should().BeTrue();
			first.AsSpan(headerLength).SequenceEqual(second.AsSpan(headerLength)).Should().BeFalse();
		}

		[TestMethod]
		public void DecryptRejectsEveryFlippedBitTest()
		{
			// Arrange
			var encrypted = _classUnderTest.Encrypt(INPUT);
			var rejected = 0;

			// Act
			for (var index = 0; index < encrypted.Length * 8; index++)
			{
				var tampered = (byte[])encrypted.Clone();
				tampered[index / 8] ^= (byte)(1 << (index % 8));

				try
				{
					_classUnderTest.Decrypt(tampered);
				}
				catch (CryptographicException)
				{
					rejected++;
				}
			}

			// Assert
			rejected.Should().Be(encrypted.Length * 8);
		}

		[TestMethod]
		public void DecryptWithOlderKeyAfterRotationTest()
		{
			// Arrange
			var oldKey = CreateKey();
			var oldEngine = new AesGcmEngine(CreateSettings("k202610", ("k202610", oldKey)));
			var rotatedEngine = new AesGcmEngine(CreateSettings("k202710", ("k202610", oldKey), ("k202710", CreateKey())));

			// Act
			var encryptedWithOldKey = oldEngine.Encrypt(INPUT);
			var encryptedWithNewKey = rotatedEngine.Encrypt(INPUT);

			// Assert
			rotatedEngine.Decrypt(encryptedWithOldKey).Should().Be(INPUT);
			rotatedEngine.Decrypt(encryptedWithNewKey).Should().Be(INPUT);
			Encoding.ASCII.GetString(encryptedWithNewKey, 4, 7).Should().Be("k202710");
		}

		[TestMethod]
		public void DecryptWithUnknownKeyIdThrowsTest()
		{
			// Arrange
			var otherEngine = new AesGcmEngine(CreateSettings("k202710", ("k202710", CreateKey())));
			var encrypted = otherEngine.Encrypt(INPUT);

			// Act
			var act = () => _classUnderTest.Decrypt(encrypted);

			// Assert
			act.Should().Throw<CryptographicException>().WithMessage("*k202710*");
		}

		[TestMethod]
		public void DecryptWithWrongKeyUnderSameIdThrowsTest()
		{
			// Arrange
			var otherEngine = new AesGcmEngine(CreateSettings(KEY_ID, (KEY_ID, CreateKey())));
			var encrypted = otherEngine.Encrypt(INPUT);

			// Act
			var act = () => _classUnderTest.Decrypt(encrypted);

			// Assert
			act.Should().Throw<CryptographicException>();
		}

		[TestMethod]
		public void DecryptValueWithoutHeaderThrowsTest()
		{
			// Arrange
			var legacy = new byte[64];
			RandomNumberGenerator.Fill(legacy);
			legacy[0] = 0x00;

			// Act
			var act = () => _classUnderTest.Decrypt(legacy);

			// Assert
			AesGcmEngine.IsAesGcmFormat(legacy).Should().BeFalse();
			act.Should().Throw<CryptographicException>();
		}

		[TestMethod]
		public void DecryptTruncatedValueThrowsTest()
		{
			// Arrange
			var encrypted = _classUnderTest.Encrypt(INPUT);

			// Act
			var act = () => _classUnderTest.Decrypt(encrypted.AsSpan(0, 4 + KEY_ID.Length + 12 + 15).ToArray());

			// Assert
			act.Should().Throw<CryptographicException>();
		}

		[TestMethod]
		public void EncryptWithoutActiveKeyIdThrowsTest()
		{
			// Arrange
			var engine = new AesGcmEngine(CreateSettings(null, (KEY_ID, CreateKey())));

			// Act
			var act = () => engine.Encrypt(INPUT);

			// Assert
			engine.CanEncrypt.Should().BeFalse();
			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void ConstructWithoutKeysTest()
		{
			// Act
			var engine = new AesGcmEngine(new AesGcmSettings());

			// Assert
			engine.CanEncrypt.Should().BeFalse();
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow("K202610")]
		[DataRow("k-202610")]
		[DataRow("k2026100000000000")]
		public void ConstructWithInvalidKeyIdThrowsTest(string keyId)
		{
			// Act
			var act = () => new AesGcmEngine(CreateSettings(null, (keyId, CreateKey())));

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void ConstructWithDuplicateKeyIdThrowsTest()
		{
			// Act
			var act = () => new AesGcmEngine(CreateSettings(KEY_ID, (KEY_ID, CreateKey()), (KEY_ID, CreateKey())));

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void ConstructWithUnknownActiveKeyIdThrowsTest()
		{
			// Act
			var act = () => new AesGcmEngine(CreateSettings("k202710", (KEY_ID, CreateKey())));

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[DataTestMethod]
		[DataRow("not base64")]
		[DataRow("AAAAAAAAAAAAAAAAAAAAAA==")]
		public void ConstructWithInvalidKeyThrowsTest(string key)
		{
			// Act
			var act = () => new AesGcmEngine(CreateSettings(KEY_ID, (KEY_ID, key)));

			// Assert
			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void GenerateKeysLeavesSettingsUntouchedTest()
		{
			// Arrange
			var keyBefore = _settings.Keys[0].Key;

			// Act
			(var iv, var key) = _classUnderTest.GenerateKeys(true);

			// Assert
			iv.Should().BeEmpty();
			key.Length.Should().Be(32);
			_settings.Keys.Should().HaveCount(1);
			_settings.Keys[0].Key.Should().Be(keyBefore);
		}

		private static string CreateKey()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		}

		private static AesGcmSettings CreateSettings(string activeKeyId, params (string Id, string Key)[] keys)
		{
			var settings = new AesGcmSettings
			{
				ActiveKeyId = activeKeyId,
				Keys = new List<AesGcmKey>()
			};

			foreach (var key in keys)
			{
				settings.Keys.Add(new AesGcmKey { Id = key.Id, Key = key.Key });
			}

			return settings;
		}
	}
}