#if NET8_0_OR_GREATER
using System;
using Eshava.Core.Security.Cryptography.Constants;
using Eshava.Core.Security.Cryptography.Interfaces;

namespace Eshava.Core.Security.Cryptography
{
	/// <summary>
	/// For the move from <see cref="RijndaelEngine"/> to <see cref="AesGcmEngine"/>:
	/// reads a value in the AES-GCM format through AES-GCM and anything else through the legacy engine,
	/// and writes through AES-GCM once an active key id is set, through the legacy engine until then.
	/// </summary>
	public class MigratingCryptographyEngine : ISymmetricCryptographyEngine
	{
		private readonly AesGcmEngine _aesGcmEngine;
		private readonly ISymmetricCryptographyEngine _legacyEngine;

		public MigratingCryptographyEngine(ISymmetricCryptographyEngine aesGcmEngine, ISymmetricCryptographyEngine legacyEngine)
		{
			if (aesGcmEngine is null)
			{
				throw new ArgumentNullException(nameof(aesGcmEngine));
			}

			if (legacyEngine is null)
			{
				throw new ArgumentNullException(nameof(legacyEngine));
			}

			if (aesGcmEngine.Type != SymmetricCryptographyEngineConstants.AESGCM || aesGcmEngine is not AesGcmEngine engine)
			{
				throw new ArgumentException($"The writing engine has to be the {nameof(AesGcmEngine)}, not '{aesGcmEngine.Type}'.", nameof(aesGcmEngine));
			}

			if (legacyEngine.Type != SymmetricCryptographyEngineConstants.RIJNDAEL)
			{
				throw new ArgumentException($"The legacy engine has to be of type '{SymmetricCryptographyEngineConstants.RIJNDAEL}', not '{legacyEngine.Type}'.", nameof(legacyEngine));
			}

			_aesGcmEngine = engine;
			_legacyEngine = legacyEngine;
		}

		public string Type => SymmetricCryptographyEngineConstants.MIGRATING;

		public (byte[] IV, byte[] Key) GenerateKeys(bool overwriteInstanceKeys)
		{
			return _aesGcmEngine.GenerateKeys(overwriteInstanceKeys);
		}

		public byte[] Encrypt(string input)
		{
			return _aesGcmEngine.CanEncrypt
				? _aesGcmEngine.Encrypt(input)
				: _legacyEngine.Encrypt(input);
		}

		public string Decrypt(byte[] encryptedData)
		{
			return AesGcmEngine.IsAesGcmFormat(encryptedData)
				? _aesGcmEngine.Decrypt(encryptedData)
				: _legacyEngine.Decrypt(encryptedData);
		}
	}
}
#endif