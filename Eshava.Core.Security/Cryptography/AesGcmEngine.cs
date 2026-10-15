#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Eshava.Core.Security.Cryptography.Constants;
using Eshava.Core.Security.Cryptography.Interfaces;
using Eshava.Core.Security.Cryptography.Models;

namespace Eshava.Core.Security.Cryptography
{
	/// <summary>
	/// AES-GCM with a random nonce per value and a key id stored in every value.
	/// Layout: magic (3 bytes, base64 "ESG1"), key id length (1 byte), key id (ASCII), nonce (12 bytes), ciphertext, tag (16 bytes).
	/// Magic, length and key id are authenticated as associated data.
	/// </summary>
	public class AesGcmEngine : ISymmetricCryptographyEngine
	{
		private const int KEY_SIZE = 32;
		private const int NONCE_SIZE = 12;
		private const int TAG_SIZE = 16;
		private const int KEY_ID_MAX_LENGTH = 16;
		private static readonly byte[] _magic = { 0x11, 0x21, 0xB5 };

		private readonly Dictionary<string, byte[]> _keys;
		private readonly string _activeKeyId;

		public AesGcmEngine(AesGcmSettings settings)
		{
			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_keys = new Dictionary<string, byte[]>(StringComparer.Ordinal);
			foreach (var key in settings.Keys ?? new List<AesGcmKey>())
			{
				if (!IsValidKeyId(key.Id))
				{
					throw new ArgumentException($"The key id '{key.Id}' is invalid. Allowed are 1 to {KEY_ID_MAX_LENGTH} lower-case letters and digits.", nameof(settings));
				}

				if (_keys.ContainsKey(key.Id))
				{
					throw new ArgumentException($"The key id '{key.Id}' is given more than once.", nameof(settings));
				}

				_keys.Add(key.Id, DecodeKey(key));
			}

			if (!String.IsNullOrEmpty(settings.ActiveKeyId) && !_keys.ContainsKey(settings.ActiveKeyId))
			{
				throw new ArgumentException($"The active key id '{settings.ActiveKeyId}' names no key.", nameof(settings));
			}

			_activeKeyId = String.IsNullOrEmpty(settings.ActiveKeyId) ? null : settings.ActiveKeyId;
		}

		public string Type => SymmetricCryptographyEngineConstants.AESGCM;

		/// <summary>
		/// Whether an active key is set, which is what encrypting requires.
		/// </summary>
		public bool CanEncrypt => _activeKeyId is not null;

		/// <summary>
		/// Whether the value carries the header of this engine's format.
		/// Says nothing about whether it can be decrypted.
		/// </summary>
		public static bool IsAesGcmFormat(byte[] encryptedData)
		{
			return encryptedData is not null
				&& encryptedData.Length >= _magic.Length
				&& encryptedData.AsSpan(0, _magic.Length).SequenceEqual(_magic);
		}

		/// <summary>
		/// Returns a fresh 256-bit key and an empty IV. The settings are never changed, whatever <paramref name="overwriteInstanceKeys"/> says:
		/// a key ring is not one key, so a new key is added to the configuration under an id of its own.
		/// </summary>
		public (byte[] IV, byte[] Key) GenerateKeys(bool overwriteInstanceKeys)
		{
			return (Array.Empty<byte>(), RandomNumberGenerator.GetBytes(KEY_SIZE));
		}

		public byte[] Encrypt(string input)
		{
			if (input is null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			if (_activeKeyId is null)
			{
				throw new InvalidOperationException("No active key id is set, so nothing can be encrypted.");
			}

			var keyId = Encoding.ASCII.GetBytes(_activeKeyId);
			var plaintext = Encoding.Unicode.GetBytes(input);
			var headerLength = _magic.Length + 1 + keyId.Length;
			var output = new byte[headerLength + NONCE_SIZE + plaintext.Length + TAG_SIZE];

			_magic.CopyTo(output, 0);
			output[_magic.Length] = (byte)keyId.Length;
			keyId.CopyTo(output, _magic.Length + 1);

			var header = output.AsSpan(0, headerLength);
			var nonce = output.AsSpan(headerLength, NONCE_SIZE);
			var ciphertext = output.AsSpan(headerLength + NONCE_SIZE, plaintext.Length);
			var tag = output.AsSpan(headerLength + NONCE_SIZE + plaintext.Length, TAG_SIZE);

			RandomNumberGenerator.Fill(nonce);

			using (var aesGcm = new AesGcm(_keys[_activeKeyId], TAG_SIZE))
			{
				aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, header);
			}

			return output;
		}

		public string Decrypt(byte[] encryptedData)
		{
			if (encryptedData is null)
			{
				throw new ArgumentNullException(nameof(encryptedData));
			}

			if (!IsAesGcmFormat(encryptedData) || encryptedData.Length < _magic.Length + 1)
			{
				throw new CryptographicException("The value is not in the AES-GCM format.");
			}

			var keyIdLength = encryptedData[_magic.Length];
			var headerLength = _magic.Length + 1 + keyIdLength;
			if (keyIdLength < 1 || keyIdLength > KEY_ID_MAX_LENGTH || encryptedData.Length < headerLength + NONCE_SIZE + TAG_SIZE)
			{
				throw new CryptographicException("The value is not in the AES-GCM format.");
			}

			var keyId = Encoding.ASCII.GetString(encryptedData, _magic.Length + 1, keyIdLength);
			if (!_keys.TryGetValue(keyId, out var key))
			{
				throw new CryptographicException($"The value names the key id '{(IsValidKeyId(keyId) ? keyId : "?")}', which is not configured.");
			}

			var ciphertextLength = encryptedData.Length - headerLength - NONCE_SIZE - TAG_SIZE;
			var header = encryptedData.AsSpan(0, headerLength);
			var nonce = encryptedData.AsSpan(headerLength, NONCE_SIZE);
			var ciphertext = encryptedData.AsSpan(headerLength + NONCE_SIZE, ciphertextLength);
			var tag = encryptedData.AsSpan(headerLength + NONCE_SIZE + ciphertextLength, TAG_SIZE);
			var plaintext = new byte[ciphertextLength];

			using (var aesGcm = new AesGcm(key, TAG_SIZE))
			{
				aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, header);
			}

			return Encoding.Unicode.GetString(plaintext);
		}

		private static bool IsValidKeyId(string keyId)
		{
			return !String.IsNullOrEmpty(keyId)
				&& keyId.Length <= KEY_ID_MAX_LENGTH
				&& keyId.All(character => (character >= 'a' && character <= 'z') || (character >= '0' && character <= '9'));
		}

		private static byte[] DecodeKey(AesGcmKey key)
		{
			byte[] bytes;
			try
			{
				bytes = Convert.FromBase64String(key.Key ?? String.Empty);
			}
			catch (FormatException)
			{
				throw new ArgumentException($"The key '{key.Id}' is not valid base64.");
			}

			if (bytes.Length != KEY_SIZE)
			{
				throw new ArgumentException($"The key '{key.Id}' has {bytes.Length * 8} bits instead of {KEY_SIZE * 8}.");
			}

			return bytes;
		}
	}
}
#endif