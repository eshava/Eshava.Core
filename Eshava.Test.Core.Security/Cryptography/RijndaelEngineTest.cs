using System;
using System.IO;
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
	public class RijndaelEngineTest
	{
		private RijndaelEngine _classUnderTest;
		private SymmetricCryptographySettings _settings;

		[TestInitialize]
		public void Setup()
		{
			_settings = new SymmetricCryptographySettings();
			_classUnderTest = new RijndaelEngine(_settings);
		}

		[TestMethod]
		public void EncryptAndDecryptWithPaddingModeISO10126CipherModeCBCKeySize256Test()
		{
			// Arrange
			var input = "Darkwing Duck knows QuackFu";

			// Act
			(var iv, var key) = _classUnderTest.GenerateKeys(true);
			var encrypted = _classUnderTest.Encrypt(input);
			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			iv.Length.Should().Be(16);
			key.Length.Should().Be(32);

			_settings.IV.Should().BeEquivalentTo(iv);
			_settings.Key.Should().BeEquivalentTo(key);

			encrypted.Length.Should().Be(64);
			decrypted.Should().Be(input);
		}

		[TestMethod]
		public void EncryptAndDecryptWithPaddingModeISO10126CipherModeCBCKeySize256WrongEncryptedInputTest()
		{
			// Arrange
			var input = "Darkwing Duck knows QuackFu";
			var random = new Random();

			// Act
			(var iv, var key) = _classUnderTest.GenerateKeys(true);
			var encrypted = _classUnderTest.Encrypt(input);

			encrypted[0] = Convert.ToByte(random.Next(255));
			encrypted[1] = Convert.ToByte(random.Next(255));
			encrypted[2] = Convert.ToByte(random.Next(255));
			encrypted[3] = Convert.ToByte(random.Next(255));
			encrypted[4] = Convert.ToByte(random.Next(255));

			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			iv.Length.Should().Be(16);
			key.Length.Should().Be(32);

			_settings.IV.Should().BeEquivalentTo(iv);
			_settings.Key.Should().BeEquivalentTo(key);

			encrypted.Length.Should().Be(64);
			decrypted.Should().NotBe(input);
		}

		[TestMethod]
		public void EncryptAndDecryptWithPaddingModeISO10126CipherModeCBCKeySize256WrongIVTest()
		{
			// Arrange
			var input = "Darkwing Duck knows QuackFu";
			var random = new Random();

			// Act
			(var iv, var key) = _classUnderTest.GenerateKeys(true);
			var encrypted = _classUnderTest.Encrypt(input);
			_settings.IV[0] = Convert.ToByte(random.Next(255));
			_settings.IV[1] = Convert.ToByte(random.Next(255));
			_settings.IV[2] = Convert.ToByte(random.Next(255));
			_settings.IV[3] = Convert.ToByte(random.Next(255));
			_settings.IV[4] = Convert.ToByte(random.Next(255));

			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			iv.Length.Should().Be(16);
			key.Length.Should().Be(32);
			encrypted.Length.Should().Be(64);
			decrypted.Should().NotBe(input);
		}

		[TestMethod]
		public void EncryptAndDecryptWithPaddingModeISO10126CipherModeCBCKeySize256WrongKeyTest()
		{
			Assert.Throws<CryptographicException>(() =>
			{
				// Arrange
				var input = "Darkwing Duck knows QuackFu";
				var random = new Random();

				// Act
				_classUnderTest.GenerateKeys(true);
				var encrypted = _classUnderTest.Encrypt(input);
				_settings.Key[1] = Convert.ToByte(random.Next(255));
				_settings.Key[2] = Convert.ToByte(random.Next(255));
				_settings.Key[3] = Convert.ToByte(random.Next(255));
				_settings.Key[4] = Convert.ToByte(random.Next(255));
				_settings.Key[5] = Convert.ToByte(random.Next(255));
				_classUnderTest.Decrypt(encrypted);
			});
		}

		[TestMethod]
		public void EncryptAndDecryptWithPaddingModePKCS7CipherModeECBKeySize128Test()
		{
			// Arrange
			var input = "Darkwing Duck knows QuackFu";
			_settings.CipherMode = CipherMode.ECB;
			_settings.PaddingMode = PaddingMode.PKCS7;
			_settings.KeySize = 128;

			// Act
			(var iv, var key) = _classUnderTest.GenerateKeys(true);
			var encrypted = _classUnderTest.Encrypt(input);
			var decrypted = _classUnderTest.Decrypt(encrypted);

			// Assert
			iv.Length.Should().Be(16);
			key.Length.Should().Be(16);
			encrypted.Length.Should().Be(64);
			decrypted.Should().Be(input);
		}

		[TestMethod]
		public void TypeIsRijndaelTest()
		{
			// Act & Assert
			_classUnderTest.Type.Should().Be(SymmetricCryptographyEngineConstants.RIJNDAEL);
			_classUnderTest.Type.Should().Be("Rijndael");
		}

		[TestMethod]
		public void ValuesOfRijndaelManagedAreReadAndReadableTest()
		{
			// Arrange
			_classUnderTest.GenerateKeys(true);

			for (var length = 0; length < 70; length++)
			{
				var input = new string('x', length) + "Quack";

				// Act
				var fromRijndaelManaged = EncryptWithRijndaelManaged(input);
				var fromEngine = _classUnderTest.Encrypt(input);

				// Assert
				_classUnderTest.Decrypt(fromRijndaelManaged).Should().Be(input);
				DecryptWithRijndaelManaged(fromEngine).Should().Be(input);
			}
		}

#pragma warning disable SYSLIB0022 // the obsolete implementation is the reference the engine has to stay compatible with
		private byte[] EncryptWithRijndaelManaged(string input)
		{
			using (var rijndaelManaged = CreateRijndaelManaged())
			using (var outputStream = new MemoryStream())
			{
				using (var cryptStream = new CryptoStream(outputStream, rijndaelManaged.CreateEncryptor(_settings.Key, _settings.IV), CryptoStreamMode.Write))
				{
					var bytes = Encoding.Unicode.GetBytes(input);
					cryptStream.Write(bytes, 0, bytes.Length);
					cryptStream.FlushFinalBlock();

					return outputStream.ToArray();
				}
			}
		}

		private string DecryptWithRijndaelManaged(byte[] encryptedData)
		{
			using (var rijndaelManaged = CreateRijndaelManaged())
			using (var inputStream = new MemoryStream(encryptedData))
			using (var outputStream = new MemoryStream())
			{
				using (var cryptStream = new CryptoStream(inputStream, rijndaelManaged.CreateDecryptor(_settings.Key, _settings.IV), CryptoStreamMode.Read))
				{
					cryptStream.CopyTo(outputStream);

					return Encoding.Unicode.GetString(outputStream.ToArray());
				}
			}
		}

		private RijndaelManaged CreateRijndaelManaged()
		{
			return new RijndaelManaged
			{
				Padding = _settings.PaddingMode,
				KeySize = _settings.KeySize,
				Mode = _settings.CipherMode
			};
		}
#pragma warning restore SYSLIB0022
	}
}