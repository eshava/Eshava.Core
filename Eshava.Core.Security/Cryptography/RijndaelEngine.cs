using System.IO;
using System.Security.Cryptography;
using System.Text;
using Eshava.Core.Security.Cryptography.Constants;
using Eshava.Core.Security.Cryptography.Interfaces;
using Eshava.Core.Security.Cryptography.Models;

namespace Eshava.Core.Security.Cryptography
{
	public class RijndaelEngine : ISymmetricCryptographyEngine
	{
		private readonly SymmetricCryptographySettings _settings;
		
		public RijndaelEngine(SymmetricCryptographySettings settings)
		{
			_settings = settings;
		}

		public string Type => SymmetricCryptographyEngineConstants.RIJNDAEL;

		public (byte[] IV, byte[] Key) GenerateKeys(bool overwriteInstanceKeys)
		{
			using (var aes = Aes.Create())
			{
				Configurate(aes);

				aes.GenerateIV();
				aes.GenerateKey();

				if (overwriteInstanceKeys)
				{
					_settings.IV = aes.IV;
					_settings.Key = aes.Key;
				}

				return (aes.IV, aes.Key);
			}
		}

		public byte[] Encrypt(string input)
		{
			byte[] encryptedData;

			using (var aes = Aes.Create())
			{
				Configurate(aes);

				using (var inputStream = new MemoryStream(Encoding.Unicode.GetBytes(input)))
				using (var outputStream = new MemoryStream())
				{
					using (var cryptStream = new CryptoStream(outputStream, aes.CreateEncryptor(_settings.Key, _settings.IV), CryptoStreamMode.Write))
					{
						var buffer = new byte[1024];
						var read = inputStream.Read(buffer, 0, buffer.Length);
						while (read > 0)
						{
							cryptStream.Write(buffer, 0, read);
							read = inputStream.Read(buffer, 0, buffer.Length);
						}

						cryptStream.FlushFinalBlock();
						encryptedData = outputStream.ToArray();
					}
				}
			}

			return encryptedData;
		}

		public string Decrypt(byte[] encryptedData)
		{
			string output;

			using (var aes = Aes.Create())
			{
				Configurate(aes);

				using (var inputStream = new MemoryStream(encryptedData))
				using (var outputStream = new MemoryStream())
				{
					using (var cryptStream = new CryptoStream(inputStream, aes.CreateDecryptor(_settings.Key, _settings.IV), CryptoStreamMode.Read))
					{
						var buffer = new byte[1024];
						var read = cryptStream.Read(buffer, 0, buffer.Length);
						while (read > 0)
						{
							outputStream.Write(buffer, 0, read);
							read = cryptStream.Read(buffer, 0, buffer.Length);
						}
						cryptStream.Flush();
						output = Encoding.Unicode.GetString(outputStream.ToArray());
					}
				}
			}

			return output;
		}

		private void Configurate(Aes aes)
		{
			aes.Padding = _settings.PaddingMode;
			aes.KeySize = _settings.KeySize;
			aes.Mode = _settings.CipherMode;
		}
	}
}